using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestQuitRoomPacket : IPacket {
		public PacketType Type => PacketType.ClientRequestQuitRoom;

		public int RoomId { get; private set; }

		public ClientRequestQuitRoomPacket(int roomId) {
			RoomId = roomId;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
		}

		public override string ToString() {
			return $"{nameof(ClientRequestQuitRoomPacket)} {{ {nameof(RoomId)}: {RoomId} }}";
		}
	}
}
