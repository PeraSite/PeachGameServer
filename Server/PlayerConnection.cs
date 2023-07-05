using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using log4net;
using PeachGame.Common.Packets;
using PeachGame.Common.Serialization;

namespace PeachGame.Server;

public class PlayerConnection : IDisposable {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(PlayerConnection));

	public TcpClient Client { get; }
	public NetworkStream Stream { get; }
	public BinaryReader Reader { get; }
	public BinaryWriter Writer { get; }
	public EndPoint? Endpoint => Client.Client.RemoteEndPoint;

	public Guid Id;
	public string Nickname;

	public PlayerConnection(TcpClient client) {
		Client = client;
		Stream = Client.GetStream();
		Writer = new BinaryWriter(Stream);
		Reader = new BinaryReader(Stream);

		Id = Guid.Empty;
		Nickname = string.Empty;
	}

	public IPacket? ReadPacket() {
		var id = Stream.ReadByte();

		// 더 이상 읽을 바이트가 없다면 리턴
		if (id == -1) return null;

		try {
			// 타입에 맞는 패킷 객체 생성
			var packetType = (PacketType)id;

			var packet = packetType.CreatePacket(Reader);
			Logger.Debug($"[C({ToString()}) -> S] {packet}");
			return packet;
		}
		catch (ArgumentOutOfRangeException) {
			throw;
		}
		catch (Exception e) {
			Console.WriteLine(e);
			return null;
		}
	}

	public void SendPacket(IPacket packet) {
		if (!Stream.CanWrite) return;
		if (!Client.Connected) {
			Logger.Error($"[S -> C({ToString()})] Cannot send packet due to disconnected: {packet}");
			return;
		}
		Logger.Debug($"[S -> C({ToString()})] {packet}");
		Writer.Write(packet);
	}

	public override string ToString() {
		return string.IsNullOrWhiteSpace(Nickname) ? $"{Endpoint}" : Nickname;
	}

	public void Dispose() {
		Stream.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		Client.Dispose();
		GC.SuppressFinalize(this);
	}

	protected bool Equals(PlayerConnection other) {
		if (Id != Guid.Empty && other.Id != Guid.Empty) {
			return Id.Equals(other.Id);
		}
		return Endpoint != null && Equals(Endpoint, other.Endpoint);
	}

	public override bool Equals(object? obj) {
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((PlayerConnection)obj);
	}

	public override int GetHashCode() {
		if (Id != Guid.Empty) return Id.GetHashCode();
		if (Endpoint != null) return Endpoint.GetHashCode();

		throw new Exception("Can't get hash code of PlayerConnection without Id or Endpoint");
	}
}
