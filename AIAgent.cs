using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace BlocksAI
{
	public struct AIAgent
	{
		public float gamma;

		public float omega;

		public int alpha;

		public int beta;

		public int maxDepth;

		public int player;

		public ConcurrentStack<Move> moves;

		public AIAgent(int player, int alpha, int beta, int maxDepth)
		{
			this.player = player;
			this.alpha = alpha;
			this.beta = beta;
			this.maxDepth = maxDepth;
			this.moves = new ConcurrentStack<Move>();
			this.gamma = 0.33f;
			this.omega = 0.5f;
		}

		public void MinimaxToStack(ref Game game)
		{
			this.moves.Clear();
			Minimax(this.player, ref game, maxDepth);
		}

		public Move Minimax(ref Game game)
		{
			this.moves.Clear();

			Minimax(this.player, ref game, maxDepth);

			if(this.moves.TryPop(out Move move))
				return move;

			return Move.Empty();
		}

		public float CalculateScore(ref Game game) =>
			this.gamma * game.GetPlacementInverse(this.player) + ((float)game.score[this.player] / ((float)game.scoreSum + 0.001f));
			 

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
				return this.player == player ? CalculateScore(ref game) : -CalculateScore(ref game);

			var opponents = game.GetOpponents(player);

			float maxScore = float.MinValue; 

			
			for(int i = 0; i < freeFirst.Length; ++i)
			for(int j = 0; j < freeSecond.Length; ++j)
			{
				if(freeFirst[i] == freeSecond[j])
					continue;

				var state = new PlayState(freeFirst[i], freeSecond[j]);
				var score = Minimax(player, ref game, opponents, state, depth, out Move move);

				if(score > maxScore)
				{
					maxScore = score;
					if(depth == maxDepth)
						this.moves.Push(move);
				}
			}
			

			// Second loop: Move only the first stone.
			for(int i = 0; i < freeFirst.Length; ++i)
			{
				var state = new PlayState(freeFirst[i], game.states[player].second);
				var score = Minimax(player, ref game, opponents, state, depth, out Move move);

				if(score > maxScore)
				{
					maxScore = score;
					if(depth == maxDepth)
						this.moves.Push(move);
				}
			}

			// Third loop: Move only the second stone.
			for(int i = 0; i < freeSecond.Length; ++i)
			{
				var state = new PlayState(game.states[player].first, freeSecond[i]);
				var score = Minimax(player, ref game, opponents, state, depth, out Move move);
					
				if(score > maxScore)
				{
					maxScore = score;
					if(depth == maxDepth)
						this.moves.Push(move);
				}
			}

			if(depth == maxDepth)
				Console.WriteLine("### MAX: " + maxScore);

			return maxScore;
		}

		public float Minimax(int player, ref Game game, Span<int> opponents, PlayState state, int depth, out Move move)
		{
			Turn.Play(game.board, game.states[player], state);
			var block = FindBlockingField(ref game, opponents);
			Turn.Withdraw(game.board, game.states[player], state);

			move = new Move(player, state, block);
			
			var nextPlayer = game.NextPlayer(player);
			var oldState = game.Play(move);
			var score = Minimax(nextPlayer, ref game, depth - 1);
			game.Withdraw(oldState, move);

			return this.player == player || nextPlayer == this.player ? -score : score;
		}

		public static int FindBlockingField(ref Game game, Span<int> opponents)
		{
			Span<int> neighbors = stackalloc int[6];
			Span<int> free = stackalloc int[6];

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

				if(killTwo != -1) { //Console.WriteLine("[Heuristic] Kill Two: " + i);
					return killTwo; }

				var killBorder = KillAtDeadEndHeuristic(game.board, fFirst);

				if(killBorder != -1) { //Console.WriteLine("[Heuristic] Kill Dead End (A): " + i);
					return killBorder; }

				killBorder = KillAtDeadEndHeuristic(game.board, fSecond);

				if(killBorder != -1) { //Console.WriteLine("[Heuristic] Kill Dead End (B): " + i);
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
			// Fallback if no heuristic matches.
			for(int i = 0; i < board.fields.Length; ++i)
				if(board.fields[i] == Field.Free)
					return i;
			return -1;
		}

		public static int MinMovementSpaceHeuristic(Span<int> freeFirst, Span<int> freeSecond)
		{
			// This heuristic tries to minimize the available movement space
			// for the stone with the lowest number of free fields.

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

			if(board.GetFreeFieldCount(n) <= 1)
				return free[1];

			n = board.GetNeighbors(free[1], neighbors);
			
			if(board.GetFreeFieldCount(n) <= 1)
				return free[0];

			return -1;
		}
	}
}