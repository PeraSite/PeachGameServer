using System.IO;
using PeachGame.Common.Models;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Server {
	public struct ServerRoomStatePacket : IPacket {
		public PacketType Type => PacketType.ServerRoomState;

		public RoomInfo RoomInfo;

		public ServerRoomStatePacket(RoomInfo roomInfo) {
			RoomInfo = roomInfo;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomInfo);
		}

		public void Deserialize(BinaryReader reader) {
			RoomInfo = reader.ReadSerializable<RoomInfo>();
		}

		public override string ToString() {
			return $"{nameof(ServerRoomStatePacket)} {RoomInfo}";
		}
	}
}
