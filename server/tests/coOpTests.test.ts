import { CoOpRoomManager } from '../src/sockets/CoOpRoomManager';
import { CoOpMatchmakingService } from '../src/sockets/CoOpMatchmakingService';
import { ReconnectionHandler } from '../src/sockets/ReconnectionHandler';

describe('CoOpRoomManager', () => {
  let roomManager: CoOpRoomManager;

  beforeEach(() => {
    roomManager = new CoOpRoomManager();
  });

  it('clamps maxPlayers between 2 and 3', () => {
    const tooFew = roomManager.createRoom('dungeon-1', 1);
    const tooMany = roomManager.createRoom('dungeon-1', 10);

    expect(tooFew.maxPlayers).toBe(2);
    expect(tooMany.maxPlayers).toBe(3);
  });

  it('joins players up to the room cap and then rejects further joins', () => {
    const room = roomManager.createRoom('dungeon-1', 2);

    expect(roomManager.joinRoom(room.roomId, 'user-1', 'socket-1')).not.toBeNull();
    expect(roomManager.joinRoom(room.roomId, 'user-2', 'socket-2')).not.toBeNull();
    expect(roomManager.joinRoom(room.roomId, 'user-3', 'socket-3')).toBeNull();
  });

  it('marks a room in_progress once it reaches capacity', () => {
    const room = roomManager.createRoom('dungeon-1', 2);
    roomManager.joinRoom(room.roomId, 'user-1', 'socket-1');
    const fullRoom = roomManager.joinRoom(room.roomId, 'user-2', 'socket-2');

    expect(fullRoom?.status).toBe('in_progress');
  });

  it('removes the room once the last player leaves', () => {
    const room = roomManager.createRoom('dungeon-1', 2);
    roomManager.joinRoom(room.roomId, 'user-1', 'socket-1');
    roomManager.leaveRoom(room.roomId, 'user-1');

    expect(roomManager.getRoom(room.roomId)).toBeUndefined();
  });

  it('excludes full or foreign-dungeon rooms from listOpenRooms', () => {
    const openRoom = roomManager.createRoom('dungeon-1', 3);
    const fullRoom = roomManager.createRoom('dungeon-1', 2);
    roomManager.joinRoom(fullRoom.roomId, 'user-1', 'socket-1');
    roomManager.joinRoom(fullRoom.roomId, 'user-2', 'socket-2');
    roomManager.createRoom('dungeon-2', 3);

    const open = roomManager.listOpenRooms('dungeon-1');

    expect(open).toHaveLength(1);
    expect(open[0].roomId).toBe(openRoom.roomId);
  });

  it('finds the room a given player currently belongs to', () => {
    const room = roomManager.createRoom('dungeon-1', 3);
    roomManager.joinRoom(room.roomId, 'user-1', 'socket-1');

    expect(roomManager.findRoomByPlayer('user-1')?.roomId).toBe(room.roomId);
    expect(roomManager.findRoomByPlayer('nobody')).toBeUndefined();
  });
});

describe('CoOpMatchmakingService', () => {
  let roomManager: CoOpRoomManager;
  let matchmaking: CoOpMatchmakingService;

  beforeEach(() => {
    roomManager = new CoOpRoomManager();
    matchmaking = new CoOpMatchmakingService(roomManager);
  });

  it('creates a new room when no open room exists for the dungeon', () => {
    const room = matchmaking.findOrCreateRoom('user-1', 'socket-1', 'dungeon-1');

    expect(room.dungeonId).toBe('dungeon-1');
    expect(room.players.has('user-1')).toBe(true);
  });

  it('joins an existing open room instead of creating a new one', () => {
    const firstRoom = matchmaking.findOrCreateRoom('user-1', 'socket-1', 'dungeon-1', 3);
    const secondRoom = matchmaking.findOrCreateRoom('user-2', 'socket-2', 'dungeon-1', 3);

    expect(secondRoom.roomId).toBe(firstRoom.roomId);
    expect(secondRoom.players.size).toBe(2);
  });

  it('returns the same room for a player who is already in one', () => {
    const firstCall = matchmaking.findOrCreateRoom('user-1', 'socket-1', 'dungeon-1');
    const secondCall = matchmaking.findOrCreateRoom('user-1', 'socket-1', 'dungeon-1');

    expect(secondCall.roomId).toBe(firstCall.roomId);
  });
});

describe('ReconnectionHandler', () => {
  const fakeIo = { to: jest.fn().mockReturnThis(), emit: jest.fn() } as any;
  let roomManager: CoOpRoomManager;
  let handler: ReconnectionHandler;

  beforeEach(() => {
    jest.useFakeTimers();
    fakeIo.to.mockClear();
    fakeIo.emit.mockClear();
    roomManager = new CoOpRoomManager();
    handler = new ReconnectionHandler(roomManager, 30000);
  });

  afterEach(() => {
    handler.clearAllForTesting();
    jest.useRealTimers();
  });

  it('marks the player disconnected and emits PLAYER_DISCONNECTED without removing them immediately', () => {
    const room = roomManager.createRoom('dungeon-1', 3);
    roomManager.joinRoom(room.roomId, 'user-1', 'socket-1');

    handler.handleDisconnect(fakeIo, room.roomId, 'user-1');

    expect(fakeIo.emit).toHaveBeenCalledWith('PLAYER_DISCONNECTED', expect.objectContaining({ userId: 'user-1' }));
    expect(roomManager.getRoom(room.roomId)?.players.get('user-1')?.isConnected).toBe(false);
    expect(handler.hasPendingRemoval(room.roomId, 'user-1')).toBe(true);
  });

  it('cancels the pending removal and marks the player connected again on reconnect', () => {
    const localRoomManager = roomManager;
    const room = localRoomManager.createRoom('dungeon-1', 3);
    localRoomManager.joinRoom(room.roomId, 'user-1', 'socket-1');

    handler.handleDisconnect(fakeIo, room.roomId, 'user-1');
    const reconnected = handler.handleReconnect(fakeIo, room.roomId, 'user-1', 'socket-2');

    expect(reconnected).toBe(true);
    expect(handler.hasPendingRemoval(room.roomId, 'user-1')).toBe(false);
    expect(fakeIo.emit).toHaveBeenCalledWith('PLAYER_RECONNECTED', { userId: 'user-1' });
  });

  it('removes the player from the room once the grace period elapses without a reconnect', () => {
    const room = roomManager.createRoom('dungeon-1', 3);
    roomManager.joinRoom(room.roomId, 'user-1', 'socket-1');
    roomManager.joinRoom(room.roomId, 'user-2', 'socket-2');

    handler.handleDisconnect(fakeIo, room.roomId, 'user-1');
    jest.advanceTimersByTime(30001);

    expect(fakeIo.emit).toHaveBeenCalledWith('PLAYER_LEFT', { userId: 'user-1', reason: 'reconnect_timeout' });
    expect(roomManager.getRoom(room.roomId)?.players.has('user-1')).toBe(false);
  });
});
