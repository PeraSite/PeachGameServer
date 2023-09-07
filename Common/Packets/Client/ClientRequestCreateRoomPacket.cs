using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestCreateRoomPacket : IPacket {
		public PacketType Type => PacketType.ClientRequestCreateRoom;

		public string Name { get; private set; }

		public ClientRequestCreateRoomPacket(string name) {
			Name = name;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(Name);
		}

		public void Deserialize(BinaryReader reader) {
			Name = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ClientRequestCreateRoomPacket)} {{ {nameof(Name)}: {Name} }}";
		}
	}
}
