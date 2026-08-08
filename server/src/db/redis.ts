import dotenv from 'dotenv';
import Redis from 'ioredis';

dotenv.config();

export const redisClient = new Redis(process.env.REDIS_URL || 'redis://localhost:6379', {
  lazyConnect: true,
  maxRetriesPerRequest: 3,
});

redisClient.on('error', (err) => {
  // eslint-disable-next-line no-console
  console.error('Unexpected Redis client error', err);
});

export const PVP_LEADERBOARD_KEY = 'pvp:leaderboard';
