using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

async Task Connect(int id) {
	TcpClient client = new TcpClient();
	await client.ConnectAsync(IPAddress.Loopback, 9000);

	NetworkStream stream = client.GetStream();

	byte[] buffer = new byte[1024];
	buffer = Encoding.UTF8.GetBytes($"Hello World from Client #{id}");

	await stream.WriteAsync(buffer);
	await stream.FlushAsync();

	int readBytes = await stream.ReadAsync(buffer);
	string message = Encoding.UTF8.GetString(buffer, 0, readBytes);
	// Console.WriteLine($"Client #{id} received: {message}");
}

IEnumerable<Task> tasks = Enumerable.Range(0, 3000).Select(Connect);
var sw = Stopwatch.StartNew();
await Task.WhenAll(tasks);
sw.Stop();
Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
