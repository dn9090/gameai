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

	    while(client.isAlive())
		{
			World world = World.Create(client.getGraph());
			float[] pos = client.getBotPosition(playerNum, 0);

			//client.

			System.out.println("Nodes: " + world.nodes.length);
			System.out.println("Neighbors: " + world.neighbors.length);
			System.out.println("Max Neighbors: " + world.maxNeighbors);
			break;
		}
    }
}
