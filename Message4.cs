using System;
using System.Numerics;

namespace BlocksAI
{
	public struct Message4
	{
		public int a;

		public int b;

		public int c;

		public int d;

		public Move ToMove() => new Move(a, b, c, d);

		public ClientSettings ToClientSettings() => new ClientSettings(a, b, c);

		public static Message4 Read()
		{
			return new Message4();
		}
	}

}