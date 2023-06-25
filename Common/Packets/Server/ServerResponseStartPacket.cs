using System.IO;

namespace PeachGame.Common.Packets.Server {
	public struct ServerResponseStartPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseStart;

		public int RoomId { get; private set; }
		public int Seed { get; private set; }

		public bool Success { get; private set; }
		public string ErrorMessage { get; private set; }

		public ServerResponseStartPacket(int roomId, int seed) {
			RoomId = roomId;
			Seed = seed;
			Success = true;
			ErrorMessage = string.Empty;
		}

		public ServerResponseStartPacket(string errorMessage) {
			RoomId = -1;
			Seed = -1;
			Success = false;
			ErrorMessage = errorMessage;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
			writer.Write(Seed);
			writer.Write(Success);
			writer.Write(ErrorMessage);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
			Seed = reader.ReadInt32();
			Success = reader.ReadBoolean();
			ErrorMessage = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ServerResponseStartPacket)} {{ {nameof(Success)}: {Success}, {nameof(ErrorMessage)}: {ErrorMessage}, {nameof(RoomId)}: {RoomId}}}";
		}
	}
}
