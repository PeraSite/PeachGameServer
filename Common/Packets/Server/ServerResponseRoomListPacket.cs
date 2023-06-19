using System.Collections.Generic;
using System.IO;
using System.Linq;
using PeachGame.Common.Models;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Server {
	public struct ServerResponseRoomListPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseRoomList;

		public List<RoomInfo> InfoList { get; private set; }

		public ServerResponseRoomListPacket(List<RoomInfo> infoList) {
			InfoList = infoList;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(InfoList);
		}

		public void Deserialize(BinaryReader reader) {
			InfoList = reader.ReadSerializableList<RoomInfo>();
		}

		public override string ToString() {
			return $"{nameof(ServerResponseRoomListPacket)} {{{string.Join(", ", InfoList.Select(info => $"{info.Name} ({info.CurrentPlayers}/{info.MaxPlayers})"))}}}";
		}
	}
}
