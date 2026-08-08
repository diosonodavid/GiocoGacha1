import { NextFunction, Response } from 'express';
import { AuthenticatedRequest } from '../middleware/authMiddleware';
import { ApiError } from '../utils/ApiError';
import * as pvpService from '../services/pvpService';

export async function setDefense(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
  try {
    if (!req.userId) throw ApiError.unauthorized();
    await pvpService.setDefenseTeam(req.userId, req.body ?? {});
    res.status(200).json({ success: true });
  } catch (err) {
    next(err);
  }
}

export async function getOpponents(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
  try {
    if (!req.userId) throw ApiError.unauthorized();
    const opponents = await pvpService.findOpponents(req.userId);
    res.status(200).json({ success: true, data: opponents });
  } catch (err) {
    next(err);
  }
}

export async function submitResult(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
  try {
    if (!req.userId) throw ApiError.unauthorized();
    const outcome = await pvpService.submitMatchResult(req.userId, req.body ?? {});
    res.status(200).json({ success: true, data: outcome });
  } catch (err) {
    next(err);
  }
}

export async function leaderboard(_req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
  try {
    const entries = await pvpService.getLeaderboard();
    res.status(200).json({ success: true, data: entries });
  } catch (err) {
    next(err);
  }
}
