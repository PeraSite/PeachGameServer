using System;
using System.IO;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Client {
	public struct ClientSelectRangePacket : IPacket {
		public PacketType Type => PacketType.ClientSelectRange;

		public Guid ClientId { get; private set; }
		public bool Dragging { get; private set; }
		public float MinX { get; private set; }
		public float MaxX { get; private set; }
		public float MinY { get; private set; }
		public float MaxY { get; private set; }

		public ClientSelectRangePacket(Guid clientId, bool dragging, float minX, float maxX, float minY, float maxY) {
			ClientId = clientId;
			Dragging = dragging;
			MinX = minX;
			MaxX = maxX;
			MinY = minY;
			MaxY = maxY;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(ClientId);
			writer.Write(Dragging);
			writer.Write(MinX);
			writer.Write(MaxX);
			writer.Write(MinY);
			writer.Write(MaxY);
		}

		public void Deserialize(BinaryReader reader) {
			ClientId = reader.ReadGuid();
			Dragging = reader.ReadBoolean();
			MinX = reader.ReadSingle();
			MaxX = reader.ReadSingle();
			MinY = reader.ReadSingle();
			MaxY = reader.ReadSingle();
		}

		public override string ToString() {
			return $"{nameof(ClientSelectRangePacket)} {{ {ClientId}, {Dragging}, {MinX}, {MaxX}, {MinY}, {MaxY} }}";
		}
	}
}
