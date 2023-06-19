using System;
using System.Collections.Generic;
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

		public static T ReadSerializable<T>(this BinaryReader reader) where T : ISerializable, new() {
			var serializable = new T();
			serializable.Deserialize(reader);
			return serializable;
		}

		public static List<T> ReadSerializableList<T>(this BinaryReader reader) where T : ISerializable, new() {
			var count = reader.ReadInt32();
			var list = new List<T>(count);
			for (var i = 0; i < count; i++) {
				list.Add(reader.ReadSerializable<T>());
			}
			return list;
		}
	}
}
