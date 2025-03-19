using UnityEngine;

//Simple class to act as the primitive object passed into the Octree

public class Point
{
    public float x;
    public float y;
    public float z;

    public int boidIndex;

    //Constructor
    public Point(float x, float y, float z, int index)
    {
        this.x = x;
        this.y = y;
        this.z = z;

        this.boidIndex = index;
    }
}
