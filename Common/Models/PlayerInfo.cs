using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Models {
	public struct PlayerInfo : ISerializable {
		public string Nickname;
		public string Id;
		public bool IsOwner;

		public void Serialize(BinaryWriter writer) {
			writer.Write(Nickname);
			writer.Write(Id);
			writer.Write(IsOwner);
		}

		public void Deserialize(BinaryReader reader) {
			Nickname = reader.ReadString();
			Id = reader.ReadString();
			IsOwner = reader.ReadBoolean();
		}
	}
}
