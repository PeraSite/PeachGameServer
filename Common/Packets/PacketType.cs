using System;
using System.IO;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;

namespace PeachGame.Common.Packets {
	public enum PacketType : byte {
		ClientPing,
		ServerPong,
	}

	public static class PacketTypes {
		public static IPacket CreatePacket(this PacketType type, BinaryReader reader) {
			IPacket packet = type switch {
				PacketType.ClientPing => new ClientPingPacket(),
				PacketType.ServerPong => new ServerPongPacket(),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
			packet.Deserialize(reader);
			return packet;
		}
	}
}
