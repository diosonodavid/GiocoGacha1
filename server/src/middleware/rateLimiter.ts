import { NextFunction, Request, Response } from 'express';
import { redisClient } from '../db/redis';
import { ApiError } from '../utils/ApiError';

const MAX_REQUESTS_PER_SECOND = 10;
const WINDOW_SECONDS = 1;

// Fixed 1-second window per IP, counted in Redis so the limit holds across multiple server
// instances. Fails open (lets the request through) if Redis itself is unreachable, so an
// infrastructure outage degrades to "no rate limiting" instead of a full outage.
export async function rateLimiter(req: Request, _res: Response, next: NextFunction): Promise<void> {
  const ip = req.ip || req.socket.remoteAddress || 'unknown';
  const bucket = Math.floor(Date.now() / 1000);
  const key = `ratelimit:${ip}:${bucket}`;

  try {
    const count = await redisClient.incr(key);
    if (count === 1) {
      await redisClient.expire(key, WINDOW_SECONDS * 2);
    }

    if (count > MAX_REQUESTS_PER_SECOND) {
      next(ApiError.tooManyRequests('Rate limit exceeded. Max 10 requests per second.'));
      return;
    }

    next();
  } catch (err) {
    // eslint-disable-next-line no-console
    console.error('Rate limiter Redis error, failing open', err);
    next();
  }
}
