using UnityEngine;

public class Cuboid
{
    public Vector3 centre;

    public float width;
    public float height;
    public float depth;

    //Constructor
    public Cuboid(Vector3 centre, float width, float height, float depth) 
    {
        this.centre = centre;
        
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    //Check is a given point is within the boundary of this cuboid
    public bool Contains(Point point) 
    {
        bool inWidth = (point.x >= centre.x - width && point.x < centre.x + width);
        bool inHeight = (point.y >= centre.y - height && point.y < centre.y + height);
        bool inDepth = (point.z >= centre.z - depth && point.z < centre.z + depth);

        return (inWidth && inHeight && inDepth);
    }

    //Given a spherical area, check if the bounds of this cuboid intersect the area
    public bool IntersectsSphere(Vector3 sphereCentre, float radius)
    {
        //As explained at https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection
        //We find the coodrinate of the closest point on the cuboid to the sphere
        float closestX = Mathf.Max(centre.x - width, Mathf.Min(sphereCentre.x, centre.x + width));
        float closestY = Mathf.Max(centre.y - height, Mathf.Min(sphereCentre.y, centre.y + height));
        float closestZ = Mathf.Max(centre.z - depth, Mathf.Min(sphereCentre.z, centre.z + depth));

        //Check if the distance between this and the sphere centre is less than the radius
        float sqrDistance = (closestX - sphereCentre.x) * (closestX - sphereCentre.x) +
                            (closestY - sphereCentre.y) * (closestY - sphereCentre.y) +
                            (closestZ - sphereCentre.z) * (closestZ - sphereCentre.z);
        
        return (sqrDistance < radius * radius);

        // //TESTING VARIANT TO BE USED WITH THE COLLISION VISUALISER SCRIPT IN "Extra Scripts"
        // //FUNCTION NEEDS TO RETUN float[] FOR THIS
        // float[] testArray = new float[4];
        // testArray[0] = closestX;
        // testArray[1] = closestY;
        // testArray[2] = closestZ;
        // testArray[3] = (sqrDistance < radius * radius) ? 1 : 0;
        // return testArray;
    }
}
