import { Server } from 'socket.io';
import { CoOpRoomManager, coOpRoomManager } from './CoOpRoomManager';

const RECONNECT_GRACE_MS = 30000;

// Gives a player who drops mid-battle a grace window to rejoin the same room before they are
// actually removed from it, so a brief network blip doesn't end the match for everyone else.
export class ReconnectionHandler {
  private pendingRemovals = new Map<string, NodeJS.Timeout>();

  constructor(
    private readonly roomManager: CoOpRoomManager = coOpRoomManager,
    private readonly gracePeriodMs: number = RECONNECT_GRACE_MS,
  ) {}

  private key(roomId: string, userId: string): string {
    return `${roomId}:${userId}`;
  }

  handleDisconnect(io: Server, roomId: string, userId: string): void {
    this.roomManager.markPlayerConnection(roomId, userId, false);
    io.to(roomId).emit('PLAYER_DISCONNECTED', { userId, gracePeriodMs: this.gracePeriodMs });

    const pendingKey = this.key(roomId, userId);
    const timer = setTimeout(() => {
      this.pendingRemovals.delete(pendingKey);
      const room = this.roomManager.getRoom(roomId);
      const player = room?.players.get(userId);
      if (player && !player.isConnected) {
        this.roomManager.leaveRoom(roomId, userId);
        io.to(roomId).emit('PLAYER_LEFT', { userId, reason: 'reconnect_timeout' });
      }
    }, this.gracePeriodMs);

    this.pendingRemovals.set(pendingKey, timer);
  }

  handleReconnect(io: Server, roomId: string, userId: string, socketId: string): boolean {
    const pendingKey = this.key(roomId, userId);
    const timer = this.pendingRemovals.get(pendingKey);
    if (timer) {
      clearTimeout(timer);
      this.pendingRemovals.delete(pendingKey);
    }

    const room = this.roomManager.markPlayerConnection(roomId, userId, true, socketId);
    if (!room) return false;

    io.to(roomId).emit('PLAYER_RECONNECTED', { userId });
    return true;
  }

  hasPendingRemoval(roomId: string, userId: string): boolean {
    return this.pendingRemovals.has(this.key(roomId, userId));
  }

  clearAllForTesting(): void {
    for (const timer of this.pendingRemovals.values()) clearTimeout(timer);
    this.pendingRemovals.clear();
  }
}

export const reconnectionHandler = new ReconnectionHandler();
