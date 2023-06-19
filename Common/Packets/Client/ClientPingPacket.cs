using System;
using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Client {
	public struct ClientPingPacket : IPacket {
		public PacketType Type => PacketType.ClientPing;

		public Guid ClientId { get; private set; }
		public string Nickname { get; set; }

		public ClientPingPacket(Guid clientId, string nickname) {
			ClientId = clientId;
			Nickname = nickname;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(ClientId);
			writer.Write(Nickname);
		}

		public void Deserialize(BinaryReader reader) {
			ClientId = reader.ReadGuid();
			Nickname = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ClientPingPacket)} {{ {nameof(ClientId)}: {ClientId}, {nameof(Nickname)}: {Nickname} }}";
		}
	}
}
