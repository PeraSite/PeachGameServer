using System.IO;
using PeachGame.Common.Models;
using PeachGame.Common.Serialization;

namespace PeachGame.Common.Packets.Server {
	public struct ServerLobbyStatusPacket : IPacket {
		public PacketType Type => PacketType.ServerResponseRoomList;

		public LobbyState LobbyState;

		public ServerLobbyStatusPacket(LobbyState lobbyState) {
			LobbyState = lobbyState;
		}

		public void Serialize(BinaryWriter writer) {
			writer.Write(LobbyState);
		}

		public void Deserialize(BinaryReader reader) {
			LobbyState = reader.ReadSerializable<LobbyState>();
		}

		public override string ToString() {
			return $"{nameof(ServerLobbyStatusPacket)} {{ Name:{LobbyState.RoomInfo.Name}, Players:({LobbyState.RoomInfo.CurrentPlayers}/{LobbyState.RoomInfo.MaxPlayers}) }}";
		}
	}
}
