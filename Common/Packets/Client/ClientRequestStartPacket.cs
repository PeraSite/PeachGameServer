using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestStartPacket : IPacket {
		public PacketType Type => PacketType.ClientRequestStart;

		public int RoomId { get; private set; }

		public ClientRequestStartPacket(int roomId) {
			RoomId = roomId;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
		}

		public override string ToString() {
			return $"{nameof(ClientRequestStartPacket)} {{ {nameof(RoomId)}: {RoomId}}}";
		}
	}
}
