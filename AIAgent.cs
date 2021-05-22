using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace BlocksAI
{
	public readonly struct Hyperparameters
	{
		public readonly float gamma;

		public readonly float omega;

		public Hyperparameters(float gamma, float omega)
		{
			this.gamma = gamma;
			this.omega = omega;
		}
	}

	public struct AIAgent
	{
		public Hyperparameters hyperparams;

		public int maxDepth;

		public int player;

		public int timeout;

		public Stopwatch stopwatch;

		public ConcurrentStack<Move> moves;

		public AIAgent(int player, int maxDepth, int timeout)
		{
			this.player = player;
			this.maxDepth = maxDepth;
			this.stopwatch = new Stopwatch();
			this.moves = new ConcurrentStack<Move>();
			this.hyperparams = new Hyperparameters(10f, 0.3f);
			this.timeout = timeout;
		}

		public AIAgent(int maxDepth, Hyperparameters hyperparams) : this(-1, maxDepth, -1)
		{
			this.hyperparams = hyperparams;
		}

		public AIAgent(int player, int maxDepth) : this(player, maxDepth, -1)
		{
		}

		public Move Minimax(ref Game game)
		{
			this.moves.Clear();
			this.stopwatch.Restart();

			Max(ref game, maxDepth, float.MinValue, float.MaxValue);

			if(this.moves.TryPop(out Move move))
				return move;

			return Move.Empty();
		}

		public float CalculateScore(ref Game game) =>
			CalculateScore(ref game, game.board.GetFreeNeighborCount(game.states[this.player]));

		public float CalculateScore(ref Game game, int free) =>
			this.hyperparams.gamma * (free / 6) +
			this.hyperparams.omega * ((float)game.score[this.player] / ((float)game.scoreSum + 0.001f));
 
		public float Max(ref Game game, int depth, float alpha, float beta)
		{
			var currentState = game.states[this.player];
			var freeCount = game.board.GetFreeNeighborCount(currentState);

			// Exit if depth is reached or no free fields are available.
			if(depth == 0 || freeCount == 0)
				return CalculateScore(ref game, freeCount);

			var opponents = game.GetOpponents(this.player);
			var nextPlayer = game.NextPlayer(this.player);
			var maxScore = float.MinValue; 

			foreach(var state in GetPlayStates(game.board, currentState))
			{
				if(this.stopwatch.ElapsedMilliseconds > (uint)this.timeout)
					return maxScore;

				Turn.Play(game.board, game.states[player], state);
				var block = GetBlockingField(ref game, opponents);
				Turn.Withdraw(game.board, game.states[player], state);

				var move = new Move(player, state, block);
				var oldState = game.Play(move);
				var score = Min(nextPlayer, ref game, depth - 1, -beta, -alpha);
				game.Withdraw(oldState, move);

				if(score > maxScore)
				{
					maxScore = score;
					if(depth == maxDepth)
						this.moves.Push(move);
				}

				alpha = Math.Max(alpha, score);

				if(alpha > beta)
					break;
			}

			return maxScore;
		}

		public float Min(int player, ref Game game, int depth, float alpha, float beta)
		{
			if(depth == 0)
				return CalculateScore(ref game);
		
			var currentState = game.states[player];
			var opponents = game.GetOpponents(this.player);
			var nextPlayer = game.NextPlayer(player);
			var minScore = float.MaxValue;
		
			foreach(var state in GetPlayStates(game.board, currentState))
			{
				if(this.stopwatch.ElapsedMilliseconds > (uint)this.timeout)
					return minScore;

				Turn.Play(game.board, game.states[player], state);
				var block = GetBlockingField(ref game, opponents);
				Turn.Withdraw(game.board, game.states[player], state);

				var move = new Move(player, state, block);
				var oldState = game.Play(move);
				var score = nextPlayer == this.player ? Max(ref game, depth - 1, -beta, -alpha) : Min(nextPlayer, ref game, depth - 1, alpha, beta);
				game.Withdraw(oldState, move);

				if(score < minScore)
					minScore = score;

				alpha = Math.Max(alpha, score);

				if(alpha >= beta)
					break;
			}

			// No moves found... player is skipped.
			if(minScore == float.MaxValue)
				return nextPlayer == this.player ? Max(ref game, depth - 1, -beta, -alpha) : Min(nextPlayer, ref game, depth - 1, alpha, beta);
			
			return minScore;
		}

		public static IEnumerable<PlayState> GetPlayStates(Board board, PlayState current)
		{
			var startFirst = Board.NeighborStartingIndex(current.first);
			var endFirst = Board.NeighborEndingIndex(current.first);

			var startSecond = Board.NeighborStartingIndex(current.second);
			var endSecond = Board.NeighborEndingIndex(current.second);

			var buffer = new int[6];
			int freeFirst = 0, freeSecond = 0;

			for(int i = startFirst; i < endFirst && board.neighbors[i] != -1; ++i)
			{
				var neighbor = board.neighbors[i];

				if(board.fields[neighbor] == Field.Free)
					buffer[freeFirst++] = neighbor;
			}

			for(int i = startSecond; i < endSecond && board.neighbors[i] != -1; ++i)
			{
				var neighbor = board.neighbors[i];
				
				if(board.fields[neighbor] == Field.Free)
					buffer[3 + freeSecond++] = neighbor;
			}
				
			for(int i = 0; i < freeFirst; ++i)
			for(int j = 3; j < 3 + freeSecond; ++j)
			{
				if(buffer[i] == buffer[j])
					continue;

				yield return new PlayState(buffer[i], buffer[j]);
			}

			for(int i = 0; i < freeFirst; ++i)
				yield return new PlayState(buffer[i], current.second);

			for(int i = 3; i < 3 + freeSecond; ++i)
				yield return new PlayState(current.first, buffer[i]);
		}

		public static int GetBlockingField(ref Game game, Span<int> opponents)
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
				
				var killTwo = BlockingHeuristic.KillTwo(fFirst, fSecond);

				if(killTwo != -1) { //Console.WriteLine("[Heuristic] Kill Two: " + i);
					return killTwo; }

				var killBorder = BlockingHeuristic.KillAtDeadEnd(game.board, fFirst);

				if(killBorder != -1) { //Console.WriteLine("[Heuristic] Kill Dead End (A): " + i);
					return killBorder; }

				killBorder = BlockingHeuristic.KillAtDeadEnd(game.board, fSecond);

				if(killBorder != -1) { //Console.WriteLine("[Heuristic] Kill Dead End (B): " + i);
					return killBorder; }

				if(score > highestScore)
				{
					highestScore = score;
					scoreBlock = BlockingHeuristic.MinMovementSpace(fFirst, fSecond);
				}
			}

			return scoreBlock == -1 ? BlockingHeuristic.SomeFreeBlock(game.board) : scoreBlock;
		}
	}
}