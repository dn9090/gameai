using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace BlocksAI
{
	public static class BlockingHeuristic
	{
		public static int SomeFreeBlock(Board board, PlayState state)
		{
			// Get a free block that is not the neighbor of
			// one of our stones.
			for(int i = 0; i < board.fields.Length; ++i)
			{
				if(board.fields[i] == Field.Free)
				{
					var start = Board.NeighborStartingIndex(i);
					var end = Board.NeighborEndingIndex(i);
					var selfBlock = false;

					for(int j = start; j < end && board.neighbors[j] != -1; ++j)
					{
						if(board.neighbors[i] == state.first || board.neighbors[i] == state.second)
						{
							selfBlock = true;
							break;
						}
					}

					if(!selfBlock)
						return i;
				}
			}

			// Fallback if no non neighboring block was found.
			for(int i = 0; i < board.fields.Length; ++i)
			{
				if(board.fields[i] == Field.Free)
					return i;
			}

			return -1;
		}

		public static int MinMovementSpace(Span<int> freeFirst, Span<int> freeSecond)
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

		public static int KillTwo(Span<int> freeFirst, Span<int> freeSecond)
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

		public static int KillAtDeadEnd(Board board, Span<int> free)
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