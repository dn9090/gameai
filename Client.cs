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

	public struct Message4
	{
		public int a;

		public int b;

		public int c;

		public int d;

		public Message4(int a, int b, int c, int d)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}

		public Move ToMove() => new Move(a, b, c, d);

		public ClientSettings ToClientSettings() => new ClientSettings(a, b, c);

		public static Message4 From(Move move) => new Message4(move.player, move.first, move.second, move.block);

		public unsafe static Message4 Read(NetworkStream stream)
		{
			byte* bytes = stackalloc byte[sizeof(Message4)];

			stream.Read(new Span<byte>(bytes, sizeof(Message4)));

			return *((Message4*)bytes);
		}

		public unsafe static void Send(NetworkStream stream, Message4 msg)
		{
			byte* bytes = (byte*)&msg;
			stream.Write(new Span<byte>(bytes, sizeof(Message4)));
		}
	}

	public struct Client
	{
		public TcpClient tcpClient;

		public Thread thread;

		public bool debug;

		public void Connect(string ip, int port, bool debug = false)
		{
			this.thread = new Thread(Run);
			this.tcpClient = new TcpClient(ip, port);
			this.thread.Start();
			this.debug = debug;
		}

		public void Run()
		{
			var stream = tcpClient.GetStream();
			
			var msg = Message4.Read(stream);
			var settings = msg.ToClientSettings();

			var timeout = (settings.timeout * 1000) - (2 * settings.latency);

			var agent = new AIAgent(settings.player, 12, timeout);
			var game = Game.Create();

			game.Start();

			while(true)
			{
				var move = Move.Empty();

				while((move = Message4.Read(stream).ToMove()).player != settings.player)
				{
					game.Play(move);
				}

				move = agent.Minimax(ref game);
				Message4.Send(stream, Message4.From(move));

				if(move.isEmpty)
					break;
				
				game.Play(move);
				
				if(this.debug)
				{
					Console.WriteLine("-----------------------------");
					Console.WriteLine("Evaluated move for " + settings.player + ": " + move);
					game.PrintToConsole();
				}
			}

			stream.Close();
			tcpClient.Close();

			Console.WriteLine("Player " + settings.player + " disconnected");
		}
	}
}