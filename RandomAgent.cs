using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace BlocksAI
{
	public struct RandomAgent
	{
		public int player;

		public Random random;

		public RandomAgent(int player, int seed)
		{
			this.player = player;
			this.random = new Random(seed);
		}

		public Move Next(ref Game game)
		{
			Span<int> neighborsBuffer = stackalloc int[6];
			Span<int> freeBuffer = stackalloc int[6];

			var myPos = game.states[this.player];

			var neighborsFirst = game.board.GetNeighbors(myPos.first, neighborsBuffer);
			var freeFirst = game.board.GetFields(neighborsFirst, Field.Free, freeBuffer);

			var neighborsSecond = game.board.GetNeighbors(myPos.second, neighborsBuffer.Slice(3));
			var freeSecond = game.board.GetFields(neighborsSecond, Field.Free, freeBuffer.Slice(3));

			if(freeFirst.Length == 0 && freeSecond.Length == 0)
				return Move.Empty();

			var f = this.random.Next(freeFirst.Length);
			var s = this.random.Next(freeSecond.Length);

			var state = new PlayState(freeFirst.Length > 0 ? freeFirst[f] : myPos.first, freeSecond.Length > 0 ? freeSecond[s] : myPos.second);
			var block = -1;

			Turn.Play(game.board, game.states[this.player], state);

			var timeout = 20;
			do
			{
				block = this.random.Next(game.board.fieldCount);
				--timeout;
			} while(game.board[block] != Field.Free && timeout > 0);

			if(timeout == 0)
			{
				block = -1;

				for(int i = 0; i < game.board.fieldCount; ++i)
				{
					if(game.board[i] == Field.Free)
					{
						block = i;
						break;
					}
				}
			}

			Turn.Withdraw(game.board, game.states[this.player], state);

			return new Move(state, block);
		}
	}
}