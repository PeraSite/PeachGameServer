using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;

namespace PeachGame.Server;

public class GameServer : IDisposable {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(GameServer));

	private readonly TcpListener _server;
	private readonly ConcurrentQueue<(PlayerConnection playerConnection, IPacket packet)> _receivedPacketQueue;
	private readonly Dictionary<PlayerConnection, Guid> _playerConnections;

	// 방 관련 State
	public readonly List<Room> Rooms;
	private int _lastRoomId;

	public GameServer(int listenPort) {
		_server = new TcpListener(IPAddress.Any, listenPort);
		_receivedPacketQueue = new ConcurrentQueue<(PlayerConnection playerConnection, IPacket packet)>();
		_playerConnections = new Dictionary<PlayerConnection, Guid>();
		Rooms = new List<Room>();
		_lastRoomId = 0;
	}

	public void Dispose() {
		_server.Stop();
		Logger.Debug($"Server stopped!");
		GC.SuppressFinalize(this);
	}

	public async Task Start() {
		Logger.Debug("Server starting...");
		_server.Start();
		Logger.Debug($"Server started at {_server.LocalEndpoint}");

		// Packet Dequeue Task
		_ = Task.Run(() => {
			while (true) {
				if (_receivedPacketQueue.TryDequeue(out var tuple)) {
					var (playerConnection, packet) = tuple;

					// handle packet
					HandlePacket(packet, playerConnection);
				}
			}
		});

		// 클라이언트 접속 처리
		while (true) {
			var tcpClient = await _server.AcceptTcpClientAsync();
			HandleClientJoin(tcpClient);
		}
	}

	#region Client Handling
	private void HandleClientJoin(TcpClient tcpClient) {
		Logger.Info($"Client connected from {tcpClient.Client.RemoteEndPoint}");

		var playerConnection = new PlayerConnection(tcpClient);
		_playerConnections[playerConnection] = Guid.Empty;

		Task.Run(() => {
			try {
				while (playerConnection.Client.Connected) {
					var packet = playerConnection.ReadPacket();

					if (packet == null) break;

					// 패킷 큐에 추가
					_receivedPacketQueue.Enqueue((playerConnection, packet));
				}
			}
			catch (SocketException) { } // 클라이언트 강제 종료
			catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}
			finally {
				HandleClientQuit(playerConnection);
			}
		});
	}

	private void HandleClientQuit(PlayerConnection playerConnection) {
		// 방에 존재한 플레이어였는지 확인
		var room = Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));
		if (room != null) {
			// 방에 있었다면 방에서 제거
			room.RemovePlayer(playerConnection);

			// 방이 비었다면 방 제거
			if (room.IsEmpty()) {
				Rooms.Remove(room);
				Logger.Info($"Room {room.RoomName} ({room.RoomId}) removed");
			}
		}

		Logger.Info($"Client disconnected from {playerConnection.Endpoint}");

		// 클라이언트 닫기
		playerConnection.Dispose();
	}
  #endregion

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
		_playerConnections[playerConnection] = packet.ClientId;
		playerConnection.Id = packet.ClientId;
		playerConnection.Nickname = packet.Nickname;
		playerConnection.SendPacket(new ServerPongPacket(packet.ClientId));
	}

	private void HandleClientRequestRoomListPacket(PlayerConnection playerConnection, ClientRequestRoomListPacket packet) {
		playerConnection.SendPacket(new ServerResponseRoomListPacket(Rooms.Select(x => x.GetRoomInfo()).ToList()));
	}

	private void HandleClientRequestCreateRoomPacket(PlayerConnection playerConnection, ClientRequestCreateRoomPacket packet) {
		var roomName = packet.Name;
		var roomId = _lastRoomId++;

		// 새로운 방 생성 후 추가
		var room = new Room(this, roomName, roomId, playerConnection);
		Rooms.Add(room);

		// 방 생성 알림 -> 씬 이동
		playerConnection.SendPacket(new ServerResponseCreateRoomPacket(roomId));

		// 방 상태 업데이트 -> UI 표기
		room.BroadcastState();

		room.BroadcastPacket(new ServerLobbyAnnouncePacket("[공지] 방이 생성되었습니다."));
	}

	private void HandleClientRequestJoinRoomPacket(PlayerConnection playerConnection, ClientRequestJoinRoomPacket packet) {
		var roomId = packet.RoomId;

		// 모든 방의 ID가 주어진 ID와 모두 다르다면(=같은 ID인 방이 없다면)
		if (Rooms.All(x => x.RoomId != roomId)) {
			playerConnection.SendPacket(new ServerResponseJoinRoomPacket("방이 존재하지 않습니다."));
			return;
		}

		var room = Rooms.First(x => x.RoomId == roomId);

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
		var room = Rooms.FirstOrDefault(x => x.RoomId == roomId);

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
			Rooms.Remove(room);
			Logger.Info($"Room {room.RoomName} ({room.RoomId}) removed");
		}
	}

	private void HandleClientChatPacket(PlayerConnection playerConnection, ClientChatPacket packet) {
		// 해당 ID의 방 찾기
		var room = Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

		if (room == null) {
			Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
			return;
		}

		// 방을 찾았다면, 채팅 메시지 Broadcast
		room.BroadcastPacket(packet);
	}

	private void HandleClientRequestStartPacket(PlayerConnection playerConnection, ClientRequestStartPacket packet) {
		// 해당 ID의 방 찾기
		var room = Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

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
		var room = Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

		// 방이 없다면 오류
		if (room == null) {
			Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
			return;
		}

		room.HandleDrag(playerConnection, packet);
	}

	private void HandleClientSelectRangePacket(PlayerConnection playerConnection, ClientSelectRangePacket packet) {
		// 해당 ID의 방 찾기
		var room = Rooms.FirstOrDefault(x => x.Players.Contains(playerConnection));

		// 방이 없다면 오류
		if (room == null) {
			Logger.Error($"Room not found for {playerConnection.Nickname} ({playerConnection.Id})");
			return;
		}

		room.HandleSelectRange(playerConnection, packet);
	}
}
