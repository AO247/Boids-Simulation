using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set;}


    [SerializeField] GameObject preyPrefab;
    [SerializeField] GameObject predatorPrefab;
    public List<GameObject> preys = new List<GameObject>();
    public List<GameObject> predators = new List<GameObject>();
    [SerializeField] int preyCount = 50;
    [SerializeField] int predatorCount = 5;
    public float updateInterval = 0.1f;
    public float rotationSmoothSpeed = 5.0f;

    [Header("★★★ PREY SETTINGS ★★★")]
    public float seperationRadius = 2.0f;
    public float alignmentRadius = 3.0f;
    public float cohesionRadius = 4.0f;
    public float predatorDetectionRadius = 5.0f;
    public float maxForce = 0.5f;
    public float maxSpeed = 3.0f;
    public float seperationWeight = 6.0f;
    public float alignmentWeight = 4.0f;
    public float cohesionWeight = 3.0f;
    public float predatorAvoidanceWeight = 2.0f;
    public float randomMovementWeight = 0.5f;
    public float fatigueRecoveryRate = 0.02f;
    public float fatigueIncreaseRate = 0.05f;
    public float wanderRadius = 1.5f;
    public float wanderDistance = 2.0f;
    public float wanderJitter = 80.0f;
    public float wanderWeight = 1.0f;

    [Header("★★★ PREDATOR SETTINGS ★★★")]
    public float predatorMaxSpeed = 6.0f;
    public float predatorMaxForce = 10.0f;
    public float predatorFriction = 0.9f;

    [Header("Predator Pack Settings")]
    public float predatorSeperationRadius = 5.0f;
    public float predatorAlignmentRadius = 10.0f;
    public float predatorCohesionRadius = 10.0f;
    public float predatorSeperationWeight = 2.5f;
    public float predatorAlignmentWeight = 1.0f;
    public float predatorCohesionWeight = 1.0f;

    [Header("Predator Hunting Settings")]
    public float preyDetectionRadius = 50.0f;
    public float chaseWeight = 2.0f;
    public float eatDistance = 1.0f;
    public float hitDistance = 3.0f;
    public float eatingCooldown = 2.0f;
    public float hitCooldown = 3.0f;
    public float damagePerHit = 0.2f;

    [Header("★★★ ISLAND BOUNDARY SETTINGS ★★★")]
    public bool enableBoundary = true;
    public enum BoundaryShape { Circle, Box }
    public BoundaryShape shape = BoundaryShape.Circle;

    [Header("Circle Boundary")]
    public float boundaryRadius = 100f;

    [Header("Box Boundary")]
    public Vector2 boundarySize = new Vector2(200, 200);

    [Header("Avoidance Force")]
    public float boundaryMargin = 15f;
    public float boundaryAvoidanceWeight = 4.0f;
    public float boundaryForceMultiplier = 50.0f;





    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < preyCount; i++)
        {
            Vector3 preyPosition = new Vector3(Random.Range(-10f, 10f), 10.0f, Random.Range(-10f, 10f));
            GameObject prey = Instantiate(preyPrefab, preyPosition, Quaternion.identity);
            preys.Add(prey);
        }
        for (int i = 0; i < predatorCount; i++)
        {
            Vector3 predatorPosition = new Vector3(Random.Range(15f, 20f), 10.0f, Random.Range(15f, 20f));
            GameObject predator = Instantiate(predatorPrefab, predatorPosition, Quaternion.identity);
            predators.Add(predator);
        }
    }

    void Update()
    {

    }
}
