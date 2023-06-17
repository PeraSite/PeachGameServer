using System;
using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Server {
	public struct ServerPongPacket : IPacket {
		public PacketType Type => PacketType.ServerPong;

		public Guid ClientId { get; private set; }

		public ServerPongPacket(Guid clientId) {
			ClientId = clientId;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(ClientId);
		}

		public void Deserialize(BinaryReader reader) {
			ClientId = reader.ReadGuid();
		}

		public override string ToString() {
			return $"{nameof(ServerPongPacket)} {{ {nameof(ClientId)}: {ClientId} }}";
		}
	}
}
