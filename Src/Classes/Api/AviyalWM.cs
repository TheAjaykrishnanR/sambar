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
		aviyalClient.CONNECTED += () => AVIYAL_CONNECTED();
		aviyalClient.MESSAGE_RECEIVED += (message) => AVIYAL_MESSAGE_RECEIVED(message);
	}

	public delegate void AviyalReplyReceivedHandler(string message);
	public event AviyalReplyReceivedHandler AVIYAL_MESSAGE_RECEIVED = (message) => { };

	public delegate void AviyalConnectedHandler();
	public event AviyalConnectedHandler AVIYAL_CONNECTED = () => { };

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
			while (true) TryConnect();
		});
	}

	public void TryConnect()
	{
		while (!socket.Connected)
		{
			Logger.Log("trying to connect to aviyal...");
			try
			{
				// cant reuse a disconnected socket
				socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(new IPEndPoint(IPAddress.Loopback, aviyalPort));
			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message);
				Thread.Sleep(1000);
			}
		}
		Logger.Log("connected to aviyal");
		Task.Run(() =>
		{
			Thread.Sleep(100);
			CONNECTED();
		});
		Receive();
	}

	public void Send(string request)
	{
		if (!socket.Connected) return;
		socket.Send(Encoding.UTF8.GetBytes(request));
	}

	public delegate void MessageReceivedHandler(string message);
	public event MessageReceivedHandler MESSAGE_RECEIVED = (message) => { };

	public delegate void ConnectedEventHandler();
	public event ConnectedEventHandler CONNECTED = () => { };

	public void Receive()
	{
		while (socket.Connected)
		{
			try
			{
				byte[] buffer = new byte[1024];
				socket.Receive(buffer);
				string message = Encoding.UTF8.GetString(buffer);
				MESSAGE_RECEIVED(message);
			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message);
				if (!socket.Connected) TryConnect();
			}
		}
	}
}


