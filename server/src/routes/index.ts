import { Router } from 'express';
import authRoutes from './authRoutes';
import playerRoutes from './playerRoutes';
import pvpRoutes from './pvpRoutes';

const router = Router();

router.use('/auth', authRoutes);
router.use('/player', playerRoutes);
router.use('/pvp', pvpRoutes);

export default router;
