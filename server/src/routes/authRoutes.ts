import { Router } from 'express';
import * as authController from '../controllers/authController';

const router = Router();

router.post('/register-guest', authController.registerGuest);
router.post('/login', authController.login);

export default router;
