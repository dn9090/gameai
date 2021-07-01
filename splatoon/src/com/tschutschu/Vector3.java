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

    public Vector3(Vector3 value)
    {
        this.x = value.x;
        this.y = value.y;
        this.z = value.z;
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

    public void Set(float value)
    {
        this.x = value;
        this.y = value;
        this.z = value;
    }

    public void Set(Vector3 value)
    {
        this.x = value.x;
        this.y = value.y;
        this.z = value.z;
    }

    public void Add(Vector3 rhs)
    {
        this.x += rhs.x;
        this.y += rhs.y;
        this.z += rhs.z;
    }

    public void Substract(Vector3 rhs)
    {
        this.x -= rhs.x;
        this.y -= rhs.y;
        this.z -= rhs.z;
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

    public float Distance(Vector3 rhs)
    {
        return (float)Math.sqrt(SqrDistance(rhs));
    }

    public float SqrDistance(Vector3 rhs)
    {
        float x = rhs.x - this.x;
        float y = rhs.y - this.y;
        float z = rhs.z - this.z;

        return x * x + y * y + z * z;
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

    public static float SqrDistance(float lx, float ly, float lz, float rx, float ry, float rz)
    {
        float x = rx - lx;
        float y = ry - ly;
        float z = rz - lz;

        return x * x + y * y + z * z;
    }

    public static float SqrDistance(Vector3 position, float rx, float ry, float rz)
    {
        return SqrDistance(position.x, position.y, position.z, rx, ry, rz);
    }

    public static float Distance(float lx, float ly, float lz, float rx, float ry, float rz)
    {
        return (float)Math.sqrt(SqrDistance(lx, ly, lz, rx, ry, rz));
    }

    public static float Distance(Vector3 position, float rx, float ry, float rz)
    {
        return (float)Math.sqrt(SqrDistance(position, rx, ry, rz));
    }
}
