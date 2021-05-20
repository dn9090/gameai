using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace BlocksAI
{
	public struct ClientSettings
	{
		public int player;

		public int timeout;

		public int latency;

		public ClientSettings(int player, int timeout, int latency)
		{
			this.player = player;
			this.timeout = timeout;
			this.latency = latency;
		}
	}

	public struct Client
	{
		public TcpClient tcpClient;

		public Thread thread;

		public void Connect(string ip, int port)
		{
			this.thread = new Thread(Run);
			this.tcpClient = new TcpClient(ip, port);
			this.thread.Start();
		}

		public void Run()
		{
			var stream = tcpClient.GetStream();
			
			var msg = Message4.Read(stream);
			var settings = msg.ToClientSettings();

			var timeout = (settings.timeout * 1000) - (2 * settings.latency);


		}
	}
}