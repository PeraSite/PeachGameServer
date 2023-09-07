using log4net;
using WebSocketSharp.Server;

namespace PeachGame.Server;

public class GameServer {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(GameServer));

	private readonly WebSocketServer _server;

	public GameServer(int listenPort) {
		_server = new WebSocketServer(listenPort);
		_server.AddWebSocketService<SocketHandler>("/");

	}

	public void Start() {
		_server.Start();
		Logger.Debug($"Server started at {_server.Address}:{_server.Port}");
	}

	public void Stop() {
		_server.Stop();
		Logger.Debug("Server stopped!");
	}
}
