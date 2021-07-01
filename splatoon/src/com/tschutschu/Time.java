package com.tschutschu;

public class Time
{
    public static float time;

    public static float deltaTime;

    public static void Initialize()
    {
        time = (float)System.currentTimeMillis() / 1000f;
        deltaTime = 0f;
    }

    public static void Update()
    {
        float currentTime = (float)System.currentTimeMillis() / 1000f;
        deltaTime = currentTime - time;
        time = currentTime;
    }
}
