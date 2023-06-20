using System.IO;

namespace PeachGame.Common.Packets.Server {
	public struct ServerLobbyAnnouncePacket : IPacket {
		public PacketType Type => PacketType.ServerLobbyAnnounce;

		public string Message { get; private set; }

		public ServerLobbyAnnouncePacket(string message) {
			Message = message;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(Message);
		}

		public void Deserialize(BinaryReader reader) {
			Message = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ServerLobbyAnnouncePacket)} {{ {nameof(Message)}:{Message} }}";
		}
	}
}
