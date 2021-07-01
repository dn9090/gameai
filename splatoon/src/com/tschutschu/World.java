package com.tschutschu;

import lenz.htw.coast.world.GraphNode;

import java.util.*;

public class World
{
    public GraphNode[] testTargets;

    public GraphNode[] nodes;

    public int[] blockedNeighbors;

    public Hashtable<GraphNode, Integer> nodeToIndex;

    private World(GraphNode[] nodes)
    {
        this.nodes = nodes;
        this.blockedNeighbors = new int[this.nodes.length];
        this.nodeToIndex = new Hashtable<>();
    }

    public float GetBlockedNeightborShare(int index)
    {
        return this.blockedNeighbors[index] / this.nodes[index].neighbors.length;
    }

    public float GetNegativeImpact(int index, int player)
    {
        float impact = 0;
        impact = this.nodes[index].owner == player + 1 ? 2 : 0;
        impact = this.nodes[index].owner == 0 ? 1 : 0;
        return impact;
    }

    public static World Create(GraphNode[] nodes)
    {
        World world = new World(nodes);

        for(int i = 0; i < nodes.length; ++i)
        {
            world.nodeToIndex.put(nodes[i], i);

            for(int n = 0; n < nodes[i].neighbors.length; ++n)
                world.blockedNeighbors[i] += nodes[i].neighbors[n].blocked ? 1 : 0;
        }

        world.testTargets = new GraphNode[2];
        Random r = new Random(1);

        for(int i = 0; i < world.testTargets.length; ++i)
        {
            int index = 0;
            do
            {
                index = r.nextInt(world.testTargets.length);
            } while(world.nodes[index].blocked);
            world.testTargets[i] = world.nodes[index];
        }

        return world;
    }

    public static float Distance(GraphNode a, GraphNode b)
    {
        return Distance(new Vector3(a.x, a.y, a.z), b);
    }

    public static float Distance(Vector3 position, GraphNode node)
    {
        Vector3 v = new Vector3(node.x, node.y, node.z);

        double dot = Vector3.Dot(v, position);
        v.Cross(position);

        return (float)Math.atan(v.Magnitude());
    }
}
