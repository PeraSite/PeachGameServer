using System.IO;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestRoomListPacket : IPacket {
		public PacketType Type => PacketType.ClientRequestRoomList;

		public string Name { get; private set; }

		public ClientRequestRoomListPacket(string name) {
			Name = name;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(Name);
		}

		public void Deserialize(BinaryReader reader) {
			Name = reader.ReadString();
		}

		public override string ToString() {
			return $"{nameof(ClientRequestCreateRoomPacket)} {{ {nameof(Name)}: {Name}}}";
		}
	}
}
