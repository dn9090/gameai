package com.tschutschu;

import lenz.htw.coast.net.NetworkClient;
import lenz.htw.coast.world.GraphNode;

public class Agent
{
    public static final int BOT_FAST = 0;

    public static final int BOT_UNSTOPPABLE = 1;

    public static final int BOT_FAT = 2;

    public int player;

    public int bot;

    public Vector3 position;

    public Vector3 velocity;

    public GraphNode node;

    public GraphNode goal;

    public AgentPath path;

    public Agent(int player, int bot)
    {
        this.player = player;
        this.bot = bot;
        this.path = new AgentPath();
        this.position = Vector3.Zero();
        this.velocity = Vector3.Zero();
    }

    public void UpdateTransform(Vector3 position)
    {
        this.velocity.Set(position);
        this.velocity.Substract(this.position);
        this.velocity.Devide(Time.deltaTime);
        this.position = position;
    }

    public boolean ReachedTarget()
    {
        return this.node == this.goal;
    }

    public static void RefreshData(NetworkClient client, World world, Agent... agents)
    {
        float[] distance = new float[agents.length];

        for(int i = 0; i < agents.length; ++i)
        {
            float[] p = client.getBotPosition(agents[i].player, agents[i].bot);
            agents[i].UpdateTransform(new Vector3(p[0], p[1], p[2]));
            distance[i] = Float.POSITIVE_INFINITY;
        }

        GraphNode[] nodes = world.nodes;

        for(int i = 0; i < nodes.length; ++i)
        for(int a = 0; a < agents.length; ++a)
        {
            float d = Vector3.SqrDistance(agents[a].position,
                    nodes[i].x, nodes[i].y, nodes[i].z);
            if(d < distance[a])
            {
                distance[a] = d;
                agents[a].node = nodes[i];
            }
        }
    }

    public static void SendData(NetworkClient client, World world, Agent... agents)
    {
        for(int i = 0; i < agents.length; ++i)
        {
            GraphNode target = agents[i].path.Next(world);
            client.changeMoveDirection(agents[i].bot, target.x, target.y, target.z);
        }
    }

    public static int c = 0;

    public static void Update(World world, Agent... agents)
    {
        boolean pathPlanning = false;

        for(int i = 0; i < agents.length; ++i)
        {
            boolean follow = agents[i].path.Follow(world, agents[i]);

            if(!follow || agents[i].ReachedTarget() || agents[i].goal == null)
            {
                System.out.println("Node is goal? " + agents[i].ReachedTarget());
                System.out.println("Can follow?" + follow);
                agents[i].goal = world.testTargets[c++ % world.testTargets.length]; // @Todo: probably some bot

                pathPlanning = true;
                continue;
            }
        }

        if(pathPlanning)
        {
            System.out.println("Require new plan!");

            SpaceTimePoint[][] points = CooperativeAStar.Pathfind(world, agents);

            for(int i = 0; i < points.length; ++i)
                agents[i].path.Restart(points[i]);
        }
    }
}
