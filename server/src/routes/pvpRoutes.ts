import { Router } from 'express';
import { authMiddleware } from '../middleware/authMiddleware';
import * as pvpController from '../controllers/pvpController';

const router = Router();

router.post('/set-defense', authMiddleware, pvpController.setDefense);
router.get('/opponents', authMiddleware, pvpController.getOpponents);
router.post('/submit-result', authMiddleware, pvpController.submitResult);
router.get('/leaderboard', pvpController.leaderboard);

export default router;
