using System;
using System.IO;
using System.Linq;
using log4net;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;
using PeachGame.Common.Serialization;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PeachGame.Server;

public class SocketHandler : WebSocketBehavior {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(SocketHandler));

	private readonly GameServer _server;

	public SocketHandler(GameServer gameServer) {
		_server = gameServer;
	}

	protected override void OnOpen() {
		Logger.Info($"Client connected: {ID}");

		var session = Sessions[ID];
		var playerConnection = new PlayerConnection(session);
		_server.PlayerConnections[ID] = playerConnection;
	}

	protected override void OnClose(CloseEventArgs e) {
		Logger.Info($"Client closed: {ID}, reason: {e.Reason}, code: {e.Code}");

		if (!_server.PlayerConnections.TryGetValue(ID, out var playerConnection)) return;

		var room = _server.Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));
		if (room == null) return;

		// 방에 있었다면 방에서 제거
		room.RemovePlayer(playerConnection);

		// 방이 비었다면 방 제거
		if (!room.IsEmpty()) return;
		_server.Rooms.Remove(room);
		Logger.Info($"Room {room.RoomName} ({room.RoomId}) removed");
	}

	protected override void OnMessage(MessageEventArgs e) {
		if (!e.IsBinary) return;

		var data = e.RawData;
		var reader = new BinaryReader(new MemoryStream(data));

		var packet = reader.ReadPacket();
		if (!_server.PlayerConnections.TryGetValue(ID, out var playerConnection)) return;

		HandlePacket(packet, playerConnection);
	}

	private void HandlePacket(IPacket basePacket, PlayerConnection playerConnection) {
		switch (basePacket) {
			case ClientPingPacket packet:
				HandleClientPingPacket(playerConnection, packet);
				break;
			case ClientRequestRoomListPacket packet:
				HandleClientRequestRoomListPacket(playerConnection, packet);
				break;
			case ClientRequestCreateRoomPacket packet:
				HandleClientRequestCreateRoomPacket(playerConnection, packet);
				break;
			case ClientRequestJoinRoomPacket packet:
				HandleClientRequestJoinRoomPacket(playerConnection, packet);
				break;
			case ClientRequestQuitRoomPacket packet:
				HandleClientRequestQuitRoomPacket(playerConnection, packet);
				break;
			case ClientChatPacket packet:
				HandleClientChatPacket(playerConnection, packet);
				break;
			case ClientRequestStartPacket packet:
				HandleClientRequestStartPacket(playerConnection, packet);
				break;
			case ClientRequestDragPacket packet:
				HandleClientRequestDragPacket(playerConnection, packet);
				break;
			case ClientSelectRangePacket packet:
				HandleClientSelectRangePacket(playerConnection, packet);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(basePacket));
		}
	}

	private void HandleClientPingPacket(PlayerConnection playerConnection, ClientPingPacket packet) {
		playerConnection.Nickname = packet.Nickname;
		playerConnection.SendPacket(new ServerPongPacket(playerConnection.Id));
	}

	private void HandleClientRequestRoomListPacket(PlayerConnection playerConnection, ClientRequestRoomListPacket packet) {
		playerConnection.SendPacket(new ServerResponseRoomListPacket(_server.Rooms.Select(x => x.GetRoomInfo()).ToList()));
	}

	private void HandleClientRequestCreateRoomPacket(PlayerConnection playerConnection, ClientRequestCreateRoomPacket packet) {
		var roomName = packet.Name;

		// 새로운 방 생성 후 추가
		var room = _server.CreateRoom(roomName, playerConnection);

		// 방 생성 알림 -> 씬 이동
		playerConnection.SendPacket(new ServerResponseCreateRoomPacket(room.RoomId));

		// 씬 이동까지 기다리고, 패킷 Broadcast 해야함

		// 방 상태 업데이트 -> UI 표기
		room.BroadcastState();

		room.BroadcastPacket(new ServerLobbyAnnouncePacket("[공지] 방이 생성되었습니다."));
	}

	// private void HandleClientRequestRoomStatePacket(PlayerConnection playerConnection, ClientRequestRoomStatePacket packet) {
	// 	// 해당 ID의 방 찾기
	// 	var room = _server.Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));
	//
	// 	// 방이 없다면 오류
	// 	if (room == null) {
	// 		Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
	// 		return;
	// 	}
	//
	// 	playerConnection.SendPacket(new ServerRoomStatePacket(room.GetRoomInfo()));
	// }

	// private void HandleClientRoomJoinedPacket(PlayerConnection playerConnection, ClientRoomJoinedPacket packet) {
	// 	// 해당 ID의 방 찾기
	// 	var room = _server.Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));
	//
	// 	if (room == null) {
	// 		Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
	// 		return;
	// 	}
	//
	// 	// 방을 찾았다면, 채팅 메시지 Broadcast
	// 	room.BroadcastPacket(packet);
	// }

	private void HandleClientRequestJoinRoomPacket(PlayerConnection playerConnection, ClientRequestJoinRoomPacket packet) {
		var roomId = packet.RoomId;

		// 모든 방의 ID가 주어진 ID와 모두 다르다면(=같은 ID인 방이 없다면)
		if (_server.Rooms.All(x => x.RoomId != roomId)) {
			playerConnection.SendPacket(new ServerResponseJoinRoomPacket("방이 존재하지 않습니다."));
			return;
		}

		var room = _server.Rooms.First(x => x.RoomId == roomId);

		if (room.IsFull()) {
			playerConnection.SendPacket(new ServerResponseJoinRoomPacket("방이 꽉 찼습니다."));
			return;
		}

		if (!room.IsAvailable()) {
			playerConnection.SendPacket(new ServerResponseJoinRoomPacket("방에 들어갈 수 없습니다."));
			return;
		}

		// 방 접속 알림 -> 씬 이동
		playerConnection.SendPacket(new ServerResponseJoinRoomPacket(roomId));

		// 방 상태 업데이트 -> UI 표기
		room.AddPlayer(playerConnection);
	}

	private void HandleClientRequestQuitRoomPacket(PlayerConnection playerConnection, ClientRequestQuitRoomPacket packet) {
		var roomId = packet.RoomId;

		// 해당 ID의 방 찾기
		var room = _server.Rooms.FirstOrDefault(x => x.RoomId == roomId);

		// 방이 없다면
		if (room == null) {
			playerConnection.SendPacket(new ServerResponseQuitRoomPacket(false, "방이 존재하지 않습니다."));
			return;
		}

		// 해당 방에 플레이어가 존재하지 않다면
		if (!room.Players.Contains(playerConnection)) {
			playerConnection.SendPacket(new ServerResponseQuitRoomPacket(false, "해당 방의 참가자가 아닙니다."));
			return;
		}

		// 방 퇴장 알림 -> 씬 이동
		playerConnection.SendPacket(new ServerResponseQuitRoomPacket(true));

		// 방 상태 업데이트 -> UI 표기
		room.RemovePlayer(playerConnection);

		// 만약 유저가 나갔는데 방이 비었다면
		if (room.IsEmpty()) {
			_server.Rooms.Remove(room);
			Logger.Info($"Room {room.RoomName} ({room.RoomId}) removed");
		}
	}

	private void HandleClientChatPacket(PlayerConnection playerConnection, ClientChatPacket packet) {
		// 해당 ID의 방 찾기
		var room = _server.Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

		if (room == null) {
			Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
			return;
		}

		// 방을 찾았다면, 채팅 메시지 Broadcast
		room.BroadcastPacket(packet);
	}

	private void HandleClientRequestStartPacket(PlayerConnection playerConnection, ClientRequestStartPacket packet) {
		// 해당 ID의 방 찾기
		var room = _server.Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

		// 방이 없다면 오류
		if (room == null) {
			playerConnection.SendPacket(new ServerResponseStartPacket("당신은 방에 참여하고 있지 않습니다."));
			return;
		}

		// 씬 이동시키고 시드 설정
		room.BroadcastPacket(new ServerResponseStartPacket(room.RoomId, room.Seed));

		// 게임 State 변동
		room.StartGame();
	}

	private void HandleClientRequestDragPacket(PlayerConnection playerConnection, ClientRequestDragPacket packet) {
		// 해당 ID의 방 찾기
		var room = _server.Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

		// 방이 없다면 오류
		if (room == null) {
			Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
			return;
		}

		room.HandleDrag(playerConnection, packet);
	}

	private void HandleClientSelectRangePacket(PlayerConnection playerConnection, ClientSelectRangePacket packet) {
		// 해당 ID의 방 찾기
		var room = _server.Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

		// 방이 없다면 오류
		if (room == null) {
			Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
			return;
		}

		room.HandleSelectRange(playerConnection, packet);
	}
}
