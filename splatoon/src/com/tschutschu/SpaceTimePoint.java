package com.tschutschu;

public final class SpaceTimePoint
{
    public int index;

    public int time;

    public SpaceTimePoint(int index, int time)
    {
        this.index = index;
        this.time = time;
    }

    @Override
    public String toString() {
        return "(" + this.index + "," + this.time + ")";
    }

    @Override
    public boolean equals(Object o) {
        if (this == o)
            return true;

        SpaceTimePoint other = (SpaceTimePoint) o;
        return other.index == this.index && other.time == this.time;
    }

    @Override
    public int hashCode() {
        return 31 * this.index + this.time;
    }
}
