using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestCreateRoomPacket : IPacket {
		public PacketType Type => PacketType.ClientPing;

		public void Serialize(BinaryWriter writer) { }

		public void Deserialize(BinaryReader reader) { }

		public override string ToString() {
			return $"{nameof(ClientRequestRoomListPacket)} {{}}";
		}
	}
}
