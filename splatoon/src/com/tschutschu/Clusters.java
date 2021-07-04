package com.tschutschu;

import lenz.htw.coast.net.NetworkClient;
import lenz.htw.coast.world.GraphNode;

import java.util.Random;

public class Clusters extends Thread
{
    public static final boolean VERBOSE = false;

    private class Cluster
    {
        public int count;

        public Vector3 absolute;

        public Vector3 centroid;

        public GraphNode node;

        public Vector3 old;

        public Cluster()
        {
            this.count = 0;
            this.absolute = Vector3.Zero();
            this.centroid = Vector3.Zero();
            this.old = Vector3.Zero();
        }

        public void Randomize(Random random)
        {
            this.centroid.Set(random, -1f, 1f);
        }

        public void UpdateCentroid()
        {
            this.centroid = new Vector3(absolute);
            this.centroid.Devide(count);
            this.centroid.Normalize();
        }

        public void Reset()
        {
            this.old.Set(this.centroid);
            this.count = 0;
            this.absolute.Set(0f);
        }

        public void Add(GraphNode node)
        {
            count += 1;
            absolute.Add(node.x, node.y, node.z);
        }
    }

    public NetworkClient client;

    public Cluster[][] cluster;

    public int iterations;

    public Random random;

    private final Object lock = new Object();

    public Clusters(NetworkClient client, int iterations)
    {
        this.client = client;
        this.iterations = iterations;
        this.random = new Random(iterations);
        this.cluster = new Cluster[4][];

        for(int i = 0; i < this.cluster.length; ++i)
            this.cluster[i] = new Cluster[] {
                new Cluster(), new Cluster(), new Cluster()
            };
    }

    public GraphNode[] GetCentroids(int player)
    {
        synchronized (this.lock)
        {
            Cluster[] candidates = this.cluster[player];
            GraphNode[] centroids = new GraphNode[candidates.length];

            for(int i = 0; i < candidates.length; ++i)
                centroids[i] = candidates[i].node;

            return centroids;
        }
    }

    public void run()
    {
        while(this.client.isAlive())
        {
            Cluster();

            if(!this.client.isAlive())
                break;

            try {
                Thread.sleep(1500);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    private void Cluster() {
        GraphNode[] nodes = client.getGraph();
        Cluster[][] buffer = new Cluster[4][];

        for(int i = 0; i < buffer.length; ++i)
            buffer[i] = new Cluster[] {
                    new Cluster(), new Cluster()
            };


        for(int j = 0; j < buffer.length; ++j)
            RandomizeAll(this.random, buffer[j]);

        for (int i = 0; i < this.iterations; ++i)
        {
            for(int j = 0; j < buffer.length; ++j)
                ResetAll(buffer[j]);

            for(int n = 0; n < nodes.length; ++n)
            {
                Cluster closest = ClosestCluster(nodes[n], buffer[nodes[n].owner]);
                closest.Add(nodes[n]);
            }

            for(int j = 0; j < buffer.length; ++j)
                UpdateCentroidAll(buffer[j]);
        }

        float[][] distance = new float[buffer.length][];

        for(int i = 0; i < distance.length; ++i)
        {
            distance[i] = new float[buffer[i].length];

            for(int j = 0; j < distance[i].length; ++j)
                distance[i][j] = Float.MAX_VALUE;
        }

        for(int n = 0; n < nodes.length; ++n)
        {
            for(int i = 0; i < buffer.length; ++i)
                for(int j = 0; j < buffer[i].length; ++j)
                {
                    float d = World.Distance(buffer[i][j].centroid, nodes[n]);

                    if(d < distance[i][j])
                    {
                        distance[i][j] = d;
                        buffer[i][j].node = nodes[n];
                    }
                }
        }

        synchronized(this.lock) {
            this.cluster = buffer;
        }

        if(VERBOSE)
        {
            for(int i = 0; i < buffer.length; ++i)
            {
                System.out.println("Centroids for player " + i + ":");
                for(int j = 0; j < buffer[i].length; ++j)
                {
                    System.out.println("\t Position: " + buffer[i][j].centroid);
                    System.out.println("\t Count: " + buffer[i][j].count);
                }
            }
        }
    }

    private static void RandomizeAll(Random random, Cluster[] cluster)
    {
        for(int i = 0; i < cluster.length; ++i)
            cluster[i].Randomize(random);
    }

    private static void ResetAll(Cluster[] cluster)
    {
        for(int i = 0; i < cluster.length; ++i)
            cluster[i].Reset();
    }

    private static void UpdateCentroidAll(Cluster[] cluster)
    {
        for(int i = 0; i < cluster.length; ++i)
            cluster[i].UpdateCentroid();
    }

    private static Cluster ClosestCluster(GraphNode node, Cluster[] cluster)
    {
        float distance = Float.MAX_VALUE;
        int index = 0;

        for(int i = 0; i < cluster.length; ++i)
        {
            float d = World.Distance(cluster[i].centroid, node);

            if(d < distance)
            {
                distance = d;
                index = i;
            }
        }

        return cluster[index];
    }
}
