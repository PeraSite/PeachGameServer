using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using PeachGame.Common.Models;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Server;

namespace PeachGame.Server;

public class Room {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(Room));
	private const int MAX_PLAYER = 4;
	private const int PLAY_TIME = 60 * 2; // 2분

	// 방 상태 관련
	public readonly string RoomName;
	public readonly int RoomId;
	public PlayerConnection Owner;
	public readonly List<PlayerConnection> Players;
	private RoomState _state;

	// 플레이 로직 관련
	public int Seed { get; }
	private Random _random;
	private int _leftTime;
	private CancellationTokenSource _tickCts = new CancellationTokenSource();
	private Dictionary<PlayerConnection, int> _score;

	public Room(string roomName, int roomId, PlayerConnection owner) {
		RoomName = roomName;
		RoomId = roomId;
		Players = new List<PlayerConnection> {
			owner
		};
		Owner = owner;
		_state = RoomState.Waiting;
		_leftTime = -1;
		_score = new Dictionary<PlayerConnection, int>();

		// 현재 Tick Count로 랜덤 시드 설정 후 랜덤 생성
		Seed = Environment.TickCount;
		_random = new Random(Seed);
	}

	#region 플레이어 입장 처리 로직
	public void AddPlayer(PlayerConnection playerConnection) {
		if (IsFull()) throw new InvalidOperationException("Room is full");

		Logger.Debug($"Player {playerConnection.Nickname} join to {RoomName}({RoomId})");

		// 플레이어 목록 추가 후 상태 Broadcast
		Players.Add(playerConnection);
		BroadcastState();

		BroadcastPacket(new ServerLobbyAnnouncePacket($"[공지] {playerConnection.Nickname}님이 입장하셨습니다."));
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
			if (newOwner != null) {
				Owner = newOwner;
				BroadcastPacket(new ServerLobbyAnnouncePacket($"[공지] {newOwner.Nickname}님이 방장이 되었습니다."));
			}
		}

		BroadcastState();
		BroadcastPacket(new ServerLobbyAnnouncePacket($"[공지] {playerConnection.Nickname}님이 퇴장하셨습니다."));
	}
  #endregion

	public void StartGame() {
		_state = RoomState.Playing;
		_leftTime = PLAY_TIME;
		_tickCts = new CancellationTokenSource();
		_score.Clear();
		BroadcastState();

		// 메인 Update 로직
		Task.Run(async () => {
			while (!_tickCts.Token.IsCancellationRequested) {
				// 남은 시간 초 업데이트
				_leftTime--;
				BroadcastState();

				// 게임 종료 체크
				if (_leftTime <= 0) {
					StopGame();
				}
				await Task.Delay(TimeSpan.FromSeconds(1));
			}
		}, _tickCts.Token);
	}

	public void StopGame() {
		_state = RoomState.Ending;
		_tickCts.Cancel();
		BroadcastState();
	}

	#region 유틸 함수
	public RoomInfo GetRoomInfo() {
		return new RoomInfo {
			RoomId = RoomId,
			Name = RoomName,
			Players = Players.Select(player => new PlayerInfo {
				IsOwner = Equals(player, Owner),
				Nickname = player.Nickname
			}).ToList(),
			MaxPlayers = MAX_PLAYER,
			State = _state,
			Owner = Owner.Id,
			Score = _score.ToDictionary(x => x.Key.Nickname, x => x.Value),
			LeftTime = _leftTime
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
	public bool IsAvailable() => Players.Count < MAX_PLAYER && _state == RoomState.Waiting;
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
  #endregion
}
