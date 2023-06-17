using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

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
			var tcpClient = await _server.AcceptTcpClientAsync().ConfigureAwait(false);
			await Task.Factory.StartNew(() => HandleClient(tcpClient));
		}
	}

	private async void HandleClient(TcpClient tcpClient) {
		Logger.Debug($"Handling client {tcpClient.Client.RemoteEndPoint}");
	}
}
