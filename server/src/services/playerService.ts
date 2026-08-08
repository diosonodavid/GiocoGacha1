import { pool } from '../db/connection';
import { ApiError } from '../utils/ApiError';

export interface PlayerSyncInput {
  level: number;
  exp: number;
  gold: number;
  gems: number;
  stamina: number;
  maxStamina: number;
}

export interface PlayerProfile {
  userId: string;
  deviceId: string | null;
  email: string | null;
  createdAt: string;
  isBanned: boolean;
  level: number;
  exp: number;
  gold: number;
  gems: number;
  stamina: number;
  maxStamina: number;
  mmr: number;
}

// Guards against a client sending an impossible or out-of-range state (negative currencies,
// stamina above its own cap, etc.) before it ever reaches the database.
function validateSyncInput(body: unknown): PlayerSyncInput {
  const b = body as Partial<PlayerSyncInput> | null;
  if (!b || typeof b !== 'object') {
    throw ApiError.badRequest('Request body must be a player state object.');
  }

  const { level, exp, gold, gems, stamina, maxStamina } = b;
  const fields = { level, exp, gold, gems, stamina, maxStamina };

  for (const [key, value] of Object.entries(fields)) {
    if (typeof value !== 'number' || !Number.isFinite(value) || !Number.isInteger(value)) {
      throw ApiError.badRequest(`Field "${key}" must be an integer.`);
    }
  }

  if (level! < 1) throw ApiError.badRequest('level must be >= 1.');
  if (exp! < 0) throw ApiError.badRequest('exp must be >= 0.');
  if (gold! < 0) throw ApiError.badRequest('gold must be >= 0.');
  if (gems! < 0) throw ApiError.badRequest('gems must be >= 0.');
  if (maxStamina! < 0) throw ApiError.badRequest('maxStamina must be >= 0.');
  if (stamina! < 0 || stamina! > maxStamina!) throw ApiError.badRequest('stamina must be between 0 and maxStamina.');

  return { level: level!, exp: exp!, gold: gold!, gems: gems!, stamina: stamina!, maxStamina: maxStamina! };
}

export async function syncPlayerState(userId: string, body: unknown): Promise<PlayerProfile> {
  const input = validateSyncInput(body);

  const result = await pool.query(
    `UPDATE player_profiles
     SET level = $1, exp = $2, gold = $3, gems = $4, stamina = $5, max_stamina = $6, updated_at = now()
     WHERE user_id = $7
     RETURNING user_id, level, exp, gold, gems, stamina, max_stamina, mmr`,
    [input.level, input.exp, input.gold, input.gems, input.stamina, input.maxStamina, userId],
  );

  if (result.rows.length === 0) {
    throw ApiError.notFound('Player profile not found.');
  }

  const row = result.rows[0];
  return {
    userId: row.user_id,
    deviceId: null,
    email: null,
    createdAt: '',
    isBanned: false,
    level: row.level,
    exp: row.exp,
    gold: row.gold,
    gems: row.gems,
    stamina: row.stamina,
    maxStamina: row.max_stamina,
    mmr: row.mmr,
  };
}

export async function getPlayerProfile(userId: string): Promise<PlayerProfile> {
  const result = await pool.query(
    `SELECT u.id AS user_id, u.device_id, u.email, u.created_at, u.is_banned,
            p.level, p.exp, p.gold, p.gems, p.stamina, p.max_stamina, p.mmr
     FROM users u
     JOIN player_profiles p ON p.user_id = u.id
     WHERE u.id = $1`,
    [userId],
  );

  if (result.rows.length === 0) {
    throw ApiError.notFound('Player profile not found.');
  }

  const row = result.rows[0];
  return {
    userId: row.user_id,
    deviceId: row.device_id,
    email: row.email,
    createdAt: row.created_at,
    isBanned: row.is_banned,
    level: row.level,
    exp: row.exp,
    gold: row.gold,
    gems: row.gems,
    stamina: row.stamina,
    maxStamina: row.max_stamina,
    mmr: row.mmr,
  };
}
