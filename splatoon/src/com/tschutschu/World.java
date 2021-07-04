package com.tschutschu;

import lenz.htw.coast.net.NetworkClient;
import lenz.htw.coast.world.GraphNode;

import java.util.*;

public class World
{
    public GraphNode[] nodes;

    public int[] blockedNeighbors;

    public Hashtable<GraphNode, Integer> nodeToIndex;

    public Vector3[][] bots;

    public GraphNode[][] botToNode;

    private World(GraphNode[] nodes)
    {
        this.nodes = nodes;
        this.blockedNeighbors = new int[this.nodes.length];
        this.nodeToIndex = new Hashtable<>();
        this.bots = new Vector3[3][];
        this.botToNode = new GraphNode[3][];

        for(int i = 0; i < this.bots.length; ++i)
        {
            this.bots[i] = new Vector3[] {
                Vector3.Zero(),
                Vector3.Zero(),
                Vector3.Zero()
            };
            this.botToNode[i] = new GraphNode[3];
        }
    }

    public float GetBlockedNeighborShare(int index)
    {
        return (float)this.blockedNeighbors[index] / (float)this.nodes[index].neighbors.length;
    }

    public float GetNegativeImpact(int index, int player)
    {
        float impact = this.nodes[index].owner == player + 1 ? 2 : 0;
        impact = this.nodes[index].owner == 0 ? 1 : impact;
        return impact;
    }

    public void Update(NetworkClient client)
    {
        float[][] distance = new float[bots.length][];

        for(int i = 0; i < this.bots.length; ++i)
        {
            this.bots[i][Bot.FAST].Set(client.getBotPosition(i, Bot.FAST));
            this.bots[i][Bot.UNSTOPPABLE].Set(client.getBotPosition(i, Bot.UNSTOPPABLE));
            this.bots[i][Bot.FAT].Set(client.getBotPosition(i, Bot.FAT));

            distance[i] = new float[3];
            Arrays.fill(distance[i], Float.POSITIVE_INFINITY);
        }

        for(int i = 0; i < nodes.length; ++i)
        for(int p = 0; p < bots.length; ++p)
        for(int b = 0; b < bots[p].length; ++b)
        {
            float d = Vector3.SqrDistance(bots[p][b],
                    nodes[i].x, nodes[i].y, nodes[i].z);

            if(d < distance[p][b])
            {
                distance[p][b] = d;
                this.botToNode[p][b] = nodes[i];
            }
        }
    }

    public static World Create(NetworkClient client)
    {
        GraphNode[] nodes = client.getGraph();
        World world = new World(nodes);

        for(int i = 0; i < nodes.length; ++i)
        {
            world.nodeToIndex.put(nodes[i], i);

            for(int n = 0; n < nodes[i].neighbors.length; ++n)
                world.blockedNeighbors[i] += nodes[i].neighbors[n].blocked ? 1 : 0;
        }

        world.Update(client);

        return world;
    }

    public static float Distance(GraphNode lhs, GraphNode rhs)
    {
        return Distance(new Vector3(lhs.x, lhs.y, lhs.z), new Vector3(rhs.x, rhs.y, rhs.z));
    }

    public static float Distance(Vector3 position, GraphNode node)
    {
        Vector3 v = new Vector3(node.x, node.y, node.z);

        double dot = Vector3.Dot(v, position);
        v.Cross(position);

        return (float)Math.atan(v.Magnitude() / dot);
    }

    public static float Distance(Vector3 lhs, Vector3 rhs)
    {
        Vector3 v = new Vector3(rhs);

        double dot = Vector3.Dot(v, lhs);
        v.Cross(lhs);

        return (float)Math.atan(v.Magnitude() / dot);
    }
}
