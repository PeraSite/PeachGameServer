using PeachGame.Common.Models;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Server;
using PeachGame.Common.Serialization;

var playerList = new List<PlayerInfo>() {
	new PlayerInfo() {
		Nickname = "Player 1",
		IsOwner = true
	},
	new PlayerInfo() {
		Nickname = "Player 2",
		IsOwner = false
	},
};

var roomInfo = new RoomInfo() {
	Name = "Test Room",
	State = RoomState.Waiting,
	MaxPlayers = 4,
	RoomId = 0,
	Players = playerList
};

var packet = new ServerRoomStatePacket(roomInfo);

using var ms = new MemoryStream();
using var writer = new BinaryWriter(ms);
writer.Write((IPacket)packet);

var bytes = ms.ToArray();
Console.WriteLine($"Bytes: {string.Join(", ", bytes)}");

using var reader = new BinaryReader(new MemoryStream(bytes));
var deserializedRoomInfo = reader.ReadPacket();
Console.WriteLine($"Deserialized: {deserializedRoomInfo}");
