package com.tschutschu;

public class Vector3
{
    public float x;

    public float y;

    public float z;

    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3(float value)
    {
        this.x = value;
        this.y = value;
        this.z = value;
    }

    public float Magnitude()
    {
        return (float)Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
    }

    public float SqrMagnitude()
    {
        return this.x * this.x + this.y * this.y + this.z * this.z;
    }

    public void Multiply(float value)
    {
        this.x *= value;
        this.y *= value;
        this.z *= value;
    }

    public void Devide(float value)
    {
        this.x /= value;
        this.y /= value;
        this.z /= value;
    }

    public void Hadamard(Vector3 rhs)
    {
        this.x *= rhs.x;
        this.y *= rhs.y;
        this.z *= rhs.z;
    }

    public void Cross(Vector3 rhs)
    {
        Cross(this, rhs);
    }

    public void Cross(Vector3 lhs, Vector3 rhs)
    {
        float x = lhs.y * rhs.z - lhs.z * rhs.y;
        float y = lhs.z * rhs.x - lhs.x * rhs.z;
        float z = lhs.x * rhs.y - lhs.y * rhs.x;
        this.x = x; // Needed cause dumb java has no structs.
        this.y = y;
        this.z = z;
    }

    public static float Dot(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
    }

    public static Vector3 Zero()
    {
        return new Vector3(0f);
    }

    public static Vector3 Right()
    {
        return new Vector3(1f, 0f, 0f);
    }

    public static Vector3 Up()
    {
        return new Vector3(0f, 1f, 0f);
    }

    public static Vector3 Forward()
    {
        return new Vector3(0f, 0f, 1f);
    }
}
