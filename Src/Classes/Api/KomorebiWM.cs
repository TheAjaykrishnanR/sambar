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

	public void TryConnect()
	{
		while (!npcs.IsConnected)
		{
			try
			{
				npcs.Close();
				npcs.Dispose();
				npcs = new(pipeName);
				sr = new(npcs);
				Logger.Log("komorebi closed");
				Logger.Log("komorebi waiting for connection");
				npcs.WaitForConnection();
			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message);
				Thread.Sleep(1000);
			}
		}
		Logger.Log("komorebi connected");
		Receive();
	}

	public delegate void MessageReceivedHandler(string message);
	public event MessageReceivedHandler MESSAGE_RECEIVED = (message) => { };

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

