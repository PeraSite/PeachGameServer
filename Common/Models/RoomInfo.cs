using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Models {
	public struct RoomInfo : ISerializable {
		public int RoomId;
		public string Name;
		public RoomState State;
		public int CurrentPlayers;
		public int MaxPlayers;

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
			writer.Write(Name);
			writer.Write((byte)State);
			writer.Write(CurrentPlayers);
			writer.Write(MaxPlayers);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
			Name = reader.ReadString();
			State = (RoomState)reader.ReadByte();
			CurrentPlayers = reader.ReadInt32();
			MaxPlayers = reader.ReadInt32();
		}
	}
}
