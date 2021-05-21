using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace BlocksAI
{
	public struct ClientSettings
	{
		public int player;

		public int timeout;

		public int latency;

		public int controlBytes;

		public ClientSettings(int player, int timeout, int latency, int controlBytes)
		{
			this.player = player;
			this.timeout = timeout;
			this.latency = latency;
			this.controlBytes = controlBytes;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
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

		public ClientSettings ToClientSettings() => new ClientSettings(a, b, c, d);

		public static Message4 From(Move move) => new Message4(move.player, move.first, move.second, move.block);

		public static unsafe Message4 Read(NetworkStream stream)
		{
			byte* bytes = stackalloc byte[Marshal.SizeOf<Message4>()];
			Span<byte> buffer = new Span<byte>(bytes, Marshal.SizeOf<Message4>());
			int received = 0;

			while(received < buffer.Length)
				received += stream.Read(buffer.Slice(received));

			return ((Message4*)bytes)[0];
		}

		public unsafe static void Send(NetworkStream stream, Message4 msg)
		{
			Span<byte> buffer = new Span<byte>((byte*)&msg, Marshal.SizeOf<Message4>());
			stream.Write(buffer);
		}
	}

	public class Client
	{
		public AIAgent agent;

		public TcpClient tcpClient;

		public Thread thread;

		public bool debug;

		public Client(AIAgent agent, bool debug = false)
		{
			this.agent = agent;
			this.debug = debug;
		}

		public void Connect(string ip, int port)
		{
			this.thread = new Thread(Run);
			this.tcpClient = new TcpClient(ip, port);
			this.thread.Start();
		}

		public void WaitForDisconnect()
		{
			this.thread.Join();
		}

		public void Run()
		{
			var stream = tcpClient.GetStream();

			var settings = Message4.Read(stream).ToClientSettings();
			var timeout = (settings.timeout * 1000) - (3 * settings.latency);

			Console.WriteLine("Connected as player " + settings.player + " ---"
				+ "\nLatency: " + settings.latency
				+ "\nTimeout in sec: " + settings.timeout
				+ "\nControl bytes: " + settings.controlBytes);

			this.agent.player = settings.player;
			this.agent.timeout = settings.timeout;

			var game = Game.Create();

			game.Start();

			while(true)
			{
				var move = Move.Empty();
				var aligned = Move.Empty();
				var myTurn = false;

				try
				{
					while(true)
					{
						myTurn = false;

						move = Message4.Read(stream).ToMove();
						
						if(move.player == settings.player)
							break;

						aligned = game.AlignFromContract(move);

						if(this.debug)
						{
							Console.WriteLine($"[{settings.player}] Received move for {move.player}: {move} Aligned: {aligned}");
							Console.Out.Flush();
						}
						
						
						game.Play(aligned);
					}

					myTurn = true;

					if(this.debug)
						Console.WriteLine($"[{settings.player}] My turn...");

					var next = this.agent.Minimax(ref game);

					if(next.isEmpty)
					{
						Message4.Send(stream, Message4.From(game.InvalidMove(settings.player)));
						Console.WriteLine($"[{settings.player}] No move found...");
						break;
					}

					aligned = game.AlignToContract(next);
					Message4.Send(stream, Message4.From(aligned));
					game.Play(next);
					
					if(this.debug)
					{
						Console.WriteLine("-----------------------------");
						Console.WriteLine($"[{settings.player}] Evaluated move for {settings.player}: {next} Aligned: {aligned}");
						game.PrintToConsole();
						Console.Out.Flush();
					}
				} catch (Exception e) {
					Console.Error.WriteLine($"[{settings.player}] ###### Exception with move {move} aligned {aligned} my turn: {myTurn}");
					Console.Error.Write(e.StackTrace);
					Console.Error.Flush();
					game.PrintToConsole();
					Console.Out.Flush();
					throw e;
				}
			}

			stream.Close();
			tcpClient.Close();

			Console.WriteLine("Player " + settings.player + " disconnected");
		}
	}
}