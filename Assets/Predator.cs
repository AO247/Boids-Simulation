using UnityEngine;

public class Predator : MonoBehaviour
{
    bool isGrounded = false;
    [Header("Movement Settings")]
    [SerializeField] float maxSpeed = 2.0f;
    [SerializeField] float friction = 0.9f;

    public Vector3 velocity;
    public float fatigue = 0.0f;

    float eatingCooldown = 0.0f;

    SimulationManager simulationManager;

    void Start()
    {
        simulationManager = SimulationManager.Instance;
    }


    // Update is called once per frame
    void Update()
    {
        if(eatingCooldown > 0.0f)
        {
            eatingCooldown -= Time.deltaTime;
            return;
        }
        KeepOnGround();
        Vector3 acceleration = Vector3.zero;
        acceleration += ChasePrey();
        acceleration = Vector3.ClampMagnitude(acceleration, simulationManager.predatorMaxForce);
        acceleration.y = 0.0f;
        velocity += acceleration * Time.deltaTime;
        //velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        velocity *= friction;
        KillPrey();

        transform.rotation = Quaternion.LookRotation(velocity.normalized);
    }

    Vector3 ChasePrey()
    {
        Vector3 steer = Vector3.zero;
        GameObject closestPrey = null;
        float closestDistance = float.MaxValue;
        foreach (GameObject prey in simulationManager.preys)
        {
            float distance = Vector3.Distance(transform.position, prey.transform.position);
            if (distance < simulationManager.preyDetectionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestPrey = prey;
            }
        }
        if (closestPrey != null)
        {
            Vector3 desired = (closestPrey.transform.position - transform.position).normalized * maxSpeed;
            steer = desired - velocity;
        }
        return steer * simulationManager.chaseWeight;
    }
    void KillPrey()
    {
        GameObject closestPrey = null;
        float closestDistance = float.MaxValue;
        foreach (GameObject prey in simulationManager.preys)
        {
            float distance = Vector3.Distance(transform.position, prey.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPrey = prey;
            }
        }
        if (closestPrey != null && closestDistance < 0.5f)
        {
            simulationManager.preys.Remove(closestPrey);
            Destroy(closestPrey);
            eatingCooldown = 3.0f;
            Debug.Log("Prey eaten!");
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
        if (!isGrounded)
        {
            transform.position += new Vector3(0.0f, -9.81f, 0.0f) * Time.deltaTime;
        }
    }
}
