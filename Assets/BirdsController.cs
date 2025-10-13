// Plik: BirdController.cs
using UnityEngine;
using System.Collections.Generic;

public class BirdController : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 acceleration;
    private Birds manager;

    private static List<BirdController> allBoids = new List<BirdController>();
    public static int BoidsCount => allBoids.Count;
    public static BirdController GetBoid(int index) => allBoids[index];

    public void Initialize(Birds manager)
    {
        this.manager = manager;
        velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * manager.maxSpeed;
    }

    void OnEnable() { if (!allBoids.Contains(this)) allBoids.Add(this); }
    void OnDisable() { allBoids.Remove(this); }

    void Update()
    {
        if (manager == null) return;

        acceleration = Vector3.zero;

        ApplyAllFlockingRules();
        UpdateMovement();
        ApplyBoundaryRules();
    }

    void ApplyAllFlockingRules()
    {
        Vector3 separationSum = Vector3.zero;
        Vector3 alignmentSum = Vector3.zero;
        Vector3 cohesionSum = Vector3.zero;
        int separationCount = 0;
        int perceptionCount = 0;

        foreach (BirdController other in allBoids)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);

            if (dist > 0 && dist < manager.perceptionRadius)
            {
                // Regu³y dzia³aj¹ce w zasiêgu percepcji
                alignmentSum += other.velocity;
                cohesionSum += other.transform.position;
                perceptionCount++;

                // Regu³a separacji dzia³a w mniejszym, wewnêtrznym zasiêgu
                if (dist < manager.separationRadius)
                {
                    Vector3 diff = transform.position - other.transform.position;
                    // Zmiana: Si³a odwrotnie proporcjonalna do odleg³oœci, a nie jej kwadratu. Jest to bardziej stabilne.
                    diff /= dist;
                    separationSum += diff;
                    separationCount++;
                }
            }
        }

        Vector3 totalForce = Vector3.zero;

        if (separationCount > 0)
        {
            Vector3 force = CalculateSteer((separationSum / separationCount));
            totalForce += force * manager.separationWeight;
        }

        if (perceptionCount > 0)
        {
            Vector3 alignmentForce = CalculateSteer(alignmentSum / perceptionCount);
            totalForce += alignmentForce * manager.alignmentWeight;

            Vector3 cohesionCenter = cohesionSum / perceptionCount;
            Vector3 desired = cohesionCenter - transform.position;
            Vector3 cohesionForce = CalculateSteer(desired);
            totalForce += cohesionForce * manager.cohesionWeight;
        }

        acceleration = totalForce;
    }

    void UpdateMovement()
    {
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, manager.maxSpeed);
        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = velocity.normalized;
        }
    }

    void ApplyBoundaryRules()
    {
        Vector3 pos = transform.position;
        Vector3 bounds = manager.spawnBounds / 2;
        if (pos.x > bounds.x) pos.x = -bounds.x; if (pos.x < -bounds.x) pos.x = bounds.x;
        if (pos.y > bounds.y) pos.y = -bounds.y; if (pos.y < -bounds.y) pos.y = bounds.y;
        if (pos.z > bounds.z) pos.z = -bounds.z; if (pos.z < -bounds.z) pos.z = bounds.z;
        transform.position = pos;
    }

    Vector3 CalculateSteer(Vector3 desiredDirection)
    {
        Vector3 steer = desiredDirection.normalized * manager.maxSpeed - velocity;
        return Vector3.ClampMagnitude(steer, manager.maxForce);
    }
}