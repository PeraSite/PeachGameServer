using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using log4net;
using WebSocketSharp.Server;

namespace PeachGame.Server;

public class GameServer {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(GameServer));

	public readonly Dictionary<string, PlayerConnection> PlayerConnections = new Dictionary<string, PlayerConnection>();
	public readonly List<Room> Rooms = new List<Room>();
	private int _lastRoomId;

	private readonly WebSocketServer _server;

	public GameServer(int listenPort, bool isProduction, string certFilePath, string keyFilePath) {
		_server = new WebSocketServer(listenPort, true);

		if (isProduction) {
			// Load certificate from file
			var cert = X509Certificate2.CreateFromPemFile(certFilePath, keyFilePath);
			_server.SslConfiguration.ServerCertificate = cert;
		} else {
			// Create self-signed certificate
			var cert = CreateSelfSignedCertificate();

			Console.WriteLine(cert.HasPrivateKey);

			_server.SslConfiguration.ServerCertificate = cert;
		}

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

	private static X509Certificate2 CreateSelfSignedCertificate() {
		using var rsa = RSA.Create();
		var certRequest = new CertificateRequest("cn=127.0.0.1", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		// We're just going to create a temporary certificate, that won't be valid for long
		var certificate = certRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));

		return certificate;
	}


}
