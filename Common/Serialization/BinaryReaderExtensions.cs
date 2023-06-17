using System;
using System.IO;
using PeachGame.Common.Packets;

namespace PeachGame.Common.Serialization {
	public static class BinaryReaderExtensions {
		public static Guid ReadGuid(this BinaryReader reader) {
			return new Guid(reader.ReadBytes(16));
		}

		public static IPacket ReadPacket(this BinaryReader reader) {
			var packetId = reader.ReadInt32();
			var packetType = (PacketType)packetId;
			return packetType.CreatePacket(reader);
		}
	}
}
