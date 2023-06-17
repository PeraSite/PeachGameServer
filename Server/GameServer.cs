using System;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace PeachGame.Server;

public class GameServer : IDisposable {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(GameServer));

	private readonly TcpListener _server;

	public GameServer(int listenPort) {
		_server = new TcpListener(IPAddress.Any, listenPort);
	}

	public void Start() {
		Logger.Debug($"Server starting...");
		_server.Start();
		Logger.Debug($"Server started at {_server.LocalEndpoint}");
	}

	public void Dispose() {
		_server.Stop();
		Logger.Debug($"Server stopped!");
		GC.SuppressFinalize(this);
	}
}
