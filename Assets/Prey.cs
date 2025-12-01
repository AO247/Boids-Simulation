using UnityEngine;
using static SimulationManager;

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
    public float velocityMagnitude = 0;
    private float wanderAngle;
    private bool rolledOnDeath = false;
    private float time = 0.0f;
    private LayerMask obstacleLayer;


    private Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();
        simManager = SimulationManager.Instance;

        velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

        wanderAngle = Random.Range(0f, 2f * Mathf.PI);

        simManager.preys.Add(this.gameObject);
        obstacleLayer = LayerMask.GetMask("Obstacle");

    }

    void OnDestroy()
    {
        if (simManager != null)
        {
            simManager.preys.Remove(this.gameObject);
        }
    }

    void Update()
    {
        time += Time.deltaTime;
        velocityMagnitude = velocity.magnitude;
        if (isDead)
        {

            velocity *= 0.90f;
            transform.position += velocity * Time.deltaTime;
            if (!rolledOnDeath)
            {
                Vector3 e = transform.eulerAngles;
                e.z += 90f;
                //transform.rotation = Quaternion.Euler(e);
                animator.SetBool("isDead", true);

                rolledOnDeath = true;
            }
            return;
        }
        if(health < 1.0f )
        {
            if (!bleeding.isPlaying)
            {
                animator.SetBool("isHurt", true);
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

        Vector3 obstacleAvoidanceForce = CalculateObstacleAvoidanceForce();
        acceleration += obstacleAvoidanceForce * simManager.obstacleAvoidanceWeight;

        acceleration += CalculateFlockingForces();
        acceleration += CalculateWanderForce() * simManager.wanderWeight;



        Vector3 boundaryForce = Vector3.zero;
        if (simManager.enableBoundary)
        {
            boundaryForce = CalculateBoundaryForce();
        }

        if (boundaryForce.sqrMagnitude > 0)
        {
            acceleration += boundaryForce * simManager.boundaryAvoidanceWeight;
        }


        Vector3 fleeForce = CalculateFleeForce();
        if (fleeForce.sqrMagnitude > 0)
        {
            isFleeing = true;
            acceleration += fleeForce * simManager.predatorAvoidanceWeight;
            acceleration.y = 0.0f;
            acceleration = acceleration.normalized * simManager.maxForce;
        }

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
        //velocity.y += -9.81f * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.001f)
        {
            Vector3 flatVel = new Vector3(velocity.x, 0.0f, velocity.z);
            if (flatVel.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVel.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, simManager.rotationSmoothSpeed * Time.deltaTime);
            }
        }
    }


    private void UpdateState()
    {
        if (isFleeing)
        {
            // Zamiast: fatigue += simManager.fatigueIncreaseRate * Time.deltaTime;
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

        return (separationForce * simManager.seperationWeight * 10) +
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
            if (distance > 0 && distance < simManager.predatorDetectionRadius)
            {
                fleeVector += (transform.position - predator.transform.position).normalized / (distance / 2.0f);

                predatorsNearby++;
            }
        }

        if (predatorsNearby > 0)
        {

            fleeVector.y = 0.0f;
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

    private Vector3 CalculateBoundaryForce()
    {
        Vector3 desiredDirection = Vector3.zero;
        Vector3 pos = transform.position;
        float strength = 0.0f;

        switch (simManager.shape)
        {
            case BoundaryShape.Circle:
                float distFromCenter = pos.magnitude;
                if (distFromCenter > simManager.boundaryRadius - simManager.boundaryMargin)
                {
                    desiredDirection = -pos.normalized;

                    strength = Mathf.InverseLerp(simManager.boundaryRadius - simManager.boundaryMargin, simManager.boundaryRadius, distFromCenter);
                }
                break;

            case BoundaryShape.Box:
                float halfWidth = simManager.boundarySize.x / 2f;
                float halfHeight = simManager.boundarySize.y / 2f;

                if (pos.x > halfWidth - simManager.boundaryMargin)
                {
                    desiredDirection.x = -1;
                    strength = Mathf.Max(strength, Mathf.InverseLerp(halfWidth - simManager.boundaryMargin, halfWidth, pos.x));
                }
                else if (pos.x < -halfWidth + simManager.boundaryMargin)
                {
                    desiredDirection.x = 1;
                    strength = Mathf.Max(strength, Mathf.InverseLerp(-halfWidth + simManager.boundaryMargin, -halfWidth, pos.x));
                }

                if (pos.z > halfHeight - simManager.boundaryMargin)
                {
                    desiredDirection.z = -1;
                    strength = Mathf.Max(strength, Mathf.InverseLerp(halfHeight - simManager.boundaryMargin, halfHeight, pos.z));
                }
                else if (pos.z < -halfHeight + simManager.boundaryMargin)
                {
                    desiredDirection.z = 1;
                    strength = Mathf.Max(strength, Mathf.InverseLerp(-halfHeight + simManager.boundaryMargin, -halfHeight, pos.z));
                }
                break;
        }

        if (strength > 0)
        {
            desiredDirection.y = 0;
            Vector3 steer = desiredDirection.normalized * strength * simManager.boundaryForceMultiplier - velocity;
            return Vector3.ClampMagnitude(steer, simManager.maxForce);
        }

        return Vector3.zero;
    }

    private Vector3 CalculateObstacleAvoidanceForce()
    {
        Vector3 avoidanceForce = Vector3.zero;
        Vector3 forward = transform.forward;
        Vector3 position = transform.position;
        float distance = simManager.obstacleAvoidanceDistance;

        // "W¹s" œrodkowy - prosto przed siebie
        if (Physics.Raycast(position, forward, distance, obstacleLayer))
        {
            // Jeœli coœ jest prosto przed nami, mocno skrêæ w bok
            avoidanceForce += transform.right * 2; // Skrêæ w prawo (mo¿na losowaæ)
        }

        // "W¹s" prawy
        Quaternion rightRot = Quaternion.AngleAxis(simManager.whiskerAngle, Vector3.up);
        if (Physics.Raycast(position, rightRot * forward, distance, obstacleLayer))
        {
            // Jeœli coœ jest po prawej, skrêæ w lewo
            avoidanceForce -= transform.right;
        }

        // "W¹s" lewy
        Quaternion leftRot = Quaternion.AngleAxis(-simManager.whiskerAngle, Vector3.up);
        if (Physics.Raycast(position, leftRot * forward, distance, obstacleLayer))
        {
            // Jeœli coœ jest po lewej, skrêæ w prawo
            avoidanceForce += transform.right;
        }

        // Rysuj promienie w edytorze dla ³atwiejszego debugowania
        Debug.DrawRay(position, forward * distance, Color.blue);
        Debug.DrawRay(position, rightRot * forward * distance, Color.blue);
        Debug.DrawRay(position, leftRot * forward * distance, Color.blue);

        return avoidanceForce.normalized;
    }


    void KeepOnGround()
    {
        if (Physics.Raycast(transform.position + (Vector3.up * 15.0f), Vector3.down, out RaycastHit hit, 60.0f, LayerMask.GetMask("Ground")))
        {
            transform.position = hit.point;
        }
    }
}