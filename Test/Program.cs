using System.Net;
using System.Net.Sockets;
using PeachGame.Common.Packets.Client;
using PeachGame.Common.Serialization;
var client = new TcpClient();
client.Connect(IPAddress.Parse("152.67.203.160"), 9999);

NetworkStream stream = client.GetStream();
BinaryWriter writer = new BinaryWriter(stream);
BinaryReader reader = new BinaryReader(stream);

writer.Write(new ClientPingPacket("Test Nickname"));

var read = reader.ReadPacket();
Console.Write(read);
