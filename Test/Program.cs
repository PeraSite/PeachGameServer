using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Serialization;

async Task Connect() {
	TcpClient client = new TcpClient();
	await client.ConnectAsync(IPAddress.Loopback, 9000);

	NetworkStream stream = client.GetStream();

	var sendPacket = new ClientPingPacket(Guid.NewGuid());
	await using BinaryWriter bw = new BinaryWriter(stream);

	bw.Write(sendPacket);

}

// await Connect();

IEnumerable<Task> tasks = Enumerable.Range(0, 100).Select(x => Connect());
var sw = Stopwatch.StartNew();
await Task.WhenAll(tasks);
sw.Stop();
Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
