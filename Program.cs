using System;
using System.Numerics;
using System.Diagnostics;

namespace BlocksAI
{
	class Program
	{
		static void Main(string[] args)
		{
			var board = new Board(6);
			board.PrintToConsole();

			//PrintNeighbors(board);

			var state = new PlayState(15, 8);
			var move = new Move(-1, 14, 7, 12);
			var played = Turn.Play(board, state, move);

			board.PrintToConsole();

			Turn.Withdraw(board, state, move);

			board.PrintToConsole();

			//DebugBlock();
			//TestBlocking();

			GameOne();
			GameTwo();
			GameThree();
			GameFour();
			GameFive();
		}
		
		static void ConnectAndPlay()
		{
			// Connect...

			AIClient client = new AIClient(new AIAgent(0, 0));
			client.Initialize();

			while(true)
			{
				Move move = new Move();
				while((move = new Move()).player != 1)
				{

				}

				Move next = client.EvaluateNextMove(1000);
				// send
				client.WaitForThread();
			}
		}



		static void DebugBlock()
		{
			Game game = Game.Create();

			for(int i = 0; i < 3; ++i)
			{
				Console.WriteLine("Opponents of " + i);
				var opp = game.GetOpponents(i);
				foreach(var o in opp)
					Console.WriteLine(o);
			}
			

			game.states[0] = new PlayState(13, 6);
			game.states[1] = new PlayState(17, 5);
			game.states[2] = new PlayState(30, 31);
			game.score[0] = 2;
			game.score[1] = 2;
			game.score[2] = 1;

			foreach(var state in game.states)
			{
				game.board[state.first] = Field.Stone;
				game.board[state.second] = Field.Stone;
			}

			game.board[28] = Field.Blocked;
			game.board[32] = Field.Blocked;
			game.board[11] = Field.Blocked;
			//game.board[13] = Field.Blocked;

			game.PrintToConsole();
			Console.WriteLine(AIAgent.GetBlockingField(ref game, game.GetOpponents(0)));
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
			AIAgent smartAgent = new AIAgent(1, 3);
			AIAgent dumbAgent = new AIAgent(2, 11);
			

			for(int i = 0; i < 22; ++i)
			{
				//Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 0)
					next = randAgent.Next(ref game);
				if(i%3 == 1)
					next = smartAgent.Minimax(ref game);
				if(i%3 == 2)
					next = dumbAgent.Minimax(ref game);

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

			var smartAgent = new AIAgent(0, 8);
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
			

			for(int i = 0; i < 30; ++i)
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

		static void TestBlocking()
		{
			Console.WriteLine("++++ TEST BLOCKING +++");

			var board = new Board(6);

			var game = Game.Create();
			game.Start();
			var opponents = game.GetOpponents(1);

			board.PrintToConsole();
			var blockAt = AIAgent.GetBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);

			board[23] = Field.Blocked;

			board.PrintToConsole();
			blockAt= AIAgent.GetBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);
		
			game.states[opponents[0]] = Turn.Play(board, game.states[opponents[0]], new Move(-1, 14, 8, 16));
			board[blockAt] = Field.Blocked;

			board.PrintToConsole();

			blockAt= AIAgent.GetBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);

			game.states[opponents[0]] = Turn.Play(board, game.states[opponents[0]], new Move(-1, 15, 8, 18));
			board[blockAt] = Field.Blocked;

			board.PrintToConsole();

			blockAt= AIAgent.GetBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);

			board[14] = Field.Blocked;
			board[20] = Field.Blocked;

			board.PrintToConsole();

			blockAt= AIAgent.GetBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);
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
