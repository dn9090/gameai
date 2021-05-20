using System;
using System.Numerics;
using System.Net;
using System.Net.Sockets;

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

		public unsafe static Message4 Read(NetworkStream stream)
		{
			byte* bytes = stackalloc byte[sizeof(Message4)];

			stream.Read(new Span<byte>(bytes, sizeof(Message4)));

			return *((Message4*)bytes);
		}
	}

}