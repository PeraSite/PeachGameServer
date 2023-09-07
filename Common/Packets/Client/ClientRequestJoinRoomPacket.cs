using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestJoinRoomPacket : IPacket {
		public PacketType Type => PacketType.ClientRequestJoinRoom;

		public int RoomId { get; private set; }

		public ClientRequestJoinRoomPacket(int roomId) {
			RoomId = roomId;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
		}

		public override string ToString() {
			return $"{nameof(ClientRequestJoinRoomPacket)} {{ {nameof(RoomId)}: {RoomId} }}";
		}
	}
}
