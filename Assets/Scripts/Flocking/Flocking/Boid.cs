using UnityEngine;

public class Boid : MonoBehaviour
{
    //Components of the boid
    private Rigidbody rb;

    //Characteristics of the boid
    [Space(10)]
    [Header("Boid Characteristics")]
    [Space(5)]
    public float desiredSpeed;
    public float maxSpeed;
    public float accelerationStrength;
    public float steeringStrength;
    [Space(5)]
    public float alignmentPriority;
    public float cohesionPriority;
    public float separationPriority;

    private bool wantToClimb = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //We are using physics and so fixed update is more appropriate
    void FixedUpdate()
    {
        //Randomly decide to climb to avoid flat murmurations
        wantToClimb = (Random.Range(0f, 1f) < 0.3f) ? true : false;
        if (wantToClimb) {
            rb.AddForce(Vector3.up * accelerationStrength);
        }
    }

    //Given an array of nearby boids, move to follow three constrints
    public void UpdateBehaviour(GameObject[] nearbyBoids)
    {
        //If there are no nearby boids then return
        if (nearbyBoids.Length == 0) return;

        ////Calculate properties of the boids within view radius

        Vector3 boidsAveragePosition = new Vector3(0, 0, 0);
        Vector3 boidsAverageHeading = new Vector3(0, 0, 0);
        Vector3 separationSum = new Vector3(0, 0, 0);

        //Calculate the local flock variables
        foreach (var other in nearbyBoids) {
            //Ignore the boid in question
            if (other.transform.root != transform) {
                //Sum the positions and headings of the local boids as well as 
                // the vectors pointing from those boids to this one 
                boidsAveragePosition += other.transform.position;
                boidsAverageHeading += other.transform.forward;

                //The separation should be inversely proportional to distance
                Vector3 otherSeparationVector = transform.position - other.transform.position;
                //Save the distance
                float otherSeparationMagnitude = otherSeparationVector.magnitude;
                //Convert the separation vector to a unit vector
                otherSeparationVector.Normalize();
                //Multiply by the reciprocal of the distance
                separationSum += otherSeparationVector * (1 / otherSeparationMagnitude);
            }
        }

        //If there are boids close to this object then act on that information
        if (nearbyBoids.Length > 1) {
            //Divide by number of boids in view (-1 to avoid counting self)
            boidsAveragePosition /= nearbyBoids.Length - 1;
            boidsAverageHeading /= nearbyBoids.Length - 1;
            //We dont need to average the separations as we want larger contributions from closer boids
    

            ////We now need to generate a steering direction accumulated through the three defined rules

            //Alignment
            //"Steer towards the average heading of local flockmates"
            rb.AddForce(boidsAverageHeading * steeringStrength * alignmentPriority);

            //Cohesion
            //"Steer to move towards the average position of local flockmates"
            rb.AddForce((boidsAveragePosition - transform.position) * steeringStrength * cohesionPriority);

            //Separation
            rb.AddForce(separationSum * steeringStrength * separationPriority);
        }


        ////Speed up if you are not at the desired speed or slow down if going to fast
        if (rb.linearVelocity.magnitude < desiredSpeed) {
            rb.AddForce(transform.forward * accelerationStrength);
        } else {
            rb.AddForce(transform.forward * -accelerationStrength / 2);
        }
        
        //If you exceed max speed then just set the velocity to the max
        if (rb.linearVelocity.magnitude > maxSpeed) {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }


        ////Rotate to face your movement direction
        if (rb.linearVelocity != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity, transform.up);        
        }
    }
}
