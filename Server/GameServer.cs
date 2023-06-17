using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

	public GameServer(int listenPort) {
		_server = new TcpListener(IPAddress.Any, listenPort);
		_receivedPacketQueue = new ConcurrentQueue<(PlayerConnection playerConnection, IPacket packet)>();
		_playerConnections = new Dictionary<PlayerConnection, Guid>();
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
			default:
				throw new ArgumentOutOfRangeException(nameof(basePacket));
		}
	}

	private void HandleClientPingPacket(PlayerConnection playerConnection, ClientPingPacket packet) {
		_playerConnections[playerConnection] = packet.ClientId;
		playerConnection.SendPacket(new ServerPongPacket(packet.ClientId));
	}
}
