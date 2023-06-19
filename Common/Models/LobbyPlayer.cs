using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Models {
	public struct LobbyPlayer : ISerializable {
		public string Nickname;
		public bool IsOwner;

		public void Serialize(BinaryWriter writer) {
			writer.Write(Nickname);
			writer.Write(IsOwner);
		}

		public void Deserialize(BinaryReader reader) {
			Nickname = reader.ReadString();
			IsOwner = reader.ReadBoolean();
		}
	}
}
