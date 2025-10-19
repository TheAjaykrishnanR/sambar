/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace sambar;

public partial class Api
{
	// dont initialize in fields anything that would block/hang the api instance
	// initialization, GlazeClient waits on reply, so init in separate thread or async 
	// function
	GlazeClient client;
	private async void GlazeInit()
	{ //if (Process.GetProcessesByName("glazewm").Length < 1) return; 
	}

	public async Task StartGlazeClient(string glazeUri)
	{
		// The order is important, when sending subscription notification to glaze, the event 
		// handler must already be attached inorder to capture the response. We are processing 
		// this response  in GlazeEventHandler but we need all the active glaze workspaces 
		// to do so, therfore before sending the subscription run GetAllWorkspaces()

		client = new(glazeUri);
		client.REPLY_RECIEVED += GlazeEventHandler;
		client.CONNECTED += async () =>
		{
			await GetAllWorkspaces();
			GLAZE_CONNECTED(workspaces.Count);
			await SubscribeToGlazeWMEvents();

			Logger.Log($"GlazeInit() => Workspaces: {workspaces.Count}");
		};

	}

	public delegate void GlazeWorkspaceChangedHandler(int index);
	public event GlazeWorkspaceChangedHandler GLAZE_WORKSPACE_CHANGED = (index) => { };

	public delegate void GlazeConnectedEventHandler(int workspaceCount);
	public event GlazeConnectedEventHandler GLAZE_CONNECTED = (workspaceCount) => { };

	public Workspace currentWorkspace = new();
	public List<Workspace> workspaces = new();

	private async Task GetAllWorkspaces()
	{
		string message = "query workspaces";
		Logger.Log("querying all workspaces");
		string reply = await client.SendCommand(message);
		Logger.Log($"SendCommand: {reply}");
		Message msg = JsonConvert.DeserializeObject<Message>(reply);
		if (msg.clientMessage == message)
		{
			int i = 0;
			this.workspaces = new();
			foreach (Container workspace in msg.data.workspaces)
			{
				Workspace wksp = new();
				wksp.index = i;
				wksp.id = workspace.id;
				wksp.name = workspace.name;
				workspaces.Add(wksp);
				if (workspace.hasFocus == true)
				{
					currentWorkspace = wksp;
				}
				i++;
			}
		}
	}

	private void GlazeEventHandler(string message)
	{
		Logger.Log("glaze_event: " + message);
		Message msg = JsonConvert.DeserializeObject<Message>(message);
		switch (msg.messageType)
		{
			case "event_subscription":
				string focusedWorkspaceId = null;
				if (msg.data.focusedContainer.type == "window")
				{
					focusedWorkspaceId = msg.data.focusedContainer.parentId;
				}
				else if (msg.data.focusedContainer.type == "workspace")
				{
					focusedWorkspaceId = msg.data.focusedContainer.id;
				}
				currentWorkspace = workspaces.Where(wksp => wksp.id == focusedWorkspaceId).First();
				GLAZE_WORKSPACE_CHANGED(currentWorkspace.index);
				Logger.Log($"GLAZE_WORKSPACE_CHANGED: {currentWorkspace.index}");
				break;
		}
	}

	string? glazeSubscriptionId;
	private async Task SubscribeToGlazeWMEvents()
	{
		string command = $"sub --events focus_changed";
		string reply = await client.SendCommand(command);
		Logger.Log($"subscribe reply: {reply}");
		try
		{
			Message? replyMessage = JsonConvert.DeserializeObject<Message>(reply);
			glazeSubscriptionId = replyMessage?.data.subscriptionId;
			Logger.Log($"subscriptionId: {glazeSubscriptionId}");
		}
		catch (Exception ex)
		{
			Logger.Log($"[ JSON ERROR ]: {ex.Message}");
		}
	}

	internal async Task UnsubToGlazeWMEvents()
	{
		string command = $"unsub --id {glazeSubscriptionId}";
		string reply = await client.SendCommand(command);
		Logger.Log($"unsub reply: {reply}");
	}

	public async Task ChangeWorkspace(int index)
	{
		if (index < 0 || index > workspaces.Count - 1) return;
		string message = $"command focus --workspace {workspaces[index].name}";
		await client.SendCommand(message);
	}

	private async void GlazeCleanup()
	{
		await UnsubToGlazeWMEvents();
	}
}

public enum GlazeCommandType
{
	QUERY, COMMAND, SUB
}

public class GlazeClient
{
	ClientWebSocket client = new();
	CancellationTokenSource cts = new();
	Uri glazeUri;
	WebSocketReceiveResult result;

	string lastReply = "";

	public delegate void ReplyRecievedHandler(string reply);
	public event ReplyRecievedHandler REPLY_RECIEVED = (msg) => { };

	public delegate void ConnectedEventHandler();
	public event ConnectedEventHandler CONNECTED = () => { };

	public GlazeClient(string glazeUri)
	{
		this.glazeUri = new(glazeUri);
		Task.Run(async () => { while (true) await TryConnect(); });
	}

	public bool connected = false;
	int i = 0;
	public async Task TryConnect()
	{
		connected = client.State == WebSocketState.Open;
		while (!connected)
		{
			try
			{
				Logger.Log("trying to connect to glaze...");
				client = new();
				cts = new();
				await client.ConnectAsync(glazeUri, cts.Token);
				connected = client.State == WebSocketState.Open;
			}
			catch (Exception ex)
			{
				connected = false;
				Logger.Log(ex.Message);
				Thread.Sleep(1000);
			}
		}
		connected = true;
		Task.Run(async () =>
		{
			await Task.Delay(50);
			CONNECTED();
		});
		Logger.Log($"{i++}. client connected to glaze, client.state: {client.State}");
		await Receive();
	}

	async Task Receive()
	{
		if (!connected) return;
		byte[] buffer = new byte[4096 * 4];
		try
		{
			while ((result = await client.ReceiveAsync(buffer, cts.Token)).Count > 0)
			{
				lastReply += Encoding.UTF8.GetString(buffer, 0, result.Count);
				Array.Clear(buffer);
				if (result.EndOfMessage)
				{
					if (commandMode)
					{
						commandReplyRecieved = true;
					}
					else
					{
						REPLY_RECIEVED(lastReply);
						lastReply = "";
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log(ex.Message);
		}
	}

	bool commandMode = false;
	bool commandReplyRecieved = false;
	public async Task<string> SendCommand(string command)
	{
		if (!connected) return null;
		commandMode = true;
		commandReplyRecieved = false;
		await client.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true, cts.Token);
		while (!commandReplyRecieved)
		{
			Logger.Log("reached");
			await Task.Delay(500);
		}
		commandMode = false;
		string reply = lastReply;
		lastReply = "";
		return reply;
	}
}

public class Workspace
{
	public int index;
	public string id;
	public string name;
}
