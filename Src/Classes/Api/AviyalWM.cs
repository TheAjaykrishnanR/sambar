using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace sambar;

public partial class Api
{
	Client? aviyalClient = null;

	private void AviyalInit() { }

	public void StartAviyalClient(int port)
	{
		aviyalClient = new(port);
		aviyalClient.CONNECTED += () => AVIYAL_CONNECTED();
		aviyalClient.MESSAGE_RECEIVED += (message) => AVIYAL_MESSAGE_RECEIVED(message);
	}

	public delegate void AviyalReplyReceivedHandler(string message);
	public event AviyalReplyReceivedHandler AVIYAL_MESSAGE_RECEIVED = (message) => { };

	public delegate void AviyalConnectedHandler();
	public event AviyalConnectedHandler AVIYAL_CONNECTED = () => { };

	public void AviyalSend(string request) => aviyalClient?.Send(request);

	private void AviyalCleanup() { }
}

class Client
{
	Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	int port;

	public Client(int port)
	{
		this.port = port;

		Task.Run(() =>
		{
			while (true) TryConnect();
		});
	}

	public void TryConnect()
	{
		while (!socket.Connected)
		{
			Logger.Log("trying to connect to wm server...");
			try
			{
				// cant reuse a disconnected socket
				socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(new IPEndPoint(IPAddress.Loopback, port));
			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message);
				Thread.Sleep(1000);
			}
		}
		Logger.Log("connected to wm server");
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
				byte[] buffer = new byte[1024 * 8];
				int received = socket.Receive(buffer);
				string message = Encoding.UTF8.GetString(buffer.Take(received).ToArray());
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


