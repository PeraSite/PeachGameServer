using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;
using PeachGame.Common.Models;
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
	private readonly List<Room> _rooms;
	private int _lastRoomId;

	public GameServer(int listenPort) {
		_server = new TcpListener(IPAddress.Any, listenPort);
		_receivedPacketQueue = new ConcurrentQueue<(PlayerConnection playerConnection, IPacket packet)>();
		_playerConnections = new Dictionary<PlayerConnection, Guid>();
		_rooms = new List<Room>();
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
		Task.Run(() => {
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
		Logger.Info($"Client disconnected from {playerConnection.Ip}");

		// 클라이언트 닫기
		playerConnection.Dispose();
	}
  #endregion

	private void HandlePacket(IPacket basePacket, PlayerConnection playerConnection) {
		switch (basePacket) {
			case ClientPingPacket packet: {
				HandleClientPingPacket(playerConnection, packet);
				break;
			}
			case ClientRequestRoomListPacket packet: {
				HandleClientRequestRoomListPacket(playerConnection, packet);
				break;
			}
			case ClientRequestCreateRoomPacket packet: {
				HandleClientRequestCreateRoomPacket(playerConnection, packet);
				break;
			}
			case ClientRequestJoinRoomPacket packet: {
				HandleClientRequestJoinRoomPacket(playerConnection, packet);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(nameof(basePacket));
		}
	}

	private void HandleClientPingPacket(PlayerConnection playerConnection, ClientPingPacket packet) {
		_playerConnections[playerConnection] = packet.ClientId;
		playerConnection.Id = packet.ClientId;
		playerConnection.SendPacket(new ServerPongPacket(packet.ClientId));
	}

	private void HandleClientRequestRoomListPacket(PlayerConnection playerConnection, ClientRequestRoomListPacket packet) {
		playerConnection.SendPacket(new ServerResponseRoomListPacket(_rooms.Select(x => x.GetRoomInfo()).ToList()));
		playerConnection.SendPacket(new ServerResponseRoomListPacket(new List<RoomInfo> {
			new RoomInfo {
				RoomId = 0,
				CurrentPlayers = Random.Shared.Next(4),
				MaxPlayers = 4,
				Name = "테스트 방",
				State = RoomState.Waiting
			},
			new RoomInfo {
				RoomId = 1,
				CurrentPlayers = Random.Shared.Next(4),
				MaxPlayers = 4,
				Name = "테스트 방 2",
				State = RoomState.Playing
			},
			new RoomInfo {
				RoomId = 2,
				CurrentPlayers = Random.Shared.Next(4),
				MaxPlayers = 4,
				Name = "테스트 방 333",
				State = RoomState.Ending
			},
		}));
	}

	private void HandleClientRequestCreateRoomPacket(PlayerConnection playerConnection, ClientRequestCreateRoomPacket packet) {
		var roomName = packet.Name;
		var roomId = _lastRoomId++;

		var room = new Room(roomName, roomId, playerConnection);
		_rooms.Add(room);

		playerConnection.SendPacket(new ServerResponseCreateRoomPacket(roomId));
	}


	private void HandleClientRequestJoinRoomPacket(PlayerConnection playerConnection, ClientRequestJoinRoomPacket packet) {
		var roomId = packet.RoomId;

		// 모든 방의 ID가 주어진 ID와 모두 다르다면(=같은 ID인 방이 없다면)
		if (_rooms.All(x => x.RoomId != roomId)) {
			playerConnection.SendPacket(new ServerResponseJoinRoomPacket(false, "방이 존재하지 않습니다."));
			return;
		}

		var room = _rooms.First(x => x.RoomId == roomId);

		if (room.IsFull()) {
			playerConnection.SendPacket(new ServerResponseJoinRoomPacket(false, "방이 꽉 찼습니다."));
			return;
		}

		if (!room.IsAvailable()) {
			playerConnection.SendPacket(new ServerResponseJoinRoomPacket(false, "방에 들어갈 수 없습니다."));
			return;
		}

		playerConnection.SendPacket(new ServerResponseJoinRoomPacket(true));
	}
}
