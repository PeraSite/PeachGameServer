using System.Collections.Generic;
using log4net;
using WebSocketSharp.Server;

namespace PeachGame.Server;

public class GameServer {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(GameServer));

	public readonly Dictionary<string, PlayerConnection> PlayerConnections = new Dictionary<string, PlayerConnection>();
	public readonly List<Room> Rooms = new List<Room>();
	private int _lastRoomId;

	private readonly WebSocketServer _server;

	public GameServer(int listenPort) {
		_server = new WebSocketServer(listenPort);
		_server.AddWebSocketService("/", () => new SocketHandler(this));
	}

	public void Start() {
		_server.Start();
		Logger.Debug($"Server started at {_server.Address}:{_server.Port}");
	}

	public void Stop() {
		_server.Stop();
		Logger.Debug("Server stopped!");
	}

	public Room CreateRoom(string roomName, PlayerConnection owner) {
		var room = new Room(roomName, _lastRoomId++, owner);
		Rooms.Add(room);
		room.OnEnd += () => { Rooms.Remove(room); };
		return room;
	}
}
