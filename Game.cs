using System;
using System.Numerics;

namespace BlocksAI
{
	public struct Game
	{
		public PlayState[] states;

		public int[] score;

		public int scoreSum;

		public int[] opponents;

		public Board board;

		public static Game Create()
		{
			Game game = new Game();

			game.board = new Board(6);
			game.states = new PlayState[3];
			game.score = new int[game.states.Length];
			game.opponents = new int[game.states.Length * (game.states.Length - 1)];

			for(int i = 0, j = 0; i < game.states.Length; ++i)
			{
				int next = i;
				while((next = game.NextPlayer(next)) != i)
					game.opponents[j++] = next;
			}

			return game;
		}

		public void Start()
		{
			this.states[0] = PlayState.Red(this.board);
			this.states[1] = PlayState.Green(this.board);
			this.states[2] = PlayState.Blue(this.board);

			for(int i = 0; i < this.states.Length; ++i)
			{
				this.board[this.states[i].first] = Field.Stone;
				this.board[this.states[i].second] = Field.Stone;
			}
		}

		public PlayState Play(int player, Move move)
		{
			var state = this.states[player];
			this.states[player] = Turn.Play(this.board, state, move);
			this.scoreSum -= this.score[player];
			this.score[player] += state.first != this.states[player].first ? 1 : 0;
			this.score[player] += state.second != this.states[player].second ? 1 : 0;
			this.scoreSum += this.score[player];

			return state;
		}

		public void Withdraw(int player, PlayState from, Move move)
		{
			var state = this.states[player];
			this.states[player] = Turn.Withdraw(this.board, from, move);
			this.scoreSum -= this.score[player];
			this.score[player] -= state.first != this.states[player].first ? 1 : 0;
			this.score[player] -= state.second != this.states[player].second ? 1 : 0;
			this.scoreSum += this.score[player];
		}

		public void PrintToConsole()
		{
			Console.WriteLine("--------------------------------");
			Console.WriteLine("Players: ");
			for(int i = 0; i < this.states.Length; ++i)
				Console.WriteLine($"{i}: ({this.states[i].first},{this.states[i].second}) \t {this.score[i]}");
			Console.WriteLine("Score sum \t=" + this.scoreSum);
			this.board.PrintToConsole();
			Console.WriteLine("--------------------------------");
		}

		public int NextPlayer(int player) => ++player >= this.states.Length ? 0 : player;

		public Span<int> GetOpponents(int player) => new Span<int>(this.opponents, player * 2, this.states.Length - 1);
	}

	public struct Move
	{
		public bool isEmpty => first == -1 || second == -1 || block == -1;

		public int first;

		public int second;

		public int block;

		public Move(int first, int second, int block)
		{
			this.first = first;
			this.second = second;
			this.block = block;
		}

		public Move(PlayState state, int block) : this(state.first, state.second, block)
		{
		}

		public override string ToString() => $"Move<({first},{second}):{block}>";
	
		public static Move Empty() => new Move(-1, -1, -1);
	}

	public struct PlayState
	{
		public int first;

		public int second;

		public PlayState(int first, int second)
		{
			this.first = first;
			this.second = second;
		}

		public static PlayState Red(Board board)
		{
			var y = board.size / 2;
			var x = Board.RowSizeOf(y) - 1;
			return new PlayState(Board.IndexOf(x, y), Board.IndexOf(x - 2, y - 1));
		}

		public static PlayState Green(Board board)
		{
			var y = board.size / 2;
			var x = Board.RowSizeOf(y) - 1;
			return new PlayState(Board.IndexOf(0, y), Board.IndexOf(0, y - 1));
		}

		public static PlayState Blue(Board board)
		{
			var y = board.size - 1;
			var x = Board.RowSizeOf(y) / 2;
			return new PlayState(Board.IndexOf(x - 1, y), Board.IndexOf(x + 1, y));
		}
	}

	public static class Turn
	{
		public static PlayState Play(Board board, PlayState from, PlayState to)
		{
			board[from.first] = Field.Free;
			board[from.second] = Field.Free;
			board[to.first] = Field.Stone;
			board[to.second] = Field.Stone;

			return to;
		}
	
		public static PlayState Play(Board board, PlayState state, Move move)
		{
			var played = Play(board, state, new PlayState(move.first, move.second));
			board[move.block] = Field.Blocked;
			return played;
		}

		public static PlayState Withdraw(Board board, PlayState from, PlayState to)
		{
			board[to.first] = Field.Free;
			board[to.second] = Field.Free;
			board[from.first] = Field.Stone;
			board[from.second] = Field.Stone;

			return from;
		}

		public static PlayState Withdraw(Board board, PlayState state, Move move)
		{
			board[move.block] = Field.Free;
			return Withdraw(board, state, new PlayState(move.first, move.second));
		}
	}
}