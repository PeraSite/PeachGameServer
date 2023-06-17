using System;
using System.Text;
using log4net.Config;
[assembly: XmlConfigurator(ConfigFile = "log4net.config")]

namespace PeachGame.Server;

internal static class Program {
	private static void Main() {
		// 콘솔 입출력 한글 깨짐 수정
		Console.InputEncoding = Encoding.Unicode;
		Console.OutputEncoding = Encoding.Unicode;

		// 환경변수 가져오기
		var listenPort = int.Parse(GetEnvironmentVariable("LISTEN_PORT"));

		// 서버 시작
		using GameServer server = new GameServer(listenPort);
		server.Start();
	}

	private static string GetEnvironmentVariable(string key)
		=> Environment.GetEnvironmentVariable(key) ?? throw new Exception($"{key} is not set");
}
