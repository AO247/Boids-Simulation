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

    [Header("Predator Settings")]
    public float predatorMaxSpeed = 4.0f;
    public float predatorMaxForce = 0.7f;
    public float preyDetectionRadius = 7.0f;
    public float predatorSeperationRadius = 1.0f;
    public float predatorAllignmentRadius = 2.0f;
    public float predatorCohesionRadius = 3.0f;
    public float predatorSeperationWeight = 5.0f;
    public float predatorAlignmentWeight = 3.0f;
    public float predatorCohesionWeight = 2.0f;

    public float chaseWeight = 5.0f;




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
            Vector3 preyPosition = new Vector3(Random.Range(-10f, 10f), 2.0f, Random.Range(-10f, 10f));
            GameObject prey = Instantiate(preyPrefab, preyPosition, Quaternion.identity);
            preys.Add(prey);
        }
        for (int i = 0; i < 3; i++)
        {
            Vector3 predatorPosition = new Vector3(Random.Range(15f, 20f), 2.0f, Random.Range(15f, 20f));
            GameObject predator = Instantiate(predatorPrefab, predatorPosition, Quaternion.identity);
            predators.Add(predator);
        }
    }

    void Update()
    {

    }
}
