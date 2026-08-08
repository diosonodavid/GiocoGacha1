import { Router } from 'express';
import { authMiddleware } from '../middleware/authMiddleware';
import * as playerController from '../controllers/playerController';

const router = Router();

router.post('/sync', authMiddleware, playerController.sync);
router.get('/profile', authMiddleware, playerController.profile);

export default router;
