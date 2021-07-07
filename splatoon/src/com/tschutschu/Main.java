package com.tschutschu;

import lenz.htw.coast.*;
import lenz.htw.coast.net.NetworkClient;
import lenz.htw.coast.world.GraphNode;

public class Main {

    public static void main(String[] args) {
		DummyClient c1 = new DummyClient();
		DummyClient c2 = new DummyClient();

		c1.start();
		c2.start();

	    NetworkClient client = new NetworkClient(null, "Tschu tschu", "Wann gehts los?");

	    while(!client.isGameRunning()) {}

	    int playerNum = client.getMyPlayerNumber();

		client.isAlive();

		Player.Initialize(client);
		World world = World.Create(client);
		Clusters clusters = new Clusters(client, 12);
		AgentStrategy strategy = new AgentStrategy(client, clusters);

		clusters.start();

		Agent[] agents = Agent.ForPlayer(playerNum);
		Agent[] cooperative = new Agent[] {
			agents[Bot.FAT],
			agents[Bot.FAST]
		};

		//try {Thread.sleep(500); } catch (Exception e) {}

	    while(client.isAlive())
		{
			world.Update(client);
			Agent.RefreshData(world, agents);
			Agent.UpdateCooperative(world, strategy, cooperative);
			Agent.UpdateDirectMover(world, strategy, agents[Bot.UNSTOPPABLE]);
			Agent.SendData(client, world, agents);

		}
    }
}
