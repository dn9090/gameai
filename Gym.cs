using System;
using System.Numerics;

namespace BlocksAI
{
	public struct Gym
	{
		public float mutationStrength;

		public int seed;

		public Gym(float mutationStrength, int seed)
		{
			this.mutationStrength = mutationStrength;
			this.seed = seed;
		}

		private static Hyperparameters Mutate(Hyperparameters hyperparams)
		{
			return hyperparams; // *mutationStrength ... Mutate somehow
		}

		private static Hyperparameters Randomize(Hyperparameters hyperparams)
		{
			return hyperparams; // *seed ... Randomize
		}

		private static bool Won(ref Game game, ref AIAgent agent)
		{
			var score = game.score[agent.player];
			var next = agent.player;

			while((next = game.NextPlayer(next)) != agent.player)
			{
				if(game.score[next] > score)
					return false;
			}

			return true;
		}

		public Hyperparameters Train(int iterations, Hyperparameters hyperparams, int maxDepth)
		{
			AIAgent best = new AIAgent(maxDepth, hyperparams);
			AIAgent mutated = new AIAgent(maxDepth, Mutate(hyperparams));
			AIAgent random = new AIAgent(maxDepth, Randomize(hyperparams));

			var won = new int[] { 0, 0, 0 };

			for(int epoch = 0; epoch < iterations; ++epoch)
			{
				for(int games = 0; games < 3; ++games)
				{
					best.player = games % 3;
					mutated.player = (games + 1) % 3;
					random.player = (games + 2) % 3;

					Game game = Game.Create();
					game.Start();

					for(int rounds = 0; rounds < 42; ++rounds)
					{
						Move next = Move.Empty();
						var turn = rounds % 3;

						if(turn == best.player)
							next = best.Minimax(ref game);
						else if(turn == mutated.player)
							next = mutated.Minimax(ref game);
						else if(turn == random.player)
							next = random.Minimax(ref game);
						
						if(!next.isEmpty)
							game.Play(next);
					}

					won[0] += Won(ref game, ref best) ? 1 : 0;
					won[1] += Won(ref game, ref mutated) ? 1 : 0;
					won[2] += Won(ref game, ref random) ? 1 : 0;
				}

				// @Todo: Evaluate won games...
				// @Todo: Verify...
				// @Todo: Mutate and randomize again, assign best value...
			}

			return new Hyperparameters(); // @Todo: Return best hyperparameters...
		}

		private bool Verify(int iteration, Hyperparameters hyperparams, int maxDepth)
		{
			RandomAgent a = new RandomAgent(this.seed + 100);
			RandomAgent b = new RandomAgent(this.seed + 200);
			AIAgent agent = new AIAgent(maxDepth, hyperparams);

			for(int games = 0; games < 3; ++games)
			{
				a.player = games % 3;
				b.player = (games + 1) % 3;
				agent.player = (games + 2) % 3;

				Game game = Game.Create();
				game.Start();

				for(int rounds = 0; rounds < 42; ++rounds)
				{
					Move next = Move.Empty();
					var turn = rounds % 3;

					if(turn == a.player)
						next = a.Next(ref game);
					else if(turn == b.player)
						next = b.Next(ref game);
					else if(turn == agent.player)
						next = agent.Minimax(ref game);
					
					if(!next.isEmpty)
						game.Play(next);
				}

				// @Todo: Evaluate scores...
			}


			return true;
		}
	}
}