using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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

	public struct AIClient
	{
		public Game game;

		public AIAgent agent;

		public Thread thread;

		public AIClient(AIAgent agent)
		{
			this.game = Game.Create();
			this.agent = agent;
			this.thread = null;
		}

		public void Initialize()
		{
			this.game.Start();
			this.thread = new Thread(Run);
		}


		public Move EvaluateNextMove(int timeout)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();
			this.thread.Start();

			while(stopwatch.ElapsedMilliseconds < timeout)
				Thread.Sleep(1);

			if(this.agent.moves.TryPop(out Move move))
				return move;
			
			return Move.Empty();
		}

		public void WaitForThread()
		{
			if(this.thread.IsAlive)
				this.thread.Join();
		}

		private void Run()
		{
			this.agent.MinimaxToStack(ref this.game);
		}
	}
}