using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
[assembly: XmlConfigurator(ConfigFile = "log4net.config")]

namespace PeachGame.Server;

internal static class Program {
	private static async Task Main() {
		// 콘솔 입출력 한글 깨짐 수정
		Console.InputEncoding = Encoding.UTF8;
		Console.OutputEncoding = Encoding.UTF8;

		var host = Host.CreateDefaultBuilder()
			.ConfigureServices(services => services.AddHostedService<ShutdownService>())
			.Build();

		await host.RunAsync();
	}
}

internal class ShutdownService : IHostedService {
	private readonly GameServer _server;

	public ShutdownService() {
		// 환경변수 가져오기
		var listenPort = int.Parse(GetEnvironmentVariable("LISTEN_PORT"));
		var isProduction = bool.Parse(Environment.GetEnvironmentVariable("IS_PRODUCTION") ?? "false");
		var certFilePath = Environment.GetEnvironmentVariable("CERT_FILE_PATH") ?? string.Empty;
		var keyFilePath = Environment.GetEnvironmentVariable("KEY_FILE_PATH") ?? string.Empty;

		// 서버 시작
		_server = new GameServer(listenPort, isProduction, certFilePath, keyFilePath);
	}

	public Task StartAsync(CancellationToken cancellationToken) {
		_server.Start();

		// Await until program exit
		while (!cancellationToken.IsCancellationRequested) { }
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		_server.Stop();
		return Task.CompletedTask;
	}

	private static string GetEnvironmentVariable(string key)
		=> Environment.GetEnvironmentVariable(key) ?? throw new Exception($"{key} is not set");
}
