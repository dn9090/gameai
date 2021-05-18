using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace BlocksAI
{
	public struct AIAgent
	{
		public int alpha;

		public int beta;

		public int maxDepth;

		public int player;

		public Move nextMove;

		public AIAgent(int player, int alpha, int beta, int maxDepth)
		{
			this.player = player;
			this.alpha = alpha;
			this.beta = beta;
			this.maxDepth = maxDepth;
			this.nextMove = Move.Empty();
		}

		public Move Minimax(ref Game game)
		{
			this.nextMove = Move.Empty();
			Minimax(this.player, ref game, maxDepth);
			return this.nextMove;
		}

		public float CalculateScore(int player, ref Game game, Span<int> freeFirst, Span<int> freeSecond) =>
			(freeFirst.Length + freeSecond.Length) * ((float)game.score[this.player] / ((float)game.scoreSum + 0.001f));

		public float Minimax(int player, ref Game game, int depth)
		{
			Span<int> neighborsBuffer = stackalloc int[6];
			Span<int> freeBuffer = stackalloc int[6];

			var neighborsFirst = game.board.GetNeighbors(game.states[player].first, neighborsBuffer);
			var freeFirst = game.board.GetFields(neighborsFirst, Field.Free, freeBuffer);

			var neighborsSecond = game.board.GetNeighbors(game.states[player].second, neighborsBuffer.Slice(3));
			var freeSecond = game.board.GetFields(neighborsSecond, Field.Free, freeBuffer.Slice(3));

			// Exit if depth is reached or no free fields are available.
			if(depth == 0 || freeFirst.Length + freeSecond.Length == 0)
				return CalculateScore(player, ref game, freeFirst, freeSecond);

			var opponents = game.GetOpponents(player);

			float maxScore = float.MinValue; 

			for(int i = 0; i < freeFirst.Length; i++)
			{
				for(int j = 0; j < freeSecond.Length; j++)
				{
					if(freeFirst[i] == freeSecond[j])
						continue;

					var state = new PlayState(freeFirst[i], freeSecond[j]);
					var score = Minimax(player, ref game, opponents, state, depth, out Move move);

					if(score > maxScore)
					{
						maxScore = score;
						if(depth == maxDepth)
							this.nextMove = move;
					}
				}
			}

			for(int i = 0; i < freeFirst.Length; i++)
			{
				var state = new PlayState(freeFirst[i], game.states[player].second);
				var score = Minimax(player, ref game, opponents, state, depth, out Move move);

				if(score > maxScore)
				{
					maxScore = score;
					if(depth == maxDepth)
						this.nextMove = move;
				}
			}

			for(int i = 0; i < freeSecond.Length; i++)
			{
				var state = new PlayState(game.states[player].first, freeSecond[i]);
				var score = Minimax(player, ref game, opponents, state, depth, out Move move);
					
				if(score > maxScore)
				{
					maxScore = score;
					if(depth == maxDepth)
						this.nextMove = move;
				}
			}

			return maxScore;
		}

		public float Minimax(int player, ref Game game, Span<int> opponents, PlayState state, int depth, out Move move)
		{
			Turn.Play(game.board, game.states[player], state);
			var block = FindBlockingField(ref game, opponents);
			Turn.Withdraw(game.board, game.states[player], state);

			move = new Move(state, block);

			var nextPlayer = game.NextPlayer(player);
			
			var oldState = game.Play(player, move);
			var score = Minimax(nextPlayer, ref game, depth - 1);
			game.Withdraw(player, oldState, move);

			return this.player == player ? score : -score;
		}

		public static int FindBlockingField(ref Game game, Span<int> opponents)
		{
			Span<int> neighbors = stackalloc int[opponents.Length * 3];
			Span<int> free = stackalloc int[opponents.Length * 3];

			// 1. heuristic: Killing move when only one field is left.
			// 2. heuristic: Killing move in two, when the opponent stones block themself.
			// 3. heuristic: Defense blocking.

			var highestScore = int.MinValue;
			var scoreBlock = -1;

			for(int i = opponents.Length - 1; i >= 0; --i)
			{
				var opponent = game.states[opponents[i]];
				var score = game.score[opponents[i]];

				var nFirst = game.board.GetNeighbors(opponent.first, neighbors);
				var fFirst = game.board.GetFields(nFirst, Field.Free, free);

				var nSecond = game.board.GetNeighbors(opponent.second, neighbors.Slice(3));
				var fSecond = game.board.GetFields(nSecond, Field.Free, free.Slice(3));

				if(fFirst.Length == 0 && fSecond.Length == 0)
					continue;
				
				var killTwo = KillTwoHeuristic(fFirst, fSecond);

				if(killTwo != -1) { //Console.WriteLine("[Heuristic] Kill Two!");
					return killTwo; }

				var killBorder = KillAtDeadEndHeuristic(game.board, fFirst);

				if(killBorder != -1) { //Console.WriteLine("[Heuristic] Kill Dead End (A)!");
					return killBorder; }

				killBorder = KillAtDeadEndHeuristic(game.board, fSecond);

				if(killBorder != -1) { //Console.WriteLine("[Heuristic] Kill Dead End (B)!");
					return killBorder; }

				if(score > highestScore)
				{
					highestScore = score;
					scoreBlock = MinMovementSpaceHeuristic(fFirst, fSecond);
				}
			}

			return scoreBlock == -1 ? SomeFreeBlock(game.board) : scoreBlock;
		}

		public static int SomeFreeBlock(Board board)
		{
			for(int i = 0; i < board.fields.Length; ++i)
				if(board.fields[i] == Field.Free)
					return i;
			return -1;
		}

		public static int MinMovementSpaceHeuristic(Span<int> freeFirst, Span<int> freeSecond)
		{
			if(freeFirst.Length == 0)
				return freeSecond[0];
			
			if(freeSecond.Length == 0)
				return freeFirst[0];

			for(int i = 0; i < freeFirst.Length; ++i)
			for(int j = 0; j < freeSecond.Length; ++j)
			{
				if(freeFirst[i] != freeSecond[i])
					return freeFirst[i];
			}

			return freeFirst[0];
		}

		public static int KillTwoHeuristic(Span<int> freeFirst, Span<int> freeSecond)
		{
			// Gloal is to force the opponents to block themself because
			// it is required to move at least one stone. If a field is
			// shared between two stones and it is the only free neighbor for one
			// stone, block the other stone so that one stone is forced to move
			// there and block the other:
			//  / \ /X\ /  / \ /X\ /  / \ /X\ /  / \ /X\ /
			//  \ /!\O/    \?/O\ /    \X/ \O/    \X/O\X/  
			//  /?\O/      /X\O/      /X\O/      /X\O/    
			//  \ /        \ /        \ /        \ /      
			//     -1-        -2-        -3-        -4-
			//  ! = shared between both stones
			//  ? = cut off one stone

			if(freeFirst.Length == 1 && freeSecond.Length == 2)
			{
				var temp = freeFirst;
				freeFirst = freeSecond;
				freeSecond = temp;
			}

			if(freeFirst.Length != 2 || freeSecond.Length != 1)
				return -1;
			
			if(freeFirst[0] == freeSecond[0])
				return freeFirst[1];

			return freeFirst[0];
		}

		public static int KillAtDeadEndHeuristic(Board board, Span<int> free)
		{
			// Goal is to trap a stone at an dead end.
			// The heuristic checks if one of two free fields is located
			// in a dead end and blocks the other one if thats the case.

			if(free.Length == 1)
				return free[0];

			if(free.Length != 2)
				return -1;
			
			Span<int> neighbors = stackalloc int[3];
			var n = board.GetNeighbors(free[0], neighbors);

			// @Todo: We need to check if the free field has neighbor
			// fields with stones on it, because then the kill mechanic needs to kick in.

			if(board.GetFreeFieldCount(n) == 1)
				return free[1];

			n = board.GetNeighbors(free[1], neighbors);
			
			if(board.GetFreeFieldCount(n) == 1)
				return free[0];

			return -1;
		}
	}
}