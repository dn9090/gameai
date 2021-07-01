package com.tschutschu;

import lenz.htw.coast.*;
import lenz.htw.coast.net.NetworkClient;

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

		World world = World.Create(client.getGraph());
		float[] pos = client.getBotPosition(playerNum, 0);

		System.out.println("Nodes: " + world.nodes.length);

		Agent[] agents = new Agent[] {
				new Agent(client.getMyPlayerNumber(), Agent.BOT_FAT)
		};

		int i = 50000;

		try {Thread.sleep(500); } catch (Exception e) {}

		Time.Initialize();

	    while(client.isAlive())
		{
			Time.Update();

			if(i % 50000 == 0) System.out.println();
			Agent.RefreshData(client, world, agents);
			if(i % 50000 == 0) System.out.println("Position: " + agents[0].position.x + ", " + agents[0].position.y + ", " + agents[0].position.z);
			if(i % 50000 == 0) System.out.println("Node: " + agents[0].node.x + ", " + agents[0].node.y + ", " + agents[0].node.z);
			Agent.Update(world, agents);
			if(i % 50000 == 0) System.out.println("Goal: " + agents[0].goal.x + ", " + agents[0].goal.y + ", " + agents[0].goal.z);
			if(i % 50000 == 0) System.out.println("Target: " + agents[0].path.Next(world).x + ", " + agents[0].path.Next(world).y + ", " + agents[0].path.Next(world).z);
			Agent.SendData(client, world, agents);

			++i;
		}
    }
}
