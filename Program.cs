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
			var move = new Move(14, 7, 12);
			var played = Turn.Play(board, state, move);

			board.PrintToConsole();

			Turn.Withdraw(board, state, move);

			board.PrintToConsole();


			//DebugBlock();
			//TestBlocking();

			GameOne();
			GameTwo();
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
			

			game.states[0] = new PlayState(8, 3);
			game.states[1] = new PlayState(10, 5);
			game.states[2] = new PlayState(28, 30);
			game.score[0] = 2;
			game.score[1] = 2;
			game.score[2] = 2;

			foreach(var state in game.states)
			{
				game.board[state.first] = Field.Stone;
				game.board[state.second] = Field.Stone;
			}

			game.board[27] = Field.Blocked;
			game.board[17] = Field.Blocked;
			game.board[11] = Field.Blocked;
			//game.board[13] = Field.Blocked;

			game.PrintToConsole();
			Console.WriteLine(AIAgent.FindBlockingField(ref game, game.GetOpponents(0)));
		}

		static void GameOne()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 1");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			game.PrintToConsole();

			RandomAgent randAgent = new RandomAgent(0, 1000);
			AIAgent smartAgent = new AIAgent(1, 0, 0, 8);
			AIAgent dumbAgent = new AIAgent(2, 0, 0, 2);
			

			for(int i = 0; i < 15; ++i)
			{
				Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 0)
					next = randAgent.Next(ref game);
				if(i%3 == 1)
					next = smartAgent.Minimax(ref game);
				if(i%3 == 2)
					next = dumbAgent.Minimax(ref game);

				Console.WriteLine(next);
				
				if(next.isEmpty)
					Console.WriteLine("...skip");
				else
					game.Play(i % 3, next);
				game.PrintToConsole();
			}
		}

		static void GameTwo()
		{
			Console.WriteLine("##################################");
			Console.WriteLine("####### GAME 2");
			Console.WriteLine("##################################");


			Game game = Game.Create();
			game.Start();
			game.PrintToConsole();

			AIAgent smartAgent = new AIAgent(0, 0, 0, 8);
			RandomAgent randAgent = new RandomAgent(1, 1000);
			RandomAgent randAgent2 = new RandomAgent(2, 4000);
			

			for(int i = 0; i < 15; ++i)
			{
				Console.WriteLine("Turn: " + i % 3);

				Move next = Move.Empty();

				if(i%3 == 0)
					next = smartAgent.Minimax(ref game);
				if(i%3 == 1)
					next = randAgent.Next(ref game);
				if(i%3 == 2)
					next = randAgent2.Next(ref game);

				Console.WriteLine(next);
				
				if(next.isEmpty)
					Console.WriteLine("...skip");
				else
					game.Play(i % 3, next);
				game.PrintToConsole();
			}
		}

		static void TestBlocking()
		{
			Console.WriteLine("++++ TEST BLOCKING +++");

			var board = new Board(6);

			var game = Game.Create();
			game.Start();
			var opponents = game.GetOpponents(1);

			board.PrintToConsole();
			var blockAt = AIAgent.FindBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);

			board[23] = Field.Blocked;

			board.PrintToConsole();
			blockAt= AIAgent.FindBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);
		
			game.states[opponents[0]] = Turn.Play(board, game.states[opponents[0]], new Move(14, 8, 16));
			board[blockAt] = Field.Blocked;

			board.PrintToConsole();

			blockAt= AIAgent.FindBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);

			game.states[opponents[0]] = Turn.Play(board, game.states[opponents[0]], new Move(15, 8, 18));
			board[blockAt] = Field.Blocked;

			board.PrintToConsole();

			blockAt= AIAgent.FindBlockingField(ref game, opponents);

			Console.WriteLine("Block at: " + blockAt);

			board[14] = Field.Blocked;
			board[20] = Field.Blocked;

			board.PrintToConsole();

			blockAt= AIAgent.FindBlockingField(ref game, opponents);

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
