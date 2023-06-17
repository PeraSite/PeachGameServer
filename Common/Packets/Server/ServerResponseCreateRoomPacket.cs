using System.IO;

namespace PeachGame.Common.Packets.Server {
	public struct ServerResponseCreateRoomPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseCreateRoom;

		public int RoomId { get; private set; }

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
		}

		public override string ToString() {
			return $"{nameof(ServerResponseCreateRoomPacket)} {{ {nameof(RoomId)}: {RoomId}}}";
		}
	}
}
