using System.Collections.Generic;
using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Models {
	public struct LobbyState : ISerializable {
		public RoomInfo RoomInfo;
		public List<LobbyPlayer> Players;

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomInfo);
			writer.Write(Players);
		}

		public void Deserialize(BinaryReader reader) {
			RoomInfo = reader.ReadSerializable<RoomInfo>();
			Players = reader.ReadSerializableList<LobbyPlayer>();
		}
	}
}
