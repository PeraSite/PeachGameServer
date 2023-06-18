using System;
using System.Collections.Generic;
using PeachGame.Common.Models;
using PeachGame.Common.Packets;

namespace PeachGame.Server;

public class Room {
	private const int MAX_PLAYER = 4;

	private RoomState _state;
	public RoomState State {
		get => _state;
		private set {
			_state = value;
			BroadcastState();
		}
	}

	public readonly string RoomName;
	public readonly int RoomId;
	public readonly PlayerConnection Owner;
	public readonly List<PlayerConnection> Players;

	public Room(string roomName, int roomId, PlayerConnection owner) {
		RoomName = roomName;
		RoomId = roomId;
		Players = new List<PlayerConnection>();
		_state = RoomState.Waiting;
		Owner = owner;
	}

	public RoomInfo GetRoomInfo() {
		return new RoomInfo {
			RoomId = RoomId,
			Name = RoomName,
			CurrentPlayers = Players.Count,
			MaxPlayers = MAX_PLAYER,
			State = _state
		};
	}

	public void AddPlayer(PlayerConnection playerConnection) {
		if (IsFull()) {
			throw new Exception("Can't add player to full room!");
		}

		if (_state != RoomState.Waiting) {
			throw new Exception("Can't add player to non-waiting room!");
		}

		// BroadcastPacket(new ServerRoomJoinPacket(playerId));
		//
		// // 기존에 사람이 있었다면
		// if (PlayerIds.Count > 0) {
		// 	foreach (var existId in PlayerIds.Values) {
		// 		playerConnection.SendPacket(new ServerRoomJoinPacket(existId));
		// 	}
		// }

		BroadcastState();

		if (IsFull()) {
			StartGame();
		}
	}

	public void RemovePlayer(PlayerConnection playerConnection) {
		// BroadcastPacket(new ServerRoomQuitPacket(id));
		BroadcastState();

		if (!IsFull()) {
			// 만약 누가 나갔는데 2명이 되지 않는다면(미래 대응) 게임 종료
			StopGame();
		}
	}

	private void StartGame() {
		State = RoomState.Playing;
	}

	private void StopGame() {
		State = RoomState.Ending;
	}

	private void BroadcastState() {
		// BroadcastPacket(new ServerRoomStatusPacket(_roomId, State, PlayerIds.Count, PlayerHP));
	}

	private void BroadcastPacket(IPacket packet) {
		foreach (PlayerConnection connection in Players) {
			connection.SendPacket(packet);
		}
	}

	public bool IsEmpty() => Players.Count == 0;
	public bool IsAvailable() => Players.Count < MAX_PLAYER && State == RoomState.Waiting;
	public bool IsFull() => Players.Count == MAX_PLAYER;

	public override string ToString() {
		return $"Room {RoomId} ({Players.Count}/{MAX_PLAYER})";
	}

	private static float Random(float min, float max) {
		return new Random().NextSingle() * (max - min) + min;
	}
}
