package com.tschutschu;

import lenz.htw.coast.world.GraphNode;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.Hashtable;
import java.util.PriorityQueue;

public class CooperativeAStar
{
    private static int Timestep(int[] history, int current)
    {
        int timestep = 0;

        while(history[current] != -1)
        {
            current = history[current];
            ++timestep;
        }

        return timestep;
    }

    private static SpaceTimePoint[] ReconstructPath(int[] history, int current)
    {
        ArrayList<Integer> path = new ArrayList<>();
        path.add(current);

        while(history[current] != -1)
        {
            current = history[current];
            path.add(current);
        }

        SpaceTimePoint[] reconstructedPath = new SpaceTimePoint[path.size()];

        for(int i = 0; i < reconstructedPath.length; ++i)
            reconstructedPath[i] = new SpaceTimePoint(path.get(path.size() - 1 - i), i);

        return reconstructedPath;
    }

    public static SpaceTimePoint[][] Pathfind(World world, Agent... agents)
    {
        Hashtable<SpaceTimePoint, Agent> reservationTable = new Hashtable<>();
        int estimatedMaxTime = 100;

        for(int i = 0; i < agents.length; ++i) // Avoid the goal of other agents.
        {
            int goal = world.nodeToIndex.get(agents[i].goal);
            for(int e = 0; e < estimatedMaxTime; ++e)
                reservationTable.put(new SpaceTimePoint(goal, e), agents[i]); // @Review: maybe skip this steps because agents are allowed to collide
        }

        SpaceTimePoint[][] points = new SpaceTimePoint[agents.length][];

        for(int i = 0; i < agents.length; ++i)
        {
            points[i] = AStar(world, agents[i], reservationTable);

            for(int s = 0; s < points[i].length; ++s)
                reservationTable.put(points[i][s], agents[i]);
        }

        return points;
    }

    private static SpaceTimePoint[] AStar(World world, Agent agent, Hashtable<SpaceTimePoint, Agent> reservationTable)
    {
        GraphNode[] nodes = world.nodes;

        int[] gScore = new int[nodes.length];
        float[] fScore = new float[nodes.length];

        int[] history = new int[nodes.length];
        byte[] closed = new byte[nodes.length];

        for(int i = 0; i < nodes.length; ++i)
        {
            gScore[i] = Integer.MAX_VALUE;
            fScore[i] = Float.POSITIVE_INFINITY;
            history[i] = -1;
        }

        int from = world.nodeToIndex.get(agent.node);
        int to = world.nodeToIndex.get(agent.goal);

        gScore[from] = 0;
        fScore[from] = World.Distance(agent.node, agent.goal);

        PriorityQueue<Integer> queue = new PriorityQueue<>(new Comparator<>() {
            @Override
            public int compare(Integer a, Integer b) {
                return Float.compare(fScore[a], fScore[b]);
            }
        });
        queue.add(from);

        int current = from;

        while(!queue.isEmpty())
        {
            current = queue.poll();

            if(current == to)
                return ReconstructPath(history, current);

            closed[current] = 1;

            int timestep = Timestep(history, current);
            int size = queue.size();
            GraphNode node = world.nodes[current];

            for(int n = 0; n < node.neighbors.length; ++n)
            {
                GraphNode neighborNode = node.neighbors[n];
                int neighbor = world.nodeToIndex.get(neighborNode);

                if(closed[neighbor] != 0 || neighborNode.blocked)
                    continue;

                Agent reservedFor = reservationTable.getOrDefault(new SpaceTimePoint(current, timestep), null);

                if(reservedFor != null && reservedFor != agent) // @Review: check for collision at t + 1
                    continue;

                int gTentative = gScore[current] + 1;

                if(gTentative >= gScore[neighbor])
                    continue;

                if(!queue.contains(neighbor))
                {
                    history[neighbor] = current;
                    gScore[neighbor] = gTentative;
                    fScore[neighbor] = gTentative + World.Distance(nodes[neighbor], agent.goal)
                            + world.GetBlockedNeighborShare(neighbor) * 5f
                            + world.GetNegativeImpact(neighbor, agent.player) * 3f;

                    queue.add(neighbor);
                }
            }

            if(size == queue.size()) // If nothing was added, insert wait node.
            {
                // @Todo
            }
        }

        // No path was found, but we need somewhere to go.
        // We are using the last result and hope that the path will be reevaluated in the future.
        return ReconstructPath(history, current);
    }
}
