import { Server as HttpServer } from 'http';
import { Server as HttpsServer } from 'https';
import { Server, Socket } from 'socket.io';
import { verifyAccessToken } from '../utils/jwt';
import { coOpMatchmakingService } from './CoOpMatchmakingService';
import { CoOpRoom, coOpRoomManager } from './CoOpRoomManager';
import { reconnectionHandler } from './ReconnectionHandler';

interface AuthenticatedSocket extends Socket {
  data: { userId: string };
}

let ioInstance: Server | null = null;

// Every event below assumes the io.use() handshake middleware already rejected unauthenticated
// sockets, so handlers can trust socket.data.userId without re-checking it per event.
export function initializeSocketManager(httpServer: HttpServer | HttpsServer): Server {
  const io = new Server(httpServer, {
    cors: { origin: process.env.CORS_ORIGIN || '*' },
  });

  io.use((socket, next) => {
    const token = socket.handshake.auth?.token ?? socket.handshake.query?.token;
    if (typeof token !== 'string') {
      next(new Error('Missing authentication token.'));
      return;
    }
    try {
      const payload = verifyAccessToken(token);
      socket.data.userId = payload.userId;
      next();
    } catch {
      next(new Error('Invalid or expired token.'));
    }
  });

  io.on('connection', (socket: AuthenticatedSocket) => {
    const userId = socket.data.userId;
    socket.join(`user:${userId}`);

    socket.on('COOP_FIND_ROOM', (payload: { dungeonId: string; maxPlayers?: number }) => {
      const room = coOpMatchmakingService.findOrCreateRoom(userId, socket.id, payload?.dungeonId, payload?.maxPlayers);
      socket.join(room.roomId);
      io.to(room.roomId).emit('COOP_ROOM_UPDATE', serializeRoom(room));
    });

    socket.on('COOP_JOIN_ROOM', (payload: { roomId: string }) => {
      const room = coOpRoomManager.joinRoom(payload?.roomId, userId, socket.id);
      if (!room) {
        socket.emit('COOP_ERROR', { message: 'Room is full or does not exist.' });
        return;
      }
      socket.join(room.roomId);
      io.to(room.roomId).emit('COOP_ROOM_UPDATE', serializeRoom(room));
    });

    socket.on('COOP_LEAVE_ROOM', (payload: { roomId: string }) => {
      const room = coOpRoomManager.leaveRoom(payload?.roomId, userId);
      socket.leave(payload?.roomId);
      if (room) io.to(room.roomId).emit('COOP_ROOM_UPDATE', serializeRoom(room));
    });

    // Broadcasts a used-skill packet to every member of the caster's active co-op room,
    // including the caster, so all clients apply the same combat action in lockstep.
    socket.on('PLAYER_ACTION', (payload: Record<string, unknown>) => {
      const room = coOpRoomManager.findRoomByPlayer(userId);
      if (!room) return;
      io.to(room.roomId).emit('PLAYER_ACTION', { ...payload, userId, timestamp: Date.now() });
    });

    socket.on('COOP_RECONNECT', (payload: { roomId: string }) => {
      const reconnected = reconnectionHandler.handleReconnect(io, payload?.roomId, userId, socket.id);
      if (reconnected) socket.join(payload.roomId);
    });

    socket.on('disconnect', () => {
      const room = coOpRoomManager.findRoomByPlayer(userId);
      if (room) reconnectionHandler.handleDisconnect(io, room.roomId, userId);
    });
  });

  ioInstance = io;
  return io;
}

export function getSocketServer(): Server | null {
  return ioInstance;
}

function serializeRoom(room: CoOpRoom) {
  return {
    roomId: room.roomId,
    dungeonId: room.dungeonId,
    maxPlayers: room.maxPlayers,
    status: room.status,
    players: [...room.players.values()],
  };
}
