using System.Collections.Generic;
using System.IO;
using System.Linq;
using PeachGame.Common.Models;

namespace PeachGame.Common.Packets.Server {
	public struct ServerResponseRoomListPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseRoomList;

		public List<RoomInfo> InfoList { get; private set; }

		public ServerResponseRoomListPacket(List<RoomInfo> infoList) {
			InfoList = infoList;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(InfoList.Count);
			foreach (var info in InfoList) {
				writer.Write(info.RoomId);
				writer.Write(info.Name);
				writer.Write((byte)info.State);
				writer.Write(info.CurrentPlayers);
				writer.Write(info.MaxPlayers);
			}
		}

		public void Deserialize(BinaryReader reader) {
			var count = reader.ReadInt32();
			InfoList = new List<RoomInfo>(count);
			for (var i = 0; i < count; i++) {
				var info = new RoomInfo {
					RoomId = reader.ReadInt32(),
					Name = reader.ReadString(),
					State = (RoomState)reader.ReadByte(),
					CurrentPlayers = reader.ReadInt32(),
					MaxPlayers = reader.ReadInt32(),
				};
				InfoList.Add(info);
			}
		}

		public override string ToString() {
			return $"{nameof(ServerResponseRoomListPacket)} {{{string.Join(", ", InfoList.Select(info => $"{info.Name} ({info.CurrentPlayers}/{info.MaxPlayers})"))}}}";
		}
	}
}
