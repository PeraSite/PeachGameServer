using System.IO;

namespace PeachGame.Common.Packets.Server {
	public struct ServerResponseQuitRoomPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseQuitRoom;

		public bool Success { get; private set; }
		public string ErrorMessage { get; private set; }

		public ServerResponseQuitRoomPacket(bool success) {
			Success = success;
			ErrorMessage = string.Empty;
		}

		public ServerResponseQuitRoomPacket(bool success, string errorMessage) {
			Success = success;
			ErrorMessage = errorMessage;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(Success);
			writer.Write(ErrorMessage);
		}

		public void Deserialize(BinaryReader reader) {
			Success = reader.ReadBoolean();
			ErrorMessage = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ServerResponseQuitRoomPacket)} {{ {nameof(Success)}: {Success}, {nameof(ErrorMessage)}: {ErrorMessage}}}";
		}
	}
}
