using System.IO;

namespace PeachGame.Common.Serialization {
	public interface ISerializable {
		void Serialize(BinaryWriter writer);

		void Deserialize(BinaryReader reader);
	}
}
