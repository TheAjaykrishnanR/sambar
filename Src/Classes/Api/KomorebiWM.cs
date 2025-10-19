using System.IO;
using System.Text;
using System.IO.Pipes;

namespace sambar;

public partial class Api
{
	private void KomorebiInit() { }

	public event AviyalReplyReceivedHandler KOMOREBI_MESSAGE_RECEIVED = (message) => { };
	public event AviyalConnectedHandler KOMOREBI_CONNECTED = () => { };

	public void StartKomorebiClient(string pipeName)
	{
		KomorebiClient komorebiClient = new(pipeName);
		komorebiClient.MESSAGE_RECEIVED += (message) => KOMOREBI_MESSAGE_RECEIVED(message);
	}

	private void KomorebiCleanup() { }
}

class KomorebiClient
{
	NamedPipeServerStream npcs;
	StreamReader sr;
	string pipeName;
	public KomorebiClient(string pipeName)
	{
		this.pipeName = pipeName;
		npcs = new(pipeName);
		sr = new(npcs);
		Task.Run(() => { while (true) TryConnect(); });
	}

	public bool connected = false;
	public void TryConnect()
	{
		connected = npcs.IsConnected;
		while (!connected)
		{
			try
			{
				npcs.Close();
				npcs.Dispose();
				npcs = new(pipeName);
				sr = new(npcs);
				Logger.Log("komorebi closed");
				Logger.Log("komorebi waiting for connection");
				Task.Run(TryMakeSubscribe);
				npcs.WaitForConnection();
				connected = npcs.IsConnected;
			}
			catch (Exception ex)
			{
				connected = false;
				Logger.Log(ex.Message);
				Thread.Sleep(1000);
			}
		}
		Logger.Log("komorebi connected");
		Task.Run(async () =>
		{
			await Task.Delay(50);
			CONNECTED();
		});
		Receive();
	}

	void TryMakeSubscribe()
	{
		while (!connected)
		{
			Logger.Log($"sending subsribe-pipe command to Komorebi");
			Utils.ExecuteShellCommand($"komorebic subscribe-pipe {pipeName}");
			Thread.Sleep(1000);
		}
	}

	public delegate void MessageReceivedHandler(string message);
	public event MessageReceivedHandler MESSAGE_RECEIVED = (message) => { };

	public delegate void ConnectedEventHandler();
	public event ConnectedEventHandler CONNECTED = () => { };

	public void Receive()
	{
		string? message = sr.ReadLine();
		//Logger.Log($"KOMOREBI MESSAGE READ: {message}");
		try
		{
			if (message != null) MESSAGE_RECEIVED(message);
		}
		catch (Exception ex)
		{
			Logger.Log(ex.Message);
		}
	}
}

