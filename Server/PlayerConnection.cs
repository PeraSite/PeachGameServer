using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using PeachGame.Common.Packets;
using PeachGame.Common.Serialization;

namespace PeachGame.Server;

public class PlayerConnection : IDisposable {
	public TcpClient Client { get; }
	public NetworkStream Stream { get; }
	public BinaryReader Reader { get; }
	public BinaryWriter Writer { get; }
	public IPEndPoint Ip => (IPEndPoint)Client.Client.RemoteEndPoint!;

	public PlayerConnection(TcpClient client) {
		Client = client;
		Stream = Client.GetStream();
		Writer = new BinaryWriter(Stream);
		Reader = new BinaryReader(Stream);
	}

	public IPacket? ReadPacket() {
		var id = Stream.ReadByte();

		// 더 이상 읽을 바이트가 없다면 리턴
		if (id == -1) return null;

		try {
			// 타입에 맞는 패킷 객체 생성
			var packetType = (PacketType)id;

			var packet = packetType.CreatePacket(Reader);
			Console.WriteLine($"[C({ToString()}) -> S] {packet}");
			return packet;
		}
		catch (ArgumentOutOfRangeException) {
			return null;
		}
		catch (Exception e) {
			Console.WriteLine(e);
			return null;
		}
	}

	public void SendPacket(IPacket packet) {
		if (!Stream.CanWrite) return;
		if (!Client.Connected) {
			Console.WriteLine($"[S -> C({ToString()})] Cannot send packet due to disconnected: {packet}");
			return;
		}
		Console.WriteLine($"[S -> C({ToString()})] {packet}");
		Writer.Write(packet);
	}

	public override string ToString() {
		return $"{Ip.Address}:{Ip.Port}";
	}

	public void Dispose() {
		Stream.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		Client.Dispose();
		GC.SuppressFinalize(this);
	}

	protected bool Equals(PlayerConnection other) {
		return Ip.Equals(other.Ip);
	}

	public override bool Equals(object? obj) {
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((PlayerConnection)obj);
	}

	public override int GetHashCode() {
		return Ip.GetHashCode();
	}
}
