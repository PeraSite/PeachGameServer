using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Client;

namespace PeachGame.Server;

public class GameServer : IDisposable {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(GameServer));

	private readonly TcpListener _server;

	public GameServer(int listenPort) {
		_server = new TcpListener(IPAddress.Any, listenPort);
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

		while (true) {
			try {
				var tcpClient = await _server.AcceptTcpClientAsync();
				HandleClientJoin(tcpClient);
			}
			catch (SocketException) { }
			catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}
		}
	}

	private void HandleClientJoin(TcpClient tcpClient) {
		Logger.Info($"Client connected from {tcpClient.Client.RemoteEndPoint}");

		var playerConnection = new PlayerConnection(tcpClient);

		Task.Run(() => {
			try {
				while (playerConnection.Client.Connected) {
					var basePacket = playerConnection.ReadPacket();

					if (basePacket == null) break;

					switch (basePacket) {
						case ClientPingPacket packet: {
							Logger.Info($"[C -> S] {packet}");
							break;
						}
					}
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
}
