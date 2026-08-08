import { NextFunction, Request, Response } from 'express';
import { ApiError } from '../utils/ApiError';

// Centralized Express error handler - every failure path (thrown ApiError, unexpected
// exception, or 404 fallthrough) is normalized into the same { success, error } JSON shape.
export function errorHandler(err: unknown, _req: Request, res: Response, _next: NextFunction): void {
  if (err instanceof ApiError) {
    res.status(err.statusCode).json({
      success: false,
      error: { code: err.code, message: err.message },
    });
    return;
  }

  // eslint-disable-next-line no-console
  console.error('Unhandled error', err);
  res.status(500).json({
    success: false,
    error: { code: 'INTERNAL_ERROR', message: 'Internal server error' },
  });
}

export function notFoundHandler(req: Request, res: Response): void {
  res.status(404).json({
    success: false,
    error: { code: 'NOT_FOUND', message: `No route for ${req.method} ${req.originalUrl}` },
  });
}
