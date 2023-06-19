using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using PeachGame.Common.Models;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Server;

namespace PeachGame.Server;

public class Room {
	private static ILog Logger = LogManager.GetLogger(typeof(Room));
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
	public PlayerConnection Owner;
	public readonly List<PlayerConnection> Players;

	public Room(string roomName, int roomId, PlayerConnection owner) {
		RoomName = roomName;
		RoomId = roomId;
		Players = new List<PlayerConnection> {
			owner
		};
		Owner = owner;
		_state = RoomState.Waiting;
	}

	public void AddPlayer(PlayerConnection playerConnection) {
		if (IsFull()) throw new InvalidOperationException("Room is full");

		Logger.Debug($"Player {playerConnection.Nickname} join to {RoomName}({RoomId})");

		// 플레이어 목록 추가 후 상태 Broadcast
		Players.Add(playerConnection);
		BroadcastState();
	}

	public void RemovePlayer(PlayerConnection playerConnection) {
		Logger.Debug($"Player {playerConnection.Nickname} left room {RoomName}({RoomId})");

		// 플레이어 목록 제거 후 상태 Broadcast
		Players.Remove(playerConnection);

		// 만약 방장이 나갔다면
		if (Equals(Owner, playerConnection)) {
			// 방장을 첫 플레이어로 변경
			PlayerConnection? newOwner = Players.FirstOrDefault();

			// 첫 플레이어가 존재할 때만 변경
			if (newOwner != null)
				Owner = newOwner;
		}

		BroadcastState();
	}

	private void StartGame() {
		State = RoomState.Playing;
	}

	private void StopGame() {
		State = RoomState.Ending;
	}

	public RoomInfo GetRoomInfo() {
		return new RoomInfo {
			RoomId = RoomId,
			Name = RoomName,
			Players = Players.Select(player => new PlayerInfo {
				IsOwner = Equals(player, Owner),
				Nickname = player.Nickname
			}).ToList(),
			MaxPlayers = MAX_PLAYER,
			State = _state
		};
	}

	public void BroadcastState() {
		BroadcastPacket(new ServerRoomStatePacket(GetRoomInfo()));
	}

	public void BroadcastPacket(IPacket packet) {
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

	protected bool Equals(Room other) {
		return RoomId == other.RoomId;
	}
	public override bool Equals(object? obj) {
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((Room)obj);
	}
	public override int GetHashCode() {
		return RoomId;
	}
}
