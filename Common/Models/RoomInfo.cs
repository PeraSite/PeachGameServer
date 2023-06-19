using System.Collections.Generic;
using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Models {
	public struct RoomInfo : ISerializable {
		public int RoomId;
		public string Name;
		public RoomState State;
		public List<PlayerInfo> Players;
		public int MaxPlayers;

		public int CurrentPlayers => Players.Count;

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
			writer.Write(Name);
			writer.Write((byte)State);
			writer.Write(Players);
			writer.Write(MaxPlayers);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
			Name = reader.ReadString();
			State = (RoomState)reader.ReadByte();
			Players = reader.ReadSerializableList<PlayerInfo>();
			MaxPlayers = reader.ReadInt32();
		}
	}
}
