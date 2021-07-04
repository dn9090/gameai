package com.tschutschu;

import lenz.htw.coast.net.NetworkClient;
import lenz.htw.coast.world.GraphNode;

public class Agent
{
    public int player;

    public int bot;

    public Vector3 position;

    public GraphNode node;

    public GraphNode goal;

    public AgentPath path;

    public AgentStrategy strategy;

    public Agent(int player, int bot)
    {
        this.player = player;
        this.bot = bot;
        this.path = new AgentPath();
        this.position = Vector3.Zero();
    }

    public boolean ReachedTarget()
    {
        return this.node == this.goal;
    }

    public GraphNode Target(World world)
    {
        if(this.path.points == null)
            return goal;
        return this.path.Next(world);
    }

    public String toString() {
        return "Agent(" + this.player + ", " + this.bot + ", " + this.position + ", <"
                + this.node.x + ", " + this.node.y + ", " + this.node.z + "> => <"
                + this.goal.x + ", " + this.goal.y + ", " + this.goal.z + ">)";
    }

    public static Agent[] ForPlayer(int player)
    {
        return new Agent[] {
                new Agent(player, Bot.FAST),
                new Agent(player, Bot.UNSTOPPABLE),
                new Agent(player, Bot.FAT)
        };
    }

    public static void RefreshData(World world, Agent... agents)
    {
        float[] distance = new float[agents.length];

        for(int i = 0; i < agents.length; ++i)
        {
            agents[i].node = world.botToNode[agents[i].player][agents[i].bot];
            agents[i].position.Set(world.bots[agents[i].player][agents[i].bot]);
        }
    }

    public static void SendData(NetworkClient client, World world, Agent... agents)
    {
        for(int i = 0; i < agents.length; ++i)
        {
            GraphNode target = agents[i].Target(world);
            client.changeMoveDirection(agents[i].bot, target.x, target.y, target.z);
        }
    }

    public static void UpdateCooperative(World world, AgentStrategy strategy, Agent... agents)
    {
        boolean pathPlanning = false;

        for(int i = 0; i < agents.length; ++i)
        {
            if(agents[i].goal == null)
            {
                agents[i].goal = strategy.GetDefaultGoal(world, agents[i]);
                pathPlanning = true;
                continue;
            }

            boolean follow = agents[i].path.Follow(world, agents[i]);

            if(!follow || agents[i].ReachedTarget())
            {
                System.out.println("Reached Target! " + agents[i].bot);
                agents[i].goal = strategy.GetGoal(world, agents[i]);
                pathPlanning = true;
            }
        }

        if(pathPlanning)
        {
            SpaceTimePoint[][] points = CooperativeAStar.Pathfind(world, agents);

            for(int i = 0; i < points.length; ++i)
                agents[i].path.Restart(points[i]);
        }
    }

    public static void UpdateDirectMover(World world, AgentStrategy strategy, Agent agent)
    {
        if(agent.goal == null || agent.ReachedTarget() || World.Distance(agent.position, agent.node) < AgentPath.POINT_RADIUS)
            agent.goal = strategy.GetDefaultGoal(world, agent);

        //boolean follow = agent.path.Follow(world, agent);

        //if(!follow)
         //   agent.path.Restart(SimplePath.ScanNearby(world, agent));
    }
}
