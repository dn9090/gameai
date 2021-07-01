package com.tschutschu;

import lenz.htw.coast.world.GraphNode;

public class AgentPath
{
    public static final float POINT_RADIUS = 0.2f;

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

        if(distance < POINT_RADIUS) //|| Overshoot(world, agent, current))
        {
            //System.out.println("Follow next, overshoot? " + Overshoot(world, agent, current));

            if(this.index + 1 >= this.points.length)
                return false;

            this.index = FindNextPathNode(world, agent, distance);
        }

        return true;
    }

    private boolean Overshoot(World world, Agent agent, GraphNode current)
    {
        if(this.index <= 0)
            return false;

        GraphNode last = world.nodes[this.points[this.index - 1].index];
        return World.Distance(current, last) < World.Distance(agent.position, last);
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
