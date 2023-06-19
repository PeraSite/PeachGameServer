using System;
using System.Collections.Generic;
using System.IO;
using PeachGame.Common.Packets;

namespace PeachGame.Common.Serialization {
	public static class BinaryWriterExtensions {
		public static void Write(this BinaryWriter writer, Guid guid) {
			writer.Write(guid.ToByteArray());
		}

		public static void Write<TElement>(this BinaryWriter writer, TElement serializable) where TElement : ISerializable {
			serializable.Serialize(writer);
		}

		public static void Write<TElement>(this BinaryWriter writer, IList<TElement> serializableList)
			where TElement : ISerializable {
			writer.Write(serializableList.Count);
			foreach (var serializable in serializableList) {
				writer.Write(serializable);
			}
		}

		public static void Write(this BinaryWriter writer, IPacket packet) {
			writer.Write((byte)packet.Type);
			packet.Serialize(writer);
		}
	}
}
