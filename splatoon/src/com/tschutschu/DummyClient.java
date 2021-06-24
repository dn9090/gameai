package com.tschutschu;

import lenz.htw.coast.net.NetworkClient;

public class DummyClient extends Thread
{
    @Override
    public void run()
    {
        NetworkClient client = new NetworkClient(null, "Dummy Client", "Dummy muede.");
        while(!client.isGameRunning()) {}
    }
}
