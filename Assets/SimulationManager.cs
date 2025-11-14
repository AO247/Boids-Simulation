using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set;}


    public GameObject preyPrefab;
    public GameObject predatorPrefab;
    public List<GameObject> preys = new List<GameObject>();
    public List<GameObject> predators = new List<GameObject>();


    [Header("Prey Settings")]
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
        for (int i = 0; i < 10; i++)
        {
            Vector3 preyPosition = new Vector3(Random.Range(-10f, 10f), 3.0f, Random.Range(-10f, 10f));
            GameObject prey = Instantiate(preyPrefab, preyPosition, Quaternion.identity);
            preys.Add(prey);
        }
        for (int i = 0; i < 3; i++)
        {
            Vector3 predatorPosition = new Vector3(Random.Range(15f, 20f), 3.0f, Random.Range(15f, 20f));
            GameObject predator = Instantiate(predatorPrefab, predatorPosition, Quaternion.identity);
            predators.Add(predator);
        }
    }

    void Update()
    {

    }
}
