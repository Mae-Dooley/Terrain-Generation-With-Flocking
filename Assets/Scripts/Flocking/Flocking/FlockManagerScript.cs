using UnityEngine;
using System.Collections.Generic;

public class FlockManagerScript : MonoBehaviour
{
    [Space(10)]
    [Header("Prefabs To Instantiate")]
    [Space(5)]
    //The boid object to be used in the flock
    public GameObject boidPrefab;

    [Space(10)]
    [Header("Flock bounding box")]
    [Space(5)]
    [SerializeField]
    private float xBound;
    [SerializeField]
    private float yBound;
    [SerializeField]
    private float zBound;
    private float yOffset;

    [Space(10)]
    [Header("Flock characteristics")]
    [Space(5)]
    [SerializeField]
    private int numBoids;
    [SerializeField]
    private float boidSightRadius; //Some birds have peripheral vision of over 360 degrees!
    private GameObject[] boids;
    [SerializeField]
    private float horizontalBoundsForce;
    [SerializeField]
    private float verticalBoundsForce;
    
    [Space(10)]
    [Header("Meta")]
    [Space(5)]
    public LayerMask layerMask; //So that boids only detect other boids when using OverlapSphere

    //Octree variables
    private Cuboid boundary;
    private Octree octree;
    [Space(10)]
    [Header("Octree for collision detection?")]
    [Space(5)]
    public bool useOctree;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Define the boundary that will be used for the Octree
        boundary = new Cuboid(transform.position, xBound * 1.5f, yBound * 1.5f, zBound * 1.5f);

        //Define an array of all the boids
        boids = new GameObject[numBoids];

        yOffset = yBound + transform.position.y;
        
        for (int i = 0; i < numBoids; i++) {
            //Get a random position in the flock bounds
            Vector3 startingPosition = RandomVector(-xBound, xBound, -yBound + yOffset, 
                                                        yBound + yOffset, -zBound, zBound);

            //Create the boid at that position
            boids[i] = Instantiate(boidPrefab, startingPosition, Quaternion.identity);

            //Get a random starting direction and align the boid to it
            Vector3 startingDirection = RandomVector(0, 360, 0, 360, 0, 360);
            boids[i].transform.Rotate(startingDirection);

            //Give the boid a starting forceful push
            boids[i].GetComponent<Rigidbody>().AddForce(boids[i].transform.forward * 10);
        }
    }    

    // Update is called once per frame
    void FixedUpdate()
    {
        ////Construct the Octree
        octree = new Octree(boundary);

        //Add the boids (represented as points) to the tree
        for (int i = 0; i < numBoids; i++) {
            float boidX = boids[i].transform.position.x;
            float boidY = boids[i].transform.position.y;
            float boidZ = boids[i].transform.position.z;
            
            Point boidPoint = new Point(boidX, boidY, boidZ, i);

            octree.AddPoint(boidPoint);
        }

        
        ////Loop through each boid updating their behaviour
        foreach (GameObject boid in boids) {
            //If a boid goes out of bounds then we push it back in bounds

            Vector3 boidPosition = boid.transform.position;
            float xForce = 0;
            float yForce = 0;
            float zForce = 0;

            if (boidPosition.x < -xBound) {
                xForce = -xBound - boidPosition.x;
            } else if (boidPosition.x > xBound) {
                xForce = xBound - boidPosition.x;
            }
            //We make the bottom of the flocking area the position of the flock manager
            if (boidPosition.y < -yBound + yOffset) {
                yForce = -yBound - boidPosition.y + yOffset;
            } else if (boidPosition.y > yBound + yOffset) {
                yForce = yBound - boidPosition.y + yOffset;
            }
            if (boidPosition.z < -zBound) {
                zForce = -zBound - boidPosition.z;
            } else if(boidPosition.z > zBound) {
                zForce = zBound - boidPosition.z;
            }
            //Apply the forces
            if (xForce != 0 || yForce != 0 || zForce != 0) {
                Vector3 forceToApply = new Vector3(xForce, yForce, zForce);
                forceToApply.x *= horizontalBoundsForce;
                forceToApply.z *= horizontalBoundsForce;
                forceToApply.y *= verticalBoundsForce;
                //We only get the rigidbody now to minimise calls of GetComponent
                boid.GetComponent<Rigidbody>().AddForce(forceToApply);
            }


            //Get the boids near to this one and update the behaviour
            GameObject[] nearbyBoids;
            
            if (!useOctree) {
                //Get the boids that are nearby to this one using Unity OverlapSphere
                Collider[] nearbyBoidsColliders = Physics.OverlapSphere(boid.transform.position, 
                                                                            boidSightRadius, layerMask);
                //Create a list of the game objects found and populate the nearbyBoids with them
                nearbyBoids = new GameObject[nearbyBoidsColliders.Length];
                for (int i = 0; i < nearbyBoids.Length; i++) {
                    nearbyBoids[i] = nearbyBoidsColliders[i].gameObject;
                } 
            } else {
                //Get the boids that are nearby to this one by Querying the Octree
                List<int> nearbyBoidIndices = octree.QuerySphere(boid.transform.position, boidSightRadius);
                nearbyBoids = new GameObject[nearbyBoidIndices.Count];
                //Populate the nearbyBoids with the found objects
                for (int i = 0; i < nearbyBoidIndices.Count; i++) {
                    nearbyBoids[i] = boids[nearbyBoidIndices[i]];
                }
            }

            //Update the boids, giving the nearby boids GameObjects
            boid.GetComponent<Boid>().UpdateBehaviour(nearbyBoids);

        }
    }

    void Update()
    {
        //Slow down the simulation on pressing space for debugging
        if (Input.GetKey(KeyCode.Space)) {
            if (Time.timeScale == 1f) {
                Time.timeScale = 0.01f;
            } else {
                Time.timeScale = 1f;
            }
        }
    }

    //Generate a random vector within the six given bounds
    private Vector3 RandomVector(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
    {
        float xCoord = Random.Range(minX, maxX);
        float yCoord = Random.Range(minY, maxY);
        float zCoord = Random.Range(minZ, maxZ);

        Vector3 randomVector = new Vector3(xCoord, yCoord, zCoord);

        return randomVector;
    }
}
