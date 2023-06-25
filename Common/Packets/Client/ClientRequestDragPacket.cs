using System.IO;
using System.Linq;

namespace PeachGame.Common.Packets.Client {
	public struct ClientRequestDragPacket : IPacket {
		public PacketType Type => PacketType.ClientRequestDrag;

		public (int x, int y)[] Positions { get; private set; }

		public ClientRequestDragPacket((int x, int y)[] positions) {
			Positions = positions;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(Positions.Length);
			foreach (var position in Positions) {
				writer.Write(position.x);
				writer.Write(position.y);
			}
		}

		public void Deserialize(BinaryReader reader) {
			var length = reader.ReadInt32();
			Positions = new (int x, int y)[length];
			for (var i = 0; i < length; i++) {
				var x = reader.ReadInt32();
				var y = reader.ReadInt32();
				Positions[i] = (x, y);
			}
		}

		public override string ToString() {
			return $"{nameof(ClientRequestDragPacket)} {{ {string.Join(", ", Positions.Select(pair => $"{pair.x}-{pair.y}"))} }}";
		}
	}
}
