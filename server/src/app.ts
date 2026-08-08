import fs from 'fs';
import http, { Server as HttpServer } from 'http';
import https, { Server as HttpsServer } from 'https';
import cors from 'cors';
import dotenv from 'dotenv';
import express, { Express } from 'express';
import helmet from 'helmet';
import { errorHandler, notFoundHandler } from './middleware/errorHandler';
import { rateLimiter } from './middleware/rateLimiter';
import apiRoutes from './routes';
import { initializeSocketManager } from './sockets/socketManager';

dotenv.config();

export function createApp(): Express {
  const app = express();

  app.use(helmet());
  app.use(cors({ origin: process.env.CORS_ORIGIN || '*' }));
  app.use(express.json());
  app.use(rateLimiter);

  app.get('/health', (_req, res) => res.status(200).json({ success: true, data: { status: 'ok' } }));
  app.use('/api', apiRoutes);

  app.use(notFoundHandler);
  app.use(errorHandler);

  return app;
}

export const app = createApp();

// Serves over HTTPS when TLS_KEY_PATH/TLS_CERT_PATH are configured (production), otherwise
// falls back to plain HTTP so local development doesn't require a certificate.
export function startServer(port: number = Number(process.env.PORT || 3443)): HttpServer | HttpsServer {
  const keyPath = process.env.TLS_KEY_PATH;
  const certPath = process.env.TLS_CERT_PATH;

  let server: HttpServer | HttpsServer;
  if (keyPath && certPath) {
    const credentials = {
      key: fs.readFileSync(keyPath),
      cert: fs.readFileSync(certPath),
    };
    server = https.createServer(credentials, app);
  } else {
    // eslint-disable-next-line no-console
    console.warn('TLS_KEY_PATH/TLS_CERT_PATH not set - starting plain HTTP server (development only).');
    server = http.createServer(app);
  }

  initializeSocketManager(server);

  server.listen(port, () => {
    // eslint-disable-next-line no-console
    console.log(`GachaGame server listening on port ${port} (${keyPath && certPath ? 'HTTPS' : 'HTTP'})`);
  });

  return server;
}
