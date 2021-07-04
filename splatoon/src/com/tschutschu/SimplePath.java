package com.tschutschu;

import lenz.htw.coast.world.GraphNode;

public class SimplePath
{
    public static SpaceTimePoint[] ScanNearby(World world, Agent agent)
    {
        float distance = World.Distance(agent.position, agent.goal);

        if(distance < 3f * AgentPath.POINT_RADIUS)
            return null;

        GraphNode a = ScanNextDepth(agent, distance, agent.node);

        if(a == null)
            return null;

        GraphNode b = ScanNextDepth(agent, distance, a);

        if(b == null)
            return null;

        GraphNode c = ScanNextDepth(agent, distance, b);

        if(c == null)
            return null;

        return new SpaceTimePoint[] {
                new SpaceTimePoint(world.nodeToIndex.get(a), 1),
                new SpaceTimePoint(world.nodeToIndex.get(b), 1),
                new SpaceTimePoint(world.nodeToIndex.get(c), 1)
        };
    }

    private static GraphNode ScanNextDepth(Agent agent, float maxDistance, GraphNode node)
    {
        if(node == null)
                return null;

        GraphNode next = null;
        float distance = maxDistance;

        for(int i = 0; i < node.neighbors.length; ++i)
        {
            if(node.neighbors[i].blocked)
                continue;

            float d = World.Distance(agent.position, node.neighbors[i]);

            if(d < distance)
            {
                if(node.neighbors[i].owner != (agent.player + 1))
                {
                    next = node.neighbors[i];
                    distance = d;
                }
            }
        }

        return next;
    }
}
