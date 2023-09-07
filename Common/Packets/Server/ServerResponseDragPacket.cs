using System.IO;
using System.Linq;

namespace PeachGame.Common.Packets.Server {
	public struct ServerResponseDragPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseDrag;

		public string PlayerId;
		public (int x, int y)[] Positions { get; private set; }

		public ServerResponseDragPacket(string playerId, (int x, int y)[] positions) {
			PlayerId = playerId;
			Positions = positions;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(PlayerId);
			writer.Write(Positions.Length);
			foreach (var position in Positions) {
				writer.Write(position.x);
				writer.Write(position.y);
			}
		}

		public void Deserialize(BinaryReader reader) {
			PlayerId = reader.ReadString();
			var length = reader.ReadInt32();
			Positions = new (int x, int y)[length];
			for (var i = 0; i < length; i++) {
				var x = reader.ReadInt32();
				var y = reader.ReadInt32();
				Positions[i] = (x, y);
			}
		}

		public override string ToString() {
			return $"{nameof(ServerResponseDragPacket)} {{ {nameof(PlayerId)}: {PlayerId}, {string.Join(", ", Positions.Select(pair => $"{pair.x}-{pair.y}"))} }}";
		}
	}
}
