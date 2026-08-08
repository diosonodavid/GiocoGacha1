import { NextFunction, Request, Response } from 'express';

process.env.JWT_ACCESS_SECRET = 'test-access-secret';
process.env.JWT_REFRESH_SECRET = 'test-refresh-secret';

const mockClient = {
  query: jest.fn(),
  release: jest.fn(),
};

const mockPool = {
  query: jest.fn(),
  connect: jest.fn().mockResolvedValue(mockClient),
};

const mockRedisClient = {
  incr: jest.fn().mockResolvedValue(1),
  expire: jest.fn().mockResolvedValue(1),
  zadd: jest.fn().mockResolvedValue(1),
  zrevrange: jest.fn().mockResolvedValue([]),
};

jest.mock('../src/db/connection', () => ({ pool: mockPool }));
jest.mock('../src/db/redis', () => ({
  redisClient: mockRedisClient,
  PVP_LEADERBOARD_KEY: 'pvp:leaderboard',
}));

/* eslint-disable @typescript-eslint/no-var-requires */
const request = require('supertest');
const { app } = require('../src/app');
const { signAccessToken } = require('../src/utils/jwt');
const { calculateEloChange, getRankTierForMmr } = require('../src/services/eloService');
const { rateLimiter } = require('../src/middleware/rateLimiter');
/* eslint-enable @typescript-eslint/no-var-requires */

beforeEach(() => {
  jest.clearAllMocks();
  mockPool.connect.mockResolvedValue(mockClient);
  mockRedisClient.incr.mockResolvedValue(1);
  mockRedisClient.expire.mockResolvedValue(1);
});

describe('Auth: POST /api/auth/register-guest', () => {
  it('creates a new guest account and returns a token pair', async () => {
    mockClient.query
      .mockResolvedValueOnce({}) // BEGIN
      .mockResolvedValueOnce({ rows: [] }) // SELECT existing device
      .mockResolvedValueOnce({ rows: [{ id: 'user-1' }] }) // INSERT users
      .mockResolvedValueOnce({}) // INSERT player_profiles
      .mockResolvedValueOnce({}) // INSERT pvp_matchmaking
      .mockResolvedValueOnce({}); // COMMIT

    const res = await request(app).post('/api/auth/register-guest').send({ deviceId: 'device-abc' });

    expect(res.status).toBe(201);
    expect(res.body.success).toBe(true);
    expect(res.body.data.userId).toBe('user-1');
    expect(res.body.data.accessToken).toEqual(expect.any(String));
    expect(res.body.data.refreshToken).toEqual(expect.any(String));
  });

  it('rejects a device that already has a guest account', async () => {
    mockClient.query
      .mockResolvedValueOnce({}) // BEGIN
      .mockResolvedValueOnce({ rows: [{ id: 'existing-user' }] }) // SELECT existing device
      .mockResolvedValueOnce({}); // ROLLBACK

    const res = await request(app).post('/api/auth/register-guest').send({ deviceId: 'device-abc' });

    expect(res.status).toBe(409);
    expect(res.body.success).toBe(false);
  });
});

describe('Auth: POST /api/auth/login', () => {
  it('logs in a returning guest by deviceId', async () => {
    mockPool.query.mockResolvedValueOnce({
      rows: [{ id: 'user-1', device_id: 'device-abc', email: null, password_hash: null, is_banned: false }],
    });

    const res = await request(app).post('/api/auth/login').send({ deviceId: 'device-abc' });

    expect(res.status).toBe(200);
    expect(res.body.data.userId).toBe('user-1');
  });

  it('rejects unknown credentials', async () => {
    mockPool.query.mockResolvedValueOnce({ rows: [] });

    const res = await request(app).post('/api/auth/login').send({ deviceId: 'no-such-device' });

    expect(res.status).toBe(401);
  });

  it('rejects a banned account', async () => {
    mockPool.query.mockResolvedValueOnce({
      rows: [{ id: 'user-1', device_id: 'device-abc', email: null, password_hash: null, is_banned: true }],
    });

    const res = await request(app).post('/api/auth/login').send({ deviceId: 'device-abc' });

    expect(res.status).toBe(403);
  });

  it('requires deviceId or email+password', async () => {
    const res = await request(app).post('/api/auth/login').send({});
    expect(res.status).toBe(400);
    expect(mockPool.query).not.toHaveBeenCalled();
  });
});

describe('Player: sync and profile', () => {
  const token = signAccessToken('user-1');

  it('rejects sync without an Authorization header', async () => {
    const res = await request(app).post('/api/player/sync').send({});
    expect(res.status).toBe(401);
  });

  it('rejects an invalid player state before touching the database', async () => {
    const res = await request(app)
      .post('/api/player/sync')
      .set('Authorization', `Bearer ${token}`)
      .send({ level: 1, exp: 0, gold: -50, gems: 0, stamina: 10, maxStamina: 120 });

    expect(res.status).toBe(400);
    expect(mockPool.query).not.toHaveBeenCalled();
  });

  it('persists a valid player state', async () => {
    mockPool.query.mockResolvedValueOnce({
      rows: [{ user_id: 'user-1', level: 5, exp: 10, gold: 500, gems: 20, stamina: 100, max_stamina: 120, mmr: 1000 }],
    });

    const res = await request(app)
      .post('/api/player/sync')
      .set('Authorization', `Bearer ${token}`)
      .send({ level: 5, exp: 10, gold: 500, gems: 20, stamina: 100, maxStamina: 120 });

    expect(res.status).toBe(200);
    expect(res.body.data.gold).toBe(500);
  });

  it('returns the joined profile', async () => {
    mockPool.query.mockResolvedValueOnce({
      rows: [
        {
          user_id: 'user-1',
          device_id: 'device-abc',
          email: null,
          created_at: '2026-01-01T00:00:00.000Z',
          is_banned: false,
          level: 5,
          exp: 10,
          gold: 500,
          gems: 20,
          stamina: 100,
          max_stamina: 120,
          mmr: 1000,
        },
      ],
    });

    const res = await request(app).get('/api/player/profile').set('Authorization', `Bearer ${token}`);

    expect(res.status).toBe(200);
    expect(res.body.data.deviceId).toBe('device-abc');
  });
});

describe('PvP: Elo system', () => {
  it('calculates symmetric, zero-sum rating changes for an even matchup', () => {
    const { newAttackerMmr, newDefenderMmr } = calculateEloChange(1000, 1000, true);
    expect(newAttackerMmr).toBe(1016);
    expect(newDefenderMmr).toBe(984);
  });

  it('awards fewer points for beating a much weaker opponent', () => {
    const evenMatch = calculateEloChange(1000, 1000, true);
    const lopsidedMatch = calculateEloChange(1400, 1000, true);
    const evenGain = evenMatch.newAttackerMmr - 1000;
    const lopsidedGain = lopsidedMatch.newAttackerMmr - 1400;
    expect(lopsidedGain).toBeLessThan(evenGain);
  });

  it('maps mmr thresholds to the expected rank tier', () => {
    expect(getRankTierForMmr(0)).toBe('Bronze');
    expect(getRankTierForMmr(1000)).toBe('Gold');
    expect(getRankTierForMmr(2200)).toBe('Grandmaster');
  });

  it('POST /api/pvp/submit-result updates both MMRs and syncs the Redis leaderboard', async () => {
    const token = signAccessToken('user-1');
    mockClient.query
      .mockResolvedValueOnce({}) // BEGIN
      .mockResolvedValueOnce({ rows: [{ mmr: 1000 }] }) // attacker SELECT
      .mockResolvedValueOnce({ rows: [{ mmr: 1000 }] }) // defender SELECT
      .mockResolvedValueOnce({}) // UPDATE attacker
      .mockResolvedValueOnce({}) // UPDATE defender
      .mockResolvedValueOnce({}); // COMMIT

    const res = await request(app)
      .post('/api/pvp/submit-result')
      .set('Authorization', `Bearer ${token}`)
      .send({ defenderId: 'user-2', attackerWon: true });

    expect(res.status).toBe(200);
    expect(res.body.data.attackerMmr).toBeGreaterThan(1000);
    expect(res.body.data.defenderMmr).toBeLessThan(1000);
    expect(mockRedisClient.zadd).toHaveBeenCalledTimes(2);
  });

  it('rejects submitting a result against yourself without touching the database', async () => {
    const token = signAccessToken('user-1');
    const res = await request(app)
      .post('/api/pvp/submit-result')
      .set('Authorization', `Bearer ${token}`)
      .send({ defenderId: 'user-1', attackerWon: true });

    expect(res.status).toBe(400);
    expect(mockPool.connect).not.toHaveBeenCalled();
  });

  it('GET /api/pvp/leaderboard reads the Top N from the Redis sorted set', async () => {
    mockRedisClient.zrevrange.mockResolvedValueOnce(['user-1', '1200', 'user-2', '1000']);

    const res = await request(app).get('/api/pvp/leaderboard');

    expect(res.status).toBe(200);
    expect(res.body.data).toEqual([
      { rank: 1, userId: 'user-1', mmr: 1200 },
      { rank: 2, userId: 'user-2', mmr: 1000 },
    ]);
  });
});

describe('Middleware: rateLimiter', () => {
  it('blocks a request once the per-second IP count exceeds 10', async () => {
    mockRedisClient.incr.mockResolvedValueOnce(11);
    const next = jest.fn() as unknown as NextFunction;
    const req = { ip: '1.2.3.4', socket: { remoteAddress: '1.2.3.4' } } as unknown as Request;

    await rateLimiter(req, {} as Response, next);

    expect(next).toHaveBeenCalledTimes(1);
    const errArg = (next as jest.Mock).mock.calls[0][0];
    expect(errArg.statusCode).toBe(429);
  });

  it('fails open when Redis is unreachable', async () => {
    mockRedisClient.incr.mockRejectedValueOnce(new Error('connection refused'));
    const next = jest.fn() as unknown as NextFunction;
    const req = { ip: '1.2.3.4', socket: { remoteAddress: '1.2.3.4' } } as unknown as Request;

    await rateLimiter(req, {} as Response, next);

    expect(next).toHaveBeenCalledWith();
  });
});
