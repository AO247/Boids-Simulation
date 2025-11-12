using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Prey : MonoBehaviour
{
    bool isGrounded = false;
    [Header("Movement Settings")]
    [SerializeField] float maxSpeed = 2.0f;
    [SerializeField] float friction = 0.9f;
    
    public Vector3 velocity;
    public float fatigue = 0.0f;
    public float health = 1.0f;

    SimulationManager simulationManager;

    void Start()
    {
        simulationManager = SimulationManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        KeepOnGround();

        Vector3 acceleration = Vector3.zero;
        acceleration += Seperation();
        acceleration += Alignment();
        acceleration += Cohesion();
        acceleration += Flee();
        acceleration += new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)) * simulationManager.randomMovementWeight;
        acceleration = Vector3.ClampMagnitude(acceleration, simulationManager.maxForce);
        acceleration.y = 0.0f;
        velocity += acceleration * Time.deltaTime;
        //velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
        velocity *= friction;

        transform.rotation = Quaternion.LookRotation(velocity.normalized);
    }



    Vector3 Seperation()
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        foreach (GameObject other in simulationManager.preys)
        {
            if (other != this.gameObject)
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance < simulationManager.seperationRadius)
                {
                    Vector3 diff = transform.position - other.transform.position;
                    diff.Normalize();
                    diff /= distance;
                    steer += diff;
                    count++;
                }
            }
        }
        if (count > 0)
        {
            steer /= count;
        }
        if (steer.magnitude > 0)
        {
            steer.Normalize();
            steer *= maxSpeed;
            steer -= velocity;
            steer = Vector3.ClampMagnitude(steer, simulationManager.maxForce);
        }
        return steer * simulationManager.seperationWeight;
    }

    Vector3 Alignment()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (GameObject other in simulationManager.preys)
        {
            if (other != this.gameObject)
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance < simulationManager.alignmentRadius)
                {
                    Prey otherBoid = other.GetComponent<Prey>();
                    sum += otherBoid.velocity;
                    count++;
                }
            }
        }
        if (count > 0)
        {
            sum /= count;
            sum.Normalize();
            sum *= maxSpeed;
            Vector3 steer = sum - velocity;
            steer = Vector3.ClampMagnitude(steer, simulationManager.maxForce);
            return steer * simulationManager.alignmentWeight;
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector3 Cohesion()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (GameObject other in simulationManager.preys)
        {
            if (other != this.gameObject)
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance < simulationManager.cohesionRadius)
                {
                    sum += other.transform.position;
                    count++;
                }
            }
        }
        if (count > 0)
        {
            sum /= count;
            Vector3 desired = sum - transform.position;
            desired.Normalize();
            desired *= maxSpeed;
            Vector3 steer = desired - velocity;
            steer = Vector3.ClampMagnitude(steer, simulationManager.maxForce);
            return steer * simulationManager.cohesionWeight;
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector3 Flee()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (GameObject predator in simulationManager.predators)
        {
            float distance = Vector3.Distance(transform.position, predator.transform.position);
            if (distance < simulationManager.predatorDetectionRadius)
            {
                Vector3 diff = transform.position - predator.transform.position;
                diff.Normalize();
                diff /= distance;
                sum += diff;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            sum.Normalize();
            sum *= maxSpeed;
            Vector3 steer = sum - velocity;
            steer = Vector3.ClampMagnitude(steer, simulationManager.maxForce);
            return steer * simulationManager.predatorAvoidanceWeight;
        }
        else
        {
            return Vector3.zero;
        }
    }






    void KeepOnGround()
    {
        Ray ray = new Ray(transform.position, new Vector3(0.0f, -1.0f, 0.0f) * 0.1f);
        Debug.DrawRay(ray.origin, ray.direction, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.1f))
        {
            isGrounded = true;
            if (hitInfo.distance > 0.05f)
            {
                transform.position += new Vector3(0.0f, -hitInfo.distance + 0.05f, 0.0f);
            }
        }
        else
        {
            isGrounded = false;
        }
        if(!isGrounded)
        {
            transform.position += new Vector3(0.0f, -9.81f, 0.0f) * Time.deltaTime;
        }
    }

 
}
