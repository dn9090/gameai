using System;
using System.Numerics;
using System.Diagnostics;

namespace BlocksAI
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("##########################################################");
			Console.WriteLine(@"__________.__                 __              _____  .___ ");
			Console.WriteLine(@"\______   \  |   ____   ____ |  | __  ______ /  _  \ |   |");
			Console.WriteLine(@" |    |  _/  |  /  _ \_/ ___\|  |/ / /  ___//  /_\  \|   |");
			Console.WriteLine(@" |    |   \  |_(  <_> )  \___|    <  \___ \/    |    \   |");
			Console.WriteLine(@" |______  /____/\____/ \___  >__|_ \/____  >____|__  /___|");
			Console.WriteLine(@"        \/                 \/     \/     \/        \/     ");
			Console.WriteLine("##########################################################");

			//ConnectAndPlay(new AIAgent(10, new Hyperparameters(6f, 1f)), "192.168.0.31", 55555, count: 3);

			/*
			var board = new Board(6);
			board.PrintToConsole();

			//PrintNeighbors(board);

			var state = new PlayState(15, 8);
			var move = new Move(-1, 14, 7, 12);
			var played = Turn.Play(board, state, move);

			board.PrintToConsole();

			Turn.Withdraw(board, state, move);

			board.PrintToConsole();

			*/


			//DebugBlock();
			//TestBlocking();

			GameOne();
			GameTwo();
			GameThree();
			GameFour();
			GameFive();

			//GameFromServer();
		}

		static void ConnectAndPlay(AIAgent agent, string ip, int port, int count = 1)
		{
			Console.WriteLine("Connecting to " + ip + ":" + port + "...");

			var clients = new Client[count];

			for(int i = 0; i < clients.Length; ++i)
				clients[i] = new Client(agent, true);

			Console.WriteLine("Preparing clients...");

			foreach(var client in clients)
				client.Connect(ip, port++);

			foreach(var client in clients)
				client.WaitForDisconnect();
		}

		static void GameFromServer()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 1");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			//game.PrintToConsole();

			AIAgent a = new AIAgent(0, 10, -1);
			AIAgent b = new AIAgent(1, 10, -1);
			AIAgent c = new AIAgent(2, 10, -1);

			for(int i = 0; i < 39; ++i)
			{
				//Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 0)
					next = a.Minimax(ref game);
				if(i%3 == 1)
					next = b.Minimax(ref game);
				if(i%3 == 2)
					next = c.Minimax(ref game);

				//Console.WriteLine(next);
				
				if(!next.isEmpty)
					game.Play(next);
			}

			game.PrintToConsole();
		}


		static void GameOne()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 1");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			//game.PrintToConsole();

			RandomAgent randAgent = new RandomAgent(0, 1000);
			AIAgent dumbAgent = new AIAgent(1, 3);
			AIAgent smartAgent = new AIAgent(2, 11);
			

			for(int i = 0; i < 22; ++i)
			{
				//Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 0)
					next = randAgent.Next(ref game);
				if(i%3 == 1)
					next = dumbAgent.Minimax(ref game);
				if(i%3 == 2)
					next = smartAgent.Minimax(ref game);

				//Console.WriteLine(next);
				
				if(!next.isEmpty)
					game.Play(next);
			}

			game.PrintToConsole();
		}

		static void GameTwo()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 2");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			//game.PrintToConsole();

			AIAgent smartAgent = new AIAgent(0, 9);
			RandomAgent randAgent = new RandomAgent(1, 1000);
			RandomAgent randAgent2 = new RandomAgent(2, 4000);
			

			for(int i = 0; i < 21; ++i)
			{
				//Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 0)
					next = smartAgent.Minimax(ref game);
				if(i%3 == 1)
					next = randAgent.Next(ref game);
				if(i%3 == 2)
					next = randAgent2.Next(ref game);

				//Console.WriteLine(next);
				
				if(!next.isEmpty)
					game.Play(next);
			}

			game.PrintToConsole();
		}

		static void GameThree()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 3");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			//game.PrintToConsole();

			RandomAgent randAgent = new RandomAgent(0, 1000);
			RandomAgent randAgent2 = new RandomAgent(1, 4000);
			AIAgent smartAgent = new AIAgent(2, 9);
		
			for(int i = 0; i < 36; ++i)
			{
				//Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 2)
					next = smartAgent.Minimax(ref game);
				if(i%3 == 0)
					next = randAgent.Next(ref game);
				if(i%3 == 1)
					next = randAgent2.Next(ref game);

				//Console.WriteLine(next);
				
				if(!next.isEmpty)
					game.Play(next);
			}

			game.PrintToConsole();
		}

		static void GameFour()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 4");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			//game.PrintToConsole();

			var smartAgent = new AIAgent(0, 1000, 2000);
			var dumbAgent = new AIAgent(1, 3);
			var randAgent = new RandomAgent(2, 4000);

			for(int i = 0; i < 30; ++i)
			{
				//Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 0)
					next = smartAgent.Minimax(ref game);
				if(i%3 == 1)
					next = dumbAgent.Minimax(ref game);
				if(i%3 == 2)
					next = randAgent.Next(ref game);
				//Console.WriteLine(next);
				
				if(!next.isEmpty)
					game.Play(next);
			}

			game.PrintToConsole();
		}

		static void GameFive()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 5");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			//game.PrintToConsole();

			var dumbAgent = new AIAgent(0, 3);
			var randAgent = new RandomAgent(1, 7000);
			var smartAgent = new AIAgent(2, 15);
			

			for(int i = 0; i < 42; ++i)
			{
				//Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 2)
					next = smartAgent.Minimax(ref game);
				if(i%3 == 0)
					next = dumbAgent.Minimax(ref game);
				if(i%3 == 1)
					next = randAgent.Next(ref game);

				//Console.WriteLine(next);
				
				if(!next.isEmpty)
					game.Play(next);
			}

			game.PrintToConsole();
		}
	}
}


/*




              9 10 11 12 13 14 15
                 4  5  6  7  8
                    1  2  3
                      
 1 + 2 * (y + 1)

y(0) = 1 + 2 * (0 + 1) = 3 
y(1) = 1 + 2 * (1 + 1) = 5
y(2) = 1 + 2 * (2 + 1) = 
..
y(4) 

var fields = Math.Min(1 + 2 * (line + 1), 9)

*/
