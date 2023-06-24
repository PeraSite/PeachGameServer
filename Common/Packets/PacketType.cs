using System;
using System.IO;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;

namespace PeachGame.Common.Packets {
	public enum PacketType : byte {
		ClientPing,
		ServerPong,

		ClientRequestRoomList,
		ServerResponseRoomList,

		ClientRequestCreateRoom,
		ServerResponseCreateRoom,

		ClientRequestJoinRoom,
		ServerResponseJoinRoom,

		ClientRequestQuitRoom,
		ServerResponseQuitRoom,

		ServerRoomState,

		ClientChat,
		ServerLobbyAnnounce,

		ClientRequestStartPacket,
	}

	public static class PacketTypes {
		public static IPacket CreatePacket(this PacketType type, BinaryReader reader) {
			IPacket packet = type switch {
				PacketType.ClientPing => new ClientPingPacket(),
				PacketType.ServerPong => new ServerPongPacket(),

				PacketType.ClientRequestRoomList => new ClientRequestRoomListPacket(),
				PacketType.ServerResponseRoomList => new ServerResponseRoomListPacket(),

				PacketType.ClientRequestCreateRoom => new ClientRequestCreateRoomPacket(),
				PacketType.ServerResponseCreateRoom => new ServerResponseCreateRoomPacket(),

				PacketType.ClientRequestJoinRoom => new ClientRequestJoinRoomPacket(),
				PacketType.ServerResponseJoinRoom => new ServerResponseJoinRoomPacket(),

				PacketType.ClientRequestQuitRoom => new ClientRequestQuitRoomPacket(),
				PacketType.ServerResponseQuitRoom => new ServerResponseQuitRoomPacket(),

				PacketType.ServerRoomState => new ServerRoomStatePacket(),

				PacketType.ClientChat => new ClientChatPacket(),
				PacketType.ServerLobbyAnnounce => new ServerLobbyAnnouncePacket(),

				PacketType.ClientRequestStartPacket => new ClientRequestStartPacket(),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
			packet.Deserialize(reader);
			return packet;
		}
	}
}
