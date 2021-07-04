package com.tschutschu;

import lenz.htw.coast.net.NetworkClient;
import lenz.htw.coast.world.GraphNode;

public class AgentStrategy
{
    public NetworkClient client;

    public Clusters clusters;

    public AgentStrategy(NetworkClient client, Clusters clusters)
    {
        this.client = client;
        this.clusters = clusters;
    }

    public GraphNode GetDefaultGoal(World world, Agent agent)
    {
        int targetPlayer = 0;
        float distance = Float.MIN_VALUE;

        for(int i = 0; i < world.bots.length; ++i)
        {
            if(i == agent.player)
                continue;

            float d = World.Distance(agent.position, world.bots[i][agent.bot]);

            if(d > distance)
            {
                targetPlayer = i;
                distance = d;
            }
        }

        return GetGoalOrNeighbor(world.botToNode[targetPlayer][agent.bot]);
    }

    public GraphNode GetGoal(World world, Agent agent)
    {
        int best = GetBestOpponent(agent.player);

        GraphNode[] centroids = this.clusters.GetCentroids(best + 1);

        GraphNode goal = agent.bot == Bot.FAST
                ? GetFarthest(centroids, agent)
                : GetClosest(centroids, agent);

        if(goal == null || World.Distance(agent.position, goal) < 3f * AgentPath.POINT_RADIUS)
            return GetDefaultGoal(world, agent);

        return GetGoalOrNeighbor(goal);
    }


    private GraphNode GetGoalOrNeighbor(GraphNode goal)
    {
        if(goal.blocked)
        {
            for(int n = 0; n < goal.neighbors.length; ++n)
                if(!goal.neighbors[n].blocked)
                    return goal.neighbors[n];
        }

        return goal;
    }

    private GraphNode GetClosest(GraphNode[] nodes, Agent agent)
    {
        float distance = Float.MAX_VALUE;
        GraphNode closest = null;

        for(int i = 0; i < nodes.length; ++i)
        {
            if(nodes[i] == null || nodes[i] == agent.node)
                continue;

            float d = World.Distance(agent.position, nodes[i]);

            if(d < distance)
            {
                d = distance;
                closest = nodes[i];
            }
        }

        return closest;
    }

    private GraphNode GetFarthest(GraphNode[] nodes, Agent agent)
    {
        float distance = Float.MIN_VALUE;
        GraphNode farthest = null;

        for(int i = 0; i < nodes.length; ++i)
        {
            if(nodes[i] == null || nodes[i] == agent.node)
                continue;

            float d = World.Distance(agent.position, nodes[i]);

            if(d > distance)
            {
                d = distance;
                farthest = nodes[i];
            }
        }

        return farthest;
    }

    private int GetBestOpponent(int player)
    {
        int[] scores = new int[2];
        int sum = 0;
        int count = 0;

        for(int i = 0; i < Player.opponents[player].length; ++i)
            sum += scores[count++] = client.getScore(Player.opponents[player][i]);

        return (float)scores[0] / (float)sum > (float)scores[1] / (float)sum ? Player.opponents[player][0] : Player.opponents[player][1];
    }
}
