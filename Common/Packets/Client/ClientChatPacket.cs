using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientChatPacket : IPacket {
		public PacketType Type => PacketType.ClientChat;

		public string Nickname { get; private set; }
		public string Message { get; private set; }

		public ClientChatPacket(string nickname, string message) {
			Nickname = nickname;
			Message = message;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(Nickname);
			writer.Write(Message);
		}

		public void Deserialize(BinaryReader reader) {
			Nickname = reader.ReadString();
			Message = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ClientChatPacket)} {{ {nameof(Nickname)}: {Nickname}, {nameof(Message)}: {Message} }}";
		}
	}
}
