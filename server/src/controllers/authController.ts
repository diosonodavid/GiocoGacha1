import { NextFunction, Request, Response } from 'express';
import * as authService from '../services/authService';

export async function registerGuest(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { deviceId } = req.body ?? {};
    const result = await authService.registerGuest(deviceId);
    res.status(201).json({ success: true, data: result });
  } catch (err) {
    next(err);
  }
}

export async function login(req: Request, res: Response, next: NextFunction): Promise<void> {
  try {
    const { deviceId, email, password } = req.body ?? {};
    const result = await authService.login({ deviceId, email, password });
    res.status(200).json({ success: true, data: result });
  } catch (err) {
    next(err);
  }
}
