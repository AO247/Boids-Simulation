using UnityEngine;
using static SimulationManager;

public class Predator : MonoBehaviour
{
    public Vector3 velocity;
    private Vector3 acceleration;
    private bool isChasing = false;
    public float hunger = 0.5f;
    private SimulationManager simManager;
    private float wanderAngle;
    private GameObject currentTarget;
    private float eatingCooldownTimer;
    private float hitCooldownTimer;
    public float velocityMagnitude = 0;

    private Animator animator;
    private LayerMask obstacleLayer;
    public bool eating = false;
    private float time = 0.0f;
    void Start()
    {
        animator = GetComponent<Animator>();
        simManager = SimulationManager.Instance;
        velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        wanderAngle = Random.Range(0f, 2f * Mathf.PI);
        obstacleLayer = LayerMask.GetMask("Obstacle");
        simManager.predators.Add(this.gameObject);
    }

    void OnDestroy()
    {
        if (simManager != null)
        {
            simManager.predators.Remove(this.gameObject);
        }
    }

    void Update()
    {
        velocityMagnitude = velocity.magnitude;
        time += Time.deltaTime;
        if (eatingCooldownTimer > 0)
        {
            eatingCooldownTimer -= Time.deltaTime;
            velocity *= 0.95f;
            transform.position += velocity * Time.deltaTime;
            return;
        }

        if (time >= simManager.updateInterval)
        {
            time = 0.0f;
            CalculateForces();
        }
        UpdateState();
        CalculateForces();
        ApplyMovement();
        KeepOnGround();
    }

    private void CalculateForces()
    {
        acceleration = Vector3.zero;
        isChasing = false;

        Vector3 obstacleAvoidanceForce = CalculateObstacleAvoidanceForce();
        acceleration += obstacleAvoidanceForce * simManager.obstacleAvoidanceWeight;
        acceleration += CalculatePackForces();

        Vector3 boundaryForce = Vector3.zero;
        if (simManager.enableBoundary)
        {
            boundaryForce = CalculateBoundaryForce();
        }

        if (boundaryForce.sqrMagnitude > 0)
        {
            acceleration += boundaryForce * simManager.boundaryAvoidanceWeight;
        }

        Vector3 chaseForce = Vector3.zero;
        if (hunger > 0.1f)
        {
            chaseForce = CalculateChaseForce();
        }
        if (chaseForce.sqrMagnitude > 0)
        {
            isChasing = true;
            acceleration += chaseForce * simManager.chaseWeight;
            acceleration.y = 0.0f;
            acceleration = acceleration.normalized * simManager.predatorMaxForce;
        }
        else
        {
            acceleration += CalculateWanderForce();
        }

        acceleration = Vector3.ClampMagnitude(acceleration, simManager.predatorMaxForce);
    }

    private void ApplyMovement()
    {
        if (eating) return;
        animator.SetBool("isAttacking", false);
        animator.SetBool("isEating", false);
        velocity += acceleration * Time.deltaTime;

        float speedMultiplier = 1.0f + hunger * 0.5f;
        float currentMaxSpeed = isChasing ? simManager.predatorMaxSpeed : (simManager.predatorMaxSpeed / 2f);
        currentMaxSpeed *= speedMultiplier;

        velocity = Vector3.ClampMagnitude(velocity, currentMaxSpeed);
        transform.position += velocity * Time.deltaTime;

        velocity *= (1.0f - Time.deltaTime * simManager.predatorFriction);

        //if(eating == false)
        //    velocity.y += -9.81f * Time.deltaTime;

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
        hunger += Time.deltaTime * simManager.predatorHungerRate;
        hunger = Mathf.Clamp01(hunger);

        if (currentTarget != null)
        {
            if (currentTarget.GetComponent<Prey>().isDead)
            {
                Eat();
            }
            else
            {
                eating = false;
                TryToKill();
            }
        }
    }


    private Vector3 CalculateChaseForce()
    {
        float maxValue = float.NegativeInfinity;
        GameObject closestPrey = null;

        foreach (var prey in simManager.preys)
        {
            float distance = Vector3.Distance(transform.position, prey.transform.position);

            float closestDist = distance;
            float fatigue = prey.GetComponent<Prey>().fatigue;
            float heatlh = prey.GetComponent<Prey>().health;

            float score = (simManager.predatorDetectionRadius - closestDist) * (1.0f + fatigue/3.0f) * (1.0f + (1.0f - heatlh)/2.5f);
            if (score > maxValue)
            {
                closestPrey = prey;
                maxValue = score;
            }

        }

        currentTarget = closestPrey;

        if (currentTarget != null)
        {
            return Steer(currentTarget.transform.position - transform.position);
        }

        return Vector3.zero;
    }

    private void TryToKill()
    {
        if(hitCooldownTimer > 0.0f)
        {
            hitCooldownTimer -= Time.deltaTime;
            animator.SetBool("isAttacking", false);
            return;
        }
        if (Vector3.Distance(transform.position, currentTarget.transform.position) < simManager.hitDistance)
        {
            animator.SetBool("isAttacking", true);
            currentTarget.GetComponent<Prey>().health -= simManager.damagePerHit;
            if (currentTarget.GetComponent<Prey>().health <= 0.0f)
            {
                currentTarget.GetComponent<Prey>().health = 0.0f;
            }
            hitCooldownTimer = simManager.hitCooldown;
        }
    }
    private void Eat()
    {
        velocity *= 0.90f;
        velocity.y = 0.0f;
        if (eatingCooldownTimer > 0.0f)
        {
            eatingCooldownTimer -= Time.deltaTime;
            return;
        }

        if (Vector3.Distance(transform.position, currentTarget.transform.position) < simManager.eatDistance)
        {
            eating = true;
            animator.SetBool("isEating", true);
            currentTarget.GetComponent<Prey>().meat -= 0.1f;
            hunger = Mathf.Max(-0.3f, hunger - 0.2f);
            eatingCooldownTimer = simManager.eatingCooldown;
            if (currentTarget.GetComponent<Prey>().meat <= 0.0f)
            {
                eating = false;
                simManager.preys.Remove(currentTarget);
                Destroy(currentTarget);
                currentTarget = null;
                animator.SetBool("isEating", false);
            }
        }
        else
        {
            eating = false;
            animator.SetBool("isEating", false);

        }

    }

    private Vector3 CalculatePackForces()
    {
        Vector3 separationForce = Vector3.zero;
        Vector3 alignmentForce = Vector3.zero;
        Vector3 cohesionForce = Vector3.zero;

        int separationCount = 0;
        int alignmentAndCohesionCount = 0;

        foreach (var other in simManager.predators)
        {
            if (other == this.gameObject) continue;
            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (distance > 0 && distance < simManager.predatorSeperationRadius)
            {
                Vector3 diff = (transform.position - other.transform.position).normalized;
                separationForce += diff / distance;
                separationCount++;
            }
            if (distance > 0 && distance < simManager.predatorAlignmentRadius)
            {
                alignmentForce += other.GetComponent<Predator>().velocity;
                cohesionForce += other.transform.position;
                alignmentAndCohesionCount++;
            }
        }

        foreach (var prey in simManager.preys)
        {
            float distance = Vector3.Distance(transform.position, prey.transform.position);
            if (distance > 0 && distance < 3.0f)
            {
                Vector3 diff = (transform.position - prey.transform.position).normalized;
                separationForce += diff / distance;
                separationCount++;
            }
        }


        if (separationCount > 0) separationForce /= separationCount;
        if (alignmentAndCohesionCount > 0)
        {
            alignmentForce /= alignmentAndCohesionCount;
            cohesionForce = (cohesionForce / alignmentAndCohesionCount) - transform.position;
        }

        return (separationForce * simManager.predatorSeperationWeight * 10) +
               (alignmentForce * simManager.predatorAlignmentWeight) +
               (cohesionForce * simManager.predatorCohesionWeight);
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
        Vector3 steer = desiredDirection.normalized * simManager.predatorMaxSpeed - velocity;
        return Vector3.ClampMagnitude(steer, simManager.predatorMaxForce);
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
            desiredDirection.y = 0.0f;
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

        if (Physics.Raycast(position, forward, distance, obstacleLayer))
        {
            avoidanceForce += transform.right * 2;
        }

        Quaternion rightRot = Quaternion.AngleAxis(simManager.whiskerAngle, Vector3.up);
        if (Physics.Raycast(position, rightRot * forward, distance, obstacleLayer))
        {
            avoidanceForce -= transform.right;
        }

        Quaternion leftRot = Quaternion.AngleAxis(-simManager.whiskerAngle, Vector3.up);
        if (Physics.Raycast(position, leftRot * forward, distance, obstacleLayer))
        {
            avoidanceForce += transform.right;
        }

        Debug.DrawRay(position, forward * distance, Color.red);
        Debug.DrawRay(position, rightRot * forward * distance, Color.red);
        Debug.DrawRay(position, leftRot * forward * distance, Color.red);

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