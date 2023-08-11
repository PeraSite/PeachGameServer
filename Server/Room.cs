using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using PeachGame.Common.Models;
using PeachGame.Common.Packets;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Packets.Server;

namespace PeachGame.Server;

public class Room {
	private static readonly ILog Logger = LogManager.GetLogger(typeof(Room));
	private const int MAX_PLAYER = 4;
	private const int PLAY_TIME = 60 * 2; // 2분
	private const int PEACH_COUNT = 200;
	private const int PEACH_COLUMN = 20;

	private GameServer _server;

	// 방 상태 관련
	public readonly string RoomName;
	public readonly int RoomId;
	public PlayerConnection Owner;
	public readonly List<PlayerConnection> Players;
	private RoomState _state;

	// 플레이 로직 관련
	public int Seed { get; }
	private int _leftTime;
	private CancellationTokenSource _tickCts = new CancellationTokenSource();
	private readonly Dictionary<PlayerConnection, int> _score;
	private readonly Dictionary<(int x, int y), int> _map;

	public Room(GameServer gameServer, string roomName, int roomId, PlayerConnection owner) {
		_server = gameServer;
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
		Random random = new Random(Seed);

		// 시드 기반으로 맵 생성
		_map = new Dictionary<(int x, int y), int>();
		for (var y = 0; y < PEACH_COUNT / PEACH_COLUMN; y++) {
			for (var x = 0; x < PEACH_COLUMN; x++) {
				_map[(x, y)] = random.Next(1, 10);
				// Console.Write(_map[(x, y)] + " ");
			}
			// Console.WriteLine();
		}

		Logger.Info($"Room created: {roomName} ({roomId})");
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
		_score.Remove(playerConnection);

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

		// 점수 초기화
		_score.Clear();
		foreach (PlayerConnection playerConnection in Players) {
			_score[playerConnection] = 0;
		}
		BroadcastState();

		// 메인 Update 로직
		Task.Run(async () => {
			while (!_tickCts.Token.IsCancellationRequested) {
				// 남은 시간 초 업데이트
				_leftTime--;
				BroadcastState();

				// 게임 종료 체크
				if (_leftTime <= 0) {
					EndGame();
				}
				await Task.Delay(TimeSpan.FromSeconds(1));
			}
		}, _tickCts.Token);
	}

	public void EndGame() {
		_state = RoomState.Ending;
		_tickCts.Cancel();
		_server.Rooms.Remove(this);
		BroadcastState();

		Logger.Info($"Room removed: {RoomName} ({RoomId})");
	}

	public void HandleDrag(PlayerConnection playerConnection, ClientRequestDragPacket packet) {
		(int x, int y)[] positions = packet.Positions;

		// 좌표값 검증 - 오류 값 방지
		if (positions.Length < 2) return;
		if (positions.Distinct().Count() != positions.Length) return;
		if (positions.Any(pos => pos.x < 0 || pos.x >= PEACH_COLUMN || pos.y < 0 || pos.y >= PEACH_COUNT / PEACH_COLUMN)) return;

		// 좌표값 직사각형 꼴 검증
		var minX = positions.Min(pos => pos.x);
		var maxX = positions.Max(pos => pos.x);
		var minY = positions.Min(pos => pos.y);
		var maxY = positions.Max(pos => pos.y);

		// 최소부터 최대까지 반복했을 때, 목록 안에 없다면 무시
		for (var y = minY; y <= maxY; y++) {
			for (var x = minX; x <= maxX; x++) {
				if (positions.Contains((x, y))) continue;

				Logger.Error($"Invalid drag positions from player:{playerConnection}, packet:{packet}");
				return;
			}
		}

		var sum = positions.Sum(pos => _map[pos]);

		// 선택한 복숭아의 합이 10이라면
		if (sum == 10) {
			// 이미 고른 복숭아는 0으로 리셋(다른 플레이어가 해당 복숭아 못먹음)
			foreach (var (x, y) in positions) {
				_map[(x, y)] = 0;
			}

			// 복숭아 개수만큼 점수 주기
			_score[playerConnection] = _score.GetValueOrDefault(playerConnection, 0) + positions.Length;

			BroadcastPacket(new ServerResponseDragPacket(playerConnection.Id, positions));
			BroadcastState();

			// 모든 복숭아를 다 먹었는지 확인
			if (_score.Values.Sum() == PEACH_COUNT) {
				EndGame();
			}
		}
	}

	public void HandleSelectRange(PlayerConnection playerConnection, ClientSelectRangePacket packet) {
		// 보낸 유저를 제외하고 타 유저에게 Broadcast
		foreach (PlayerConnection connection in Players.Where(x => x.Id != packet.ClientId)) {
			connection.SendPacket(packet);
		}
	}

	#region 유틸 함수
	public RoomInfo GetRoomInfo() {
		return new RoomInfo {
			RoomId = RoomId,
			Name = RoomName,
			Players = Players.Select(player => new PlayerInfo {
				IsOwner = Equals(player, Owner),
				Id = player.Id,
				Nickname = player.Nickname
			}).ToList(),
			MaxPlayers = MAX_PLAYER,
			State = _state,
			Owner = Owner.Id,
			Score = _score.ToDictionary(x => x.Key.Id, x => x.Value),
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
