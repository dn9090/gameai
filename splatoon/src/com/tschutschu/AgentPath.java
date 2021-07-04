package com.tschutschu;

import lenz.htw.coast.world.GraphNode;

public class AgentPath
{
    public static final float POINT_RADIUS = 0.036f;

    public int index;

    public SpaceTimePoint[] points;

    public void Restart(SpaceTimePoint[] points)
    {
        this.index = 0;
        this.points = points;
    }

    public GraphNode Next(World world)
    {
        return world.nodes[this.points[this.index].index];
    }

    public boolean Follow(World world, Agent agent)
    {
        if(this.points == null)
            return false;

        GraphNode current = world.nodes[this.points[this.index].index];
        float distance = World.Distance(agent.position, current);

        if(distance < POINT_RADIUS)
        {
            if(this.index + 1 >= this.points.length)
                return false;

            this.index = FindNextPathNode(world, agent, distance);
        }

        return true;
    }

    private int FindNextPathNode(World world, Agent agent, float distance)
    {
        for(int i = points.length - 1; i > this.index; --i)
        {
            GraphNode next = world.nodes[this.points[i].index];
            float d = World.Distance(agent.position, next);

            if(d < distance)
                return i;
        }

        return this.index + 1;
    }
}
