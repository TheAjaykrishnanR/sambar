using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace sambar;

public partial class Api
{
	AviyalClient? aviyalClient = null;
	private void AviyalInit()
	{
		//if (Process.GetProcessesByName("aviyal").Length < 1) return;
		aviyalClient = new();
		Task.Run(() =>
		{
			while (true)
			{
				string recieved = aviyalClient.Receive();
				if (recieved != null)
				{
					AVIYAL_MESSAGE_RECEIVED(recieved);
					Logger.Log($"AVIYAL RESPONSE: {recieved}");
				}
			}
		});
	}

	public delegate void AviyalReplyReceivedHandler(string message);
	public event AviyalReplyReceivedHandler AVIYAL_MESSAGE_RECEIVED = (message) => { };

	public void AviyalSend(string request) => aviyalClient?.Send(request);

	private void AviyalCleanup()
	{

	}
}

class AviyalClient
{
	Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	int aviyalPort = 6969;

	public AviyalClient()
	{
		Task.Run(() =>
		{
			while (!socket.Connected)
			{
				socket.Connect(new IPEndPoint(IPAddress.Loopback, aviyalPort));
				Thread.Sleep(100);
			}
			Logger.Log("AVIYAL CLIENT CONNECTED");
		});
	}

	public void Send(string request)
	{
		socket.Send(Encoding.UTF8.GetBytes(request));
	}

	public string Receive()
	{
		if (!socket.Connected) return null;
		byte[] buffer = new byte[1024];
		Logger.Log("AVIYALCLIENT");
		socket.Receive(buffer);
		string response = Encoding.UTF8.GetString(buffer);
		return response;
	}
}


