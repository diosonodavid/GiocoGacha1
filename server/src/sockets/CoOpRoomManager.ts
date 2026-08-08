import { randomUUID } from 'crypto';

export type CoOpRoomStatus = 'waiting' | 'in_progress' | 'closed';

export interface CoOpRoomPlayer {
  userId: string;
  socketId: string;
  isConnected: boolean;
}

export interface CoOpRoom {
  roomId: string;
  dungeonId: string;
  maxPlayers: number;
  status: CoOpRoomStatus;
  players: Map<string, CoOpRoomPlayer>;
  createdAt: number;
}

const MIN_PLAYERS = 2;
const MAX_PLAYERS = 3;

// In-memory room registry: co-op presence is inherently transient (tied to live socket
// connections), so unlike auth/player/PvP state there is nothing here worth persisting to
// Postgres or Redis - a process restart simply drops active lobbies.
export class CoOpRoomManager {
  private rooms = new Map<string, CoOpRoom>();

  createRoom(dungeonId: string, maxPlayers: number = MAX_PLAYERS): CoOpRoom {
    const clampedMax = Math.min(MAX_PLAYERS, Math.max(MIN_PLAYERS, maxPlayers));
    const room: CoOpRoom = {
      roomId: randomUUID(),
      dungeonId,
      maxPlayers: clampedMax,
      status: 'waiting',
      players: new Map(),
      createdAt: Date.now(),
    };
    this.rooms.set(room.roomId, room);
    return room;
  }

  joinRoom(roomId: string, userId: string, socketId: string): CoOpRoom | null {
    const room = this.rooms.get(roomId);
    if (!room || room.status === 'closed') return null;
    if (!room.players.has(userId) && room.players.size >= room.maxPlayers) return null;

    room.players.set(userId, { userId, socketId, isConnected: true });
    if (room.players.size >= room.maxPlayers) room.status = 'in_progress';
    return room;
  }

  leaveRoom(roomId: string, userId: string): CoOpRoom | null {
    const room = this.rooms.get(roomId);
    if (!room) return null;

    room.players.delete(userId);
    if (room.players.size === 0) {
      this.rooms.delete(roomId);
      return null;
    }
    if (room.status === 'in_progress' && room.players.size < room.maxPlayers) {
      room.status = 'waiting';
    }
    return room;
  }

  markPlayerConnection(roomId: string, userId: string, isConnected: boolean, socketId?: string): CoOpRoom | null {
    const room = this.rooms.get(roomId);
    const player = room?.players.get(userId);
    if (!room || !player) return null;

    player.isConnected = isConnected;
    if (socketId) player.socketId = socketId;
    return room;
  }

  getRoom(roomId: string): CoOpRoom | undefined {
    return this.rooms.get(roomId);
  }

  findRoomByPlayer(userId: string): CoOpRoom | undefined {
    for (const room of this.rooms.values()) {
      if (room.players.has(userId)) return room;
    }
    return undefined;
  }

  listOpenRooms(dungeonId: string): CoOpRoom[] {
    return [...this.rooms.values()].filter(
      (room) => room.dungeonId === dungeonId && room.status === 'waiting' && room.players.size < room.maxPlayers,
    );
  }

  // Test-only escape hatch so specs don't leak room state into one another between cases.
  clearAllForTesting(): void {
    this.rooms.clear();
  }
}

export const coOpRoomManager = new CoOpRoomManager();
