using System.IO;

namespace PeachGame.Common.Packets.Server {
	public struct ServerResponseJoinRoomPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseJoinRoom;

		public bool Success { get; private set; }
		public string ErrorMessage { get; private set; }

		public void Serialize(BinaryWriter writer) {
			writer.Write(Success);
			writer.Write(ErrorMessage);
		}

		public void Deserialize(BinaryReader reader) {
			Success = reader.ReadBoolean();
			ErrorMessage = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ServerResponseJoinRoomPacket)} {{ {nameof(Success)}: {Success}, {nameof(ErrorMessage)}: {ErrorMessage}}}";
		}
	}
}
