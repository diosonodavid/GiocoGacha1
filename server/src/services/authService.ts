import crypto from 'crypto';
import bcrypt from 'bcryptjs';
import { pool } from '../db/connection';
import { ApiError } from '../utils/ApiError';
import { issueTokenPair } from '../utils/jwt';

const BCRYPT_ROUNDS = 12;

interface UserRow {
  id: string;
  device_id: string | null;
  email: string | null;
  password_hash: string | null;
  is_banned: boolean;
}

export interface AuthResult {
  userId: string;
  accessToken: string;
  refreshToken: string;
}

// Creates a brand-new guest account tied to a device_id (generated if the client didn't send
// one) and seeds its player_profiles / pvp_matchmaking rows so downstream endpoints never have
// to special-case a "profile doesn't exist yet" state.
export async function registerGuest(deviceId?: string): Promise<AuthResult> {
  const resolvedDeviceId = deviceId && deviceId.trim().length > 0 ? deviceId.trim() : crypto.randomUUID();

  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    const existing = await client.query<UserRow>('SELECT id FROM users WHERE device_id = $1', [resolvedDeviceId]);
    if (existing.rows.length > 0) {
      throw ApiError.conflict('A guest account already exists for this device.');
    }

    const userResult = await client.query<UserRow>(
      'INSERT INTO users (device_id) VALUES ($1) RETURNING id',
      [resolvedDeviceId],
    );
    const userId = userResult.rows[0].id;

    await client.query('INSERT INTO player_profiles (user_id) VALUES ($1)', [userId]);
    await client.query('INSERT INTO pvp_matchmaking (user_id) VALUES ($1)', [userId]);

    await client.query('COMMIT');

    const tokens = issueTokenPair(userId);
    return { userId, ...tokens };
  } catch (err) {
    await client.query('ROLLBACK');
    throw err;
  } finally {
    client.release();
  }
}

// Accepts either a returning guest (deviceId) or a registered email/password account.
export async function login(params: { deviceId?: string; email?: string; password?: string }): Promise<AuthResult> {
  const { deviceId, email, password } = params;

  let row: UserRow | undefined;

  if (deviceId) {
    const result = await pool.query<UserRow>('SELECT * FROM users WHERE device_id = $1', [deviceId]);
    row = result.rows[0];
  } else if (email && password) {
    const result = await pool.query<UserRow>('SELECT * FROM users WHERE email = $1', [email]);
    row = result.rows[0];

    if (row && (!row.password_hash || !(await bcrypt.compare(password, row.password_hash)))) {
      throw ApiError.unauthorized('Invalid email or password.');
    }
  } else {
    throw ApiError.badRequest('Provide either deviceId or email + password.');
  }

  if (!row) {
    throw ApiError.unauthorized('Invalid credentials.');
  }
  if (row.is_banned) {
    throw ApiError.forbidden('This account has been banned.');
  }

  const tokens = issueTokenPair(row.id);
  return { userId: row.id, ...tokens };
}

export async function hashPassword(password: string): Promise<string> {
  return bcrypt.hash(password, BCRYPT_ROUNDS);
}
