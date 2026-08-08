import { NextFunction, Response } from 'express';
import { AuthenticatedRequest } from '../middleware/authMiddleware';
import { ApiError } from '../utils/ApiError';
import * as playerService from '../services/playerService';

export async function sync(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
  try {
    if (!req.userId) throw ApiError.unauthorized();
    const profile = await playerService.syncPlayerState(req.userId, req.body);
    res.status(200).json({ success: true, data: profile });
  } catch (err) {
    next(err);
  }
}

export async function profile(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
  try {
    if (!req.userId) throw ApiError.unauthorized();
    const result = await playerService.getPlayerProfile(req.userId);
    res.status(200).json({ success: true, data: result });
  } catch (err) {
    next(err);
  }
}
