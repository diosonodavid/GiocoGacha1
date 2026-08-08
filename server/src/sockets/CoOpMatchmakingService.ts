import { CoOpRoom, CoOpRoomManager, coOpRoomManager } from './CoOpRoomManager';

const DEFAULT_MAX_PLAYERS = 3;

// Pairs a player looking for a dungeon run with an existing open room for that dungeon, or opens
// a new one if none is available - a simple first-fit queue rather than an MMR-aware match.
export class CoOpMatchmakingService {
  constructor(private readonly roomManager: CoOpRoomManager) {}

  findOrCreateRoom(userId: string, socketId: string, dungeonId: string, maxPlayers: number = DEFAULT_MAX_PLAYERS): CoOpRoom {
    const existingRoom = this.roomManager.findRoomByPlayer(userId);
    if (existingRoom) return existingRoom;

    const openRooms = this.roomManager.listOpenRooms(dungeonId);
    const targetRoom = openRooms[0] ?? this.roomManager.createRoom(dungeonId, maxPlayers);

    return this.roomManager.joinRoom(targetRoom.roomId, userId, socketId) ?? targetRoom;
  }
}

export const coOpMatchmakingService = new CoOpMatchmakingService(coOpRoomManager);
