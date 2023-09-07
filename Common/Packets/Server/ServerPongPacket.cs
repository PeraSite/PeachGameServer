using System;
using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Server {
	public struct ServerPongPacket : IPacket {
		public PacketType Type => PacketType.ServerPong;

		public string ClientId { get; private set; }

		public ServerPongPacket(string clientId) {
			ClientId = clientId;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(ClientId);
		}

		public void Deserialize(BinaryReader reader) {
			ClientId = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ServerPongPacket)} {{ {nameof(ClientId)}: {ClientId} }}";
		}
	}
}
