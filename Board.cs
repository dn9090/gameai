using System;
using System.Numerics;

namespace BlocksAI
{
	public struct Position
	{
		public int x;
		
		public int y;

		public Position(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public enum Field : byte
	{
		Free,
		Blocked,
		Stone,
	}

	public struct Board
	{
		public Field this[Position position]
		{
			get => this.fields[IndexOf(position.x, position.y)];
			set => this.fields[IndexOf(position.x, position.y)] = value;
		}

		public Field this[int x, int y]
		{
			get => this.fields[IndexOf(x, y)];
			set => this.fields[IndexOf(x, y)] = value;
		}

		public Field this[int index]
		{
			get => this.fields[index];
			set => this.fields[index] = value;
		}

		public int fieldCount => this.fields.Length;

		public Field[] fields;

		public Position[] coordinates;

		public int size;

		public Board(int size)
		{
			this.size = size;
			this.fields = new Field[NumberOfFields(this.size)];
			this.coordinates = new Position[this.fields.Length];

			// Initialize blocked fields.
			this.fields[0] = Field.Blocked;
			this.fields[this.fields.Length - 1] = Field.Blocked;
			this.fields[IndexOf(0, size - 1)] = Field.Blocked;

			// Initialize coordinates.
			for(int y = 0; y < this.size; ++y)
			for(int x = RowSizeOf(y) - 1; x >= 0; --x)
				this.coordinates[IndexOf(x, y)] = new Position(x, y);
		}

		public Board Copy()
		{
			var board = new Board();
			board.fields = new Field[this.fields.Length];
			board.coordinates = new Position[this.coordinates.Length];
			board.size = this.size;

			Array.Copy(this.fields, board.fields, this.fields.Length);
			Array.Copy(this.coordinates, board.coordinates, this.coordinates.Length);

			return board;
		}

		public void PrintToConsole()
		{
			var maxRowSize = RowSizeOf(this.size - 1);

			for(int y = this.size - 1; y >= 0; --y)
			{
				var rowSize = RowSizeOf(y);

				for(int s = maxRowSize - rowSize; s >= 0; --s)
					Console.Write(" ");

				Console.Write("\\");

				for(int x = 0; x < rowSize; ++x)
				{
					var index = IndexOf(x, y);
					
					if(this.fields[index] == Field.Free)
						Console.Write(" ");
					else if(this.fields[index] == Field.Blocked)
						Console.Write("X");
					else if(this.fields[index] == Field.Stone)
						Console.Write("O");
					Console.Write(x % 2 == 0 ? "/" : "\\");
				}

				Console.Write("\n");
			}
		}

		public Position GetPosition(int index) => this.coordinates[index];

		public Span<int> GetNeighbors(int index, Span<int> buffer)
		{
			var count = 0;
			var x = this.coordinates[index].x;
			var y = this.coordinates[index].y;

			buffer[count] = index - 1;
			count += x > 0 ? 1 : 0;

			buffer[count] = index + 1;
			count += x < y * 2 ? 1 : 0;

			int isOdd = x % 2;
			buffer[count] = index
				+ (1 - isOdd) * (2 * (y + 1))
				+ (1 * isOdd) * (-2 * y);

			count += (uint)buffer[count] < this.fields.Length ? 1 : 0;

			return buffer.Slice(0, count);
		}

		public Span<int> GetFields(Span<int> fields, Field filter, Span<int> buffer)
		{
			int count = 0;

			for(int i = 0; i < fields.Length; ++i)
				if(this.fields[fields[i]] == filter)
					buffer[count++] = fields[i];

			return buffer.Slice(0, count);
		}

		public int GetFreeNeighborCount(PlayState state)
		{
			Span<int> neighbors = stackalloc int[6];

			var nFirst = GetNeighbors(state.first, neighbors);
			var nSecond = GetNeighbors(state.second, neighbors.Slice(3));
			var count = 0;

			for(int i = 0; i < nFirst.Length; ++i)
				count += this.fields[nFirst[i]] == Field.Free ? 1 : 0;
		
			for(int i = 0; i < nSecond.Length; ++i)
				count += this.fields[nSecond[i]] == Field.Free ? 1 : 0;

			return count;
		}

		public int GetFreeFieldCount(Span<int> fields)
		{
			int count = 0;

			for(int i = 0; i < fields.Length; ++i)
				count += this.fields[fields[i]] == Field.Free ? 1 : 0;
				
			return count;
		}

		public static int NumberOfFields(int size)
		{
			--size;
			return size * size + RowSizeOf(size);
		}

		public static int RowSizeOf(int y) => 1 + (y * 2);
		
		public static int IndexOf(int x, int y) => (y * y) + x;
	}
}