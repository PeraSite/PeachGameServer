using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Models {
	public struct RoomInfo : ISerializable {
		public int RoomId;
		public string Name;
		public RoomState State;
		public List<PlayerInfo> Players;
		public int MaxPlayers;
		public string Owner;
		public Dictionary<string, int> Score;
		public int LeftTime;

		public int CurrentPlayers => Players.Count;

		public void Serialize(BinaryWriter writer) {
			writer.Write(RoomId);
			writer.Write(Name);
			writer.Write((byte) State);
			writer.Write(Players);
			writer.Write(MaxPlayers);
			writer.Write(Owner);
			writer.Write(Score.Count);
			foreach (KeyValuePair<string,int> pair in Score) {
				var name = pair.Key;
				var score = pair.Value;
				writer.Write(name);
				writer.Write(score);
			}
			writer.Write(LeftTime);
		}

		public void Deserialize(BinaryReader reader) {
			RoomId = reader.ReadInt32();
			Name = reader.ReadString();
			State = (RoomState) reader.ReadByte();
			Players = reader.ReadSerializableList<PlayerInfo>();
			MaxPlayers = reader.ReadInt32();
			Owner = reader.ReadString();
			Score = new Dictionary<string, int>();
			var scoreSize = reader.ReadInt32();
			for (int i = 0; i < scoreSize; i++) {
				var playerId = reader.ReadString();
				var score = reader.ReadInt32();
				Score[playerId] = score;
			}
			LeftTime = reader.ReadInt32();
		}

		public override string ToString() {
			return $"{nameof(RoomInfo)} {{ " +
			       $"Id:{RoomId}, " +
			       $"Name:{Name}, " +
			       $"State:{State}, " +
			       $"Owner: {Owner}, " +
			       $"Players({CurrentPlayers}/{MaxPlayers}): {string.Join(", ", Players.Select(x => x.Nickname))}, " +
			       $"Score: {string.Join(", ", Score.Select(x => $"{x.Key}={x.Value}"))}, " +
			       $"LeftTime: {LeftTime}}}";
		}
	}
}
