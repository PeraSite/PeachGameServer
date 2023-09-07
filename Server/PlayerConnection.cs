using System.IO;
using log4net;
using PeachGame.Common.Packets;
using PeachGame.Common.Serialization;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PeachGame.Server;

public class PlayerConnection {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(PlayerConnection));

	public IWebSocketSession Session { get; }

	public string Id => Session.ID;
	public string Nickname;

	public PlayerConnection(IWebSocketSession session) {
		Session = session;
		Nickname = string.Empty;
	}

	public void SendPacket(IPacket packet) {
		if (Session.State != WebSocketState.Open) {
			Logger.Warn($"[S -> C({ToString()})] Cannot send packet when state is {Session.State}");
			return;
		}
		Logger.Debug($"[S -> C({ToString()})] {packet}");

		// TODO: MemoryStream pooling?
		using MemoryStream ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);
		writer.Write(packet);
		writer.Flush();

		var data = ms.ToArray();
		Session.Context.WebSocket.Send(data);
	}

	public override string ToString() {
		return string.IsNullOrWhiteSpace(Nickname) ? $"{Session.ID}" : Nickname;
	}

	private bool Equals(PlayerConnection other) {
		return Session.ID.Equals(other.Session.ID);
	}

	public override bool Equals(object? obj) {
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((PlayerConnection) obj);
	}

	public override int GetHashCode() {
		return Session.ID.GetHashCode();
	}
}
