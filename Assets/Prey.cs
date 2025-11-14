using UnityEngine;

public class Prey : MonoBehaviour
{
    private SimulationManager simManager;
    [SerializeField] ParticleSystem bleeding;
    public Vector3 velocity;
    private Vector3 acceleration;
    public bool isFleeing = false;
    public float fatigue = 0.0f;
    public float health = 1.0f;
    public float meat = 1.0f;
    [SerializeField] float friction = 0.9f;
    public bool isDead => health <= 0.0f;
    public float velocityMagnitude => velocity.magnitude;
    private float wanderAngle;
    private bool rolledOnDeath = false;

    

    void Start()
    {
        simManager = SimulationManager.Instance;

        velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

        wanderAngle = Random.Range(0f, 2f * Mathf.PI);

        simManager.preys.Add(this.gameObject);
        KeepOnGround();

    }

    void OnDestroy()
    {
        if (simManager != null)
        {
            simManager.preys.Remove(this.gameObject);
        }
    }

    // === G³ówna Pêtla Logiki ===
    void Update()
    {
        if (isDead)
        {
            velocity *= 0.90f;
            transform.position += velocity * Time.deltaTime;
            if (!rolledOnDeath)
            {
                // zachowaj aktualn¹ rotacjê w Y, dodaj 90 stopni do Z
                Vector3 e = transform.eulerAngles;
                e.z += 90f;
                transform.rotation = Quaternion.Euler(e);
                rolledOnDeath = true;
            }
            return;
        }
        if(health < 1.0f )
        {
            if(!bleeding.isPlaying)
            {
                bleeding.Play();
            }
            var emission = bleeding.emission;
            emission.rateOverTime = (1.0f - health) * 10.0f;
        }
        CalculateForces();
        ApplyMovement();
        UpdateState();
        KeepOnGround();
    }


    private void CalculateForces()
    {
        acceleration = Vector3.zero;
        isFleeing = false;

        Vector3 fleeForce = CalculateFleeForce();
        if (fleeForce.sqrMagnitude > 0)
        {
            isFleeing = true;
            acceleration += fleeForce * simManager.predatorAvoidanceWeight;
        }

        acceleration += CalculateFlockingForces();
        acceleration += CalculateWanderForce() * simManager.wanderWeight;

        acceleration = Vector3.ClampMagnitude(acceleration, simManager.maxForce);
    }


    private void ApplyMovement()
    {
        acceleration.y = 0.0f;
        acceleration = Vector3.ClampMagnitude(acceleration, simManager.maxForce);
        velocity += acceleration * Time.deltaTime;

        float currentMaxSpeed = isFleeing ? simManager.maxSpeed : (simManager.maxSpeed / 2f);
        currentMaxSpeed *= (1.05f - fatigue/1.5f);
        currentMaxSpeed *= (0.05f + health);

        velocity = Vector3.ClampMagnitude(velocity, currentMaxSpeed);
        transform.position += velocity * Time.deltaTime;

        velocity *= (1.0f - Time.deltaTime * friction);
        velocity.y += -9.81f * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0.0f, velocity.z).normalized);
        }
    }


    private void UpdateState()
    {
        if (isFleeing)
        {
            fatigue += simManager.fatigueIncreaseRate * Time.deltaTime;
        }
        else
        {
            fatigue -= simManager.fatigueRecoveryRate * Time.deltaTime;
        }
        fatigue = Mathf.Clamp01(fatigue);
    }




    private Vector3 CalculateFlockingForces()
    {
        Vector3 separationForce = Vector3.zero;
        Vector3 alignmentForce = Vector3.zero;
        Vector3 cohesionForce = Vector3.zero;

        int separationCount = 0;
        int alignmentAndCohesionCount = 0;

        foreach (var other in simManager.preys)
        {
            if (other == this.gameObject) continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);


            if (distance > 0 && distance < simManager.seperationRadius)
            {
                Vector3 diff = (transform.position - other.transform.position).normalized;
                separationForce += diff / distance;
                separationCount++;
            }

            if (distance > 0 && distance < simManager.alignmentRadius)
            {
                alignmentForce += other.GetComponent<Prey>().velocity;
                cohesionForce += other.transform.position;
                alignmentAndCohesionCount++;
            }
        }

        if (separationCount > 0) separationForce /= separationCount;
        if (alignmentAndCohesionCount > 0)
        {
            alignmentForce /= alignmentAndCohesionCount;
            cohesionForce /= alignmentAndCohesionCount;
            cohesionForce = cohesionForce - transform.position;
        }

        return (separationForce * simManager.seperationWeight) +
               (alignmentForce * simManager.alignmentWeight) +
               (cohesionForce * simManager.cohesionWeight);
    }


    private Vector3 CalculateFleeForce()
    {
        Vector3 fleeVector = Vector3.zero;
        int predatorsNearby = 0;

        foreach (var predator in simManager.predators)
        {
            float distance = Vector3.Distance(transform.position, predator.transform.position);
            if (distance < simManager.predatorDetectionRadius)
            {
                fleeVector += (transform.position - predator.transform.position).normalized;
                predatorsNearby++;
            }
        }

        if (predatorsNearby > 0)
        {
            fleeVector.y = 0.0f;
            fleeVector /= predatorsNearby;
            return Steer(fleeVector);
        }

        return Vector3.zero;
    }


    private Vector3 CalculateWanderForce()
    {
        Vector3 circleCenter = transform.position + transform.forward * simManager.wanderDistance;

        float angleChange = (Random.Range(-1f, 1f) * simManager.wanderJitter) * Time.deltaTime;
        wanderAngle += angleChange;

        Vector3 displacement = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle)) * simManager.wanderRadius;

        Vector3 wanderTarget = circleCenter + displacement;

        return Steer(wanderTarget - transform.position);
    }

    private Vector3 Steer(Vector3 desiredDirection)
    {
        desiredDirection.y = 0.0f;
        Vector3 steer = desiredDirection.normalized * simManager.maxSpeed - velocity;
        return Vector3.ClampMagnitude(steer, simManager.maxForce);
    }

    void KeepOnGround()
    {
        if (Physics.Raycast(transform.position + (Vector3.up * 4.0f), Vector3.down, out RaycastHit hit, 5.0f))
        {
            transform.position = hit.point;
        }
    }
}