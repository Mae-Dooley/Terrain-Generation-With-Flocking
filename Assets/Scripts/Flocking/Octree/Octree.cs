using UnityEngine;
using System.Collections.Generic;

public class Octree
{
    public Point point = null;
    public Cuboid boundary;
    public bool divided;

    public Octree[] children;

    //Constructor
    public Octree(Cuboid boundary)
    {
        this.boundary = boundary;
        this.divided = false;
    }

    //Push a point into the tree structure
    public bool AddPoint(Point pointToAdd) {
        //If the point isn't in this boundry of this node then return
        if (!boundary.Contains(pointToAdd)) {
            return false;
        }

        //If the point is in the boundry and the node has no value then assign it. 
        //Otherwise, subdivide if needed.
        if (this.point == null) {
            this.point = pointToAdd;
            return true;
        } else if (!divided) {
            Subdivide();
        }

        //Attempt to assign the point to the children node, returning when one has taken the value
        //This will recursively divide until the point is placed.
        for (int i = 0; i < children.Length; i++) {
            if (children[i].AddPoint(pointToAdd)) return true;
        }

        return false;
    }

    //Divide the current Octree node into eight child nodes
    private void Subdivide()
    {
        this.children = new Octree[8];
        
        for (int i = 0; i < children.Length; i++) {
            //Using the index i we create a 3 bit counting system
            int xDir = i % 2;
            int yDir = (i / 2) % 2;
            int zDir = (i / 4) % 2;

            //Convert the bits to -1 or 1
            xDir = xDir * 2 - 1;
            yDir = yDir * 2 - 1;
            zDir = zDir * 2 - 1;

            //Calculate the centre of the child Octree
            float xCentre = boundary.centre.x + xDir * boundary.width / 2;
            float yCentre = boundary.centre.y + yDir * boundary.height / 2;
            float zCentre = boundary.centre.z + zDir * boundary.depth / 2;

            Vector3 childCentre = new Vector3(xCentre, yCentre, zCentre);

            //Define a child's boundary
            Cuboid childBoundary = new Cuboid(childCentre, boundary.width / 2, 
                                                    boundary.height / 2, boundary.depth / 2);

            //Make a new Octree child with boundary specified by these numbers
            children[i] = new Octree(childBoundary);
        }

        //Set the current node to divided
        divided = true;
    }

    //Search the Octree for points within a given sphere and return the indices of those points
    public List<int> QuerySphere(Vector3 sphereCentre, float sphereRadius, List<int> listOfIndices = null)
    {
        //If no list is passed then make one 
        if (listOfIndices == null) {
            listOfIndices = new List<int>();
        }

        //If the boundary and sphere do not intersect then retun
        if (!boundary.IntersectsSphere(sphereCentre, sphereRadius)) {
            return listOfIndices;
        } else {
            //If there is a point stored here then check if it is in the sphere
            if (point != null) {
                float deltaX = point.x - sphereCentre.x;
                float deltaY = point.y - sphereCentre.y;
                float deltaZ = point.z - sphereCentre.z;
                float sqrDistToCentre = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;

                if (sqrDistToCentre < sphereRadius * sphereRadius) {
                    listOfIndices.Add(point.boidIndex);
                }
            }

            //If this Octree has children then check them
            if (divided) {
                for (int i = 0; i < children.Length; i++) {
                    //The list is directly added to in these iterations and so we don't worry about its return
                    children[i].QuerySphere(sphereCentre, sphereRadius, listOfIndices);
                }
            }
        }

        return listOfIndices;
    }
}
