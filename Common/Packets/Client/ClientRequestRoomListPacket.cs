using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestRoomListPacket : IPacket {
		public PacketType Type => PacketType.ClientRequestRoomList;

		public void Serialize(BinaryWriter writer) { }

		public void Deserialize(BinaryReader reader) { }

		public override string ToString() {
			return $"{nameof(ClientRequestRoomListPacket)} {{}}";
		}
	}
}
