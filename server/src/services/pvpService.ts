import { pool } from '../db/connection';
import { PVP_LEADERBOARD_KEY, redisClient } from '../db/redis';
import { ApiError } from '../utils/ApiError';
import { calculateEloChange, getRankTierForMmr } from './eloService';

const OPPONENT_MMR_RANGE = 150;
const OPPONENT_COUNT = 3;
const LEADERBOARD_SIZE = 100;

export interface DefenseTeamInput {
  leaderId: string;
  team: unknown[];
  totalPower: number;
}

export async function setDefenseTeam(userId: string, input: DefenseTeamInput): Promise<void> {
  const { leaderId, team, totalPower } = input;

  if (typeof leaderId !== 'string' || leaderId.trim().length === 0) {
    throw ApiError.badRequest('leaderId is required.');
  }
  if (!Array.isArray(team) || team.length === 0) {
    throw ApiError.badRequest('team must be a non-empty array.');
  }
  if (typeof totalPower !== 'number' || totalPower < 0) {
    throw ApiError.badRequest('totalPower must be a non-negative number.');
  }

  await pool.query(
    `INSERT INTO pvp_defense_teams (user_id, leader_id, team_json, total_power, updated_at)
     VALUES ($1, $2, $3, $4, now())
     ON CONFLICT (user_id)
     DO UPDATE SET leader_id = $2, team_json = $3, total_power = $4, updated_at = now()`,
    [userId, leaderId, JSON.stringify(team), totalPower],
  );
}

async function getOrCreateMatchmakingRow(userId: string): Promise<{ mmr: number }> {
  const existing = await pool.query('SELECT mmr FROM pvp_matchmaking WHERE user_id = $1', [userId]);
  if (existing.rows.length > 0) return existing.rows[0];

  const created = await pool.query('INSERT INTO pvp_matchmaking (user_id) VALUES ($1) RETURNING mmr', [userId]);
  return created.rows[0];
}

export async function findOpponents(userId: string): Promise<Array<{ userId: string; mmr: number; rankTier: string; totalPower: number }>> {
  const { mmr } = await getOrCreateMatchmakingRow(userId);

  const result = await pool.query(
    `SELECT m.user_id, m.mmr, m.rank_tier, COALESCE(d.total_power, 0) AS total_power
     FROM pvp_matchmaking m
     LEFT JOIN pvp_defense_teams d ON d.user_id = m.user_id
     WHERE m.user_id != $1
       AND m.mmr BETWEEN $2 AND $3
     ORDER BY random()
     LIMIT $4`,
    [userId, mmr - OPPONENT_MMR_RANGE, mmr + OPPONENT_MMR_RANGE, OPPONENT_COUNT],
  );

  return result.rows.map((row) => ({
    userId: row.user_id,
    mmr: row.mmr,
    rankTier: row.rank_tier,
    totalPower: row.total_power,
  }));
}

export interface SubmitResultInput {
  defenderId: string;
  attackerWon: boolean;
}

export interface SubmitResultOutcome {
  attackerMmr: number;
  defenderMmr: number;
  attackerRankTier: string;
  defenderRankTier: string;
}

// Recomputes both sides' MMR via Elo, persists wins/losses + rank tier, and keeps the Redis
// leaderboard sorted set (used by GET /api/pvp/leaderboard) in sync with the new ratings.
export async function submitMatchResult(attackerId: string, input: SubmitResultInput): Promise<SubmitResultOutcome> {
  const { defenderId, attackerWon } = input;

  if (typeof defenderId !== 'string' || defenderId.trim().length === 0) {
    throw ApiError.badRequest('defenderId is required.');
  }
  if (typeof attackerWon !== 'boolean') {
    throw ApiError.badRequest('attackerWon must be a boolean.');
  }
  if (defenderId === attackerId) {
    throw ApiError.badRequest('Cannot submit a match result against yourself.');
  }

  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    const attackerRow = await client.query('SELECT mmr FROM pvp_matchmaking WHERE user_id = $1 FOR UPDATE', [attackerId]);
    const defenderRow = await client.query('SELECT mmr FROM pvp_matchmaking WHERE user_id = $1 FOR UPDATE', [defenderId]);

    if (attackerRow.rows.length === 0 || defenderRow.rows.length === 0) {
      throw ApiError.notFound('Attacker or defender has no matchmaking record.');
    }

    const attackerMmrBefore = attackerRow.rows[0].mmr;
    const defenderMmrBefore = defenderRow.rows[0].mmr;

    const { newAttackerMmr, newDefenderMmr } = calculateEloChange(attackerMmrBefore, defenderMmrBefore, attackerWon);
    const attackerRankTier = getRankTierForMmr(newAttackerMmr);
    const defenderRankTier = getRankTierForMmr(newDefenderMmr);

    await client.query(
      `UPDATE pvp_matchmaking
       SET mmr = $1, rank_tier = $2, wins = wins + $3, losses = losses + $4
       WHERE user_id = $5`,
      [newAttackerMmr, attackerRankTier, attackerWon ? 1 : 0, attackerWon ? 0 : 1, attackerId],
    );
    await client.query(
      `UPDATE pvp_matchmaking
       SET mmr = $1, rank_tier = $2, wins = wins + $3, losses = losses + $4
       WHERE user_id = $5`,
      [newDefenderMmr, defenderRankTier, attackerWon ? 0 : 1, attackerWon ? 1 : 0, defenderId],
    );

    await client.query('COMMIT');

    await redisClient.zadd(PVP_LEADERBOARD_KEY, newAttackerMmr, attackerId);
    await redisClient.zadd(PVP_LEADERBOARD_KEY, newDefenderMmr, defenderId);

    return { attackerMmr: newAttackerMmr, defenderMmr: newDefenderMmr, attackerRankTier, defenderRankTier };
  } catch (err) {
    await client.query('ROLLBACK');
    throw err;
  } finally {
    client.release();
  }
}

export interface LeaderboardEntry {
  rank: number;
  userId: string;
  mmr: number;
}

// Reads the Top 100 straight out of a Redis sorted set (ZREVRANGE ... WITHSCORES) instead of
// hitting Postgres, so the leaderboard stays fast under read-heavy traffic.
export async function getLeaderboard(): Promise<LeaderboardEntry[]> {
  const raw = await redisClient.zrevrange(PVP_LEADERBOARD_KEY, 0, LEADERBOARD_SIZE - 1, 'WITHSCORES');

  const entries: LeaderboardEntry[] = [];
  for (let i = 0; i < raw.length; i += 2) {
    entries.push({
      rank: i / 2 + 1,
      userId: raw[i],
      mmr: Number(raw[i + 1]),
    });
  }
  return entries;
}
