package com.tschutschu;

import lenz.htw.coast.net.NetworkClient;

public class Player
{
    public static int[][] opponents;

    public static void Initialize(NetworkClient client)
    {
        opponents = new int[3][];

        for(int i = 0; i < opponents.length; ++i)
        {
            opponents[i] = new int[opponents.length - 1];

            for(int j = 0; j < opponents[i].length; ++j)
                opponents[i][j] = (i + j + 1) % opponents.length;
        }
    }
}
