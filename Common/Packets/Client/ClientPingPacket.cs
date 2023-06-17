using System;
using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Client {
	public struct ClientPingPacket : IPacket {
		public PacketType Type => PacketType.ClientPing;

		public Guid ClientId { get; private set; }

		public ClientPingPacket(Guid clientId) {
			ClientId = clientId;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(ClientId);
		}

		public void Deserialize(BinaryReader reader) {
			ClientId = reader.ReadGuid();
		}

		public override string ToString() {
			return $"{nameof(ClientPingPacket)} {{ {nameof(ClientId)}: {ClientId} }}";
		}
	}
}
