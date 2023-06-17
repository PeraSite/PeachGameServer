using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets {
	public interface IPacket : ISerializable {
		public PacketType Type { get; }
	}
}
