﻿using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;

namespace TimeTortoise.Client
{
	public class SignalRClient : ISignalRClient
	{
		private HubConnection _hubConnection;
		private IHubProxy _hubProxy;

		public Stack<DateTime> Messages { get; }

		public SignalRClient()
		{
			Messages = new Stack<DateTime>();
		}

		public DateTime GetNewestMessage()
		{
			if (Messages.Count == 0) return DateTime.MinValue;
			var message = Messages.Pop();
			Messages.Clear();
			return message;
		}

		public void ConnectToServer()
		{
			_hubConnection = new HubConnection("http://127.0.0.1:8080");
			_hubProxy = _hubConnection.CreateHubProxy("MessageHub");	// must match hub name
			_hubProxy.On<DateTime>("ReceiveMessage", ReceiveMessage);
			_hubConnection.Start();
		}

		public void ReceiveMessage(DateTime lastUserActivityTime)
		{
			Messages.Push(lastUserActivityTime);
		}
	}
}
