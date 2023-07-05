using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientPingPacket : IPacket {
		public PacketType Type => PacketType.ClientPing;

		public string Nickname { get; set; }

		public ClientPingPacket(string nickname) {
			Nickname = nickname;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(Nickname);
		}

		public void Deserialize(BinaryReader reader) {
			Nickname = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ClientPingPacket)} {{ {nameof(Nickname)}: {Nickname} }}";
		}
	}
}
