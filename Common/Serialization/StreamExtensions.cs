using System.IO;

namespace PeachGame.Common.Serialization {
	public static class StreamExtensions {
		public static void Write(this Stream stream, ISerializable serializable) {
			using var writer = new BinaryWriter(stream);
			serializable.Serialize(writer);
		}

		public static T Read<T>(this Stream stream) where T : ISerializable, new() {
			using var reader = new BinaryReader(stream);
			var serializable = new T();
			serializable.Deserialize(reader);
			return serializable;
		}
	}
}
