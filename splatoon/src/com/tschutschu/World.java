package com.tschutschu;

import com.sun.corba.se.impl.orbutil.graph.Graph;
import lenz.htw.coast.world.GraphNode;
import mikera.vectorz.Vector3;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.Hashtable;
import java.util.PriorityQueue;

public class World
{
    public GraphNode[] nodes;

    public int[] neighbors;

    public int maxNeighbors;

    public Hashtable<GraphNode, Integer> nodeToIndex;

    private World(GraphNode[] nodes)
    {
        this.nodes = nodes;
        this.nodeToIndex = new Hashtable<>();
    }

    public static World Create(GraphNode[] nodes)
    {
        World world = new World(nodes);
        world.maxNeighbors = 0;

        for(int i = 0; i < nodes.length; ++i)
        {
            if(nodes[i].neighbors.length > world.maxNeighbors)
                world.maxNeighbors = nodes[i].neighbors.length;

            world.nodeToIndex.put(nodes[i], i);
        }

        world.neighbors = new int[nodes.length * world.maxNeighbors];

        for(int i = 0; i < nodes.length; ++i)
        {
            for(int n = 0; n < nodes[i].neighbors.length; ++n)
                world.neighbors[i * world.maxNeighbors + n] = world.nodeToIndex.get(nodes[i].neighbors[n]);
            for(int n = nodes[i].neighbors.length; n < world.maxNeighbors; ++n)
                world.neighbors[i * world.maxNeighbors + n] = -1;
        }

        return world;
    }

    public float Distance(GraphNode a, GraphNode b)
    {
        Vector3 va = new Vector3(a.x, a.y, a.z);
        Vector3 vb = new Vector3(b.x, b.y, b.z);

        double dot = va.dotProduct(vb);
        va.crossProduct(vb);

        return (float)Math.atan(va.magnitude());
    }

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

        do
        {
            path.add(current);
            current = history[current];
        } while(history[current] != -1);

        SpaceTimePoint[] reconstructedPath = new SpaceTimePoint[path.size()];

        for(int i = 0; i < reconstructedPath.length; ++i)
            reconstructedPath[i] = new SpaceTimePoint(path.get(path.size() - 1 - i), i);

        return reconstructedPath;
    }

    public void Pathfind(Agent[] agents)
    {
        Hashtable<SpaceTimePoint, Agent> reservationTable = new Hashtable<>();
        int estimatedMaxTime = 100;

        for(int i = 0; i < agents.length; ++i) // Avoid the goal of other agents.
        {
            int goal = this.nodeToIndex.get(agents[i].goal);
            for(int e = 0; e < estimatedMaxTime; ++e)
            reservationTable.put(new SpaceTimePoint(goal, e), agents[i]); // @Review: maybe skip this steps because agents are allowed to collide
        }

        for(int i = 0; i < agents.length; ++i)
        {
            agents[i].path = CooperativeAStar(agents[i], reservationTable);

            for(int s = 0; s < agents[i].path.length; ++s)
                reservationTable.put(agents[i].path[s], agents[i]);
        }
    }

    private SpaceTimePoint[] CooperativeAStar(Agent agent, Hashtable<SpaceTimePoint, Agent> reservationTable)
    {
        int[] gScore = new int[this.nodes.length];
        float[] fScore = new float[this.nodes.length];

        int[] history = new int[this.nodes.length];
        byte[] closed = new byte[this.nodes.length]; // @Todo: add obstacles

        for(int i = 0; i < this.nodes.length; ++i)
        {
            gScore[i] = Integer.MAX_VALUE;
            fScore[i] = Float.POSITIVE_INFINITY;
            history[i] = -1;
        }

        int from = this.nodeToIndex.get(agent.position);
        int to = this.nodeToIndex.get(agent.goal);

        gScore[from] = 0;
        fScore[from] = Distance(agent.position, agent.goal);

        PriorityQueue<Integer> queue = new PriorityQueue<>();
        queue.add(from);

        while(!queue.isEmpty())
        {
            int current = queue.poll();

            if(current == to)
                return ReconstructPath(history, current);

            closed[current] = 1;

            int timestep = Timestep(history, current);
            int size = queue.size();

            for(int n = 0; n < this.maxNeighbors; ++n)
            {
                int neighbor = this.neighbors[current + n];

                if(neighbor == -1)
                    break;

                Agent reservedFor = reservationTable.getOrDefault(new SpaceTimePoint(current, timestep), null);

                if(closed[neighbor] != 0 || (reservedFor != null && reservedFor != agent)) // @Review: check for collision at t + 1
                    continue;

                int gTentative = gScore[current] + 1;

                if(gTentative >= gScore[neighbor])
                    continue;

                if(!queue.contains(neighbor))
                {
                    gScore[neighbor] = gTentative;
                    fScore[neighbor] = gTentative + Distance(this.nodes[neighbor], agent.goal); // @Review: maybe add color score
                    history[neighbor] = current;
                    queue.add(neighbor);
                }
            }

            if(size == queue.size()) // If nothing was added, insert wait node.
            {
                // @Todo
            }
        }

        return new SpaceTimePoint[0];
    }
}
