using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

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

    [Header("★★★ OBSTACLE AVOIDANCE SETTINGS ★★★")]
    [Tooltip("Jak daleko przed siebie agenci mają 'patrzeć' w poszukiwaniu przeszkód.")]
    public float obstacleAvoidanceDistance = 10.0f;

    [Tooltip("Jak silna ma być siła omijania przeszkód.")]
    public float obstacleAvoidanceWeight = 5.0f;

    [Tooltip("Kąt, pod jakim 'wąsy' są rozstawione po bokach.")]
    public float whiskerAngle = 30.0f;


    [Header("★★★ SPAWN SETTINGS ★★★")]
    [Tooltip("Całkowita liczba ofiar do zespawnowania.")]
    [SerializeField] private int totalPreyCount = 50;
    [Tooltip("Całkowita liczba drapieżników do zespawnowania.")]
    [SerializeField] private int totalPredatorCount = 5;
    [Space]
    [Tooltip("Minimalna i maksymalna liczba ofiar w jednej grupie.")]
    [SerializeField] private Vector2Int preyGroupSize = new Vector2Int(5, 6);
    [Tooltip("Liczba drapieżników w jednej grupie.")]
    [SerializeField] private int predatorGroupSize = 3;
    [Space]
    [Tooltip("Promień, w którym będą pojawiać się grupy.")]
    [SerializeField] private float spawnRadius = 500f;
    [Tooltip("Jak blisko siebie mogą pojawić się osobniki w jednej grupie.")]
    [SerializeField] private float groupSpawnRadius = 5.0f;
    [Tooltip("Wysokość, na której pojawiają się agenci (powinni potem opaść na ziemię).")]
    [SerializeField] private float spawnHeight = 10.0f;


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


    [Header("★★★ SIMULATION SPEED ★★★")]
    [Range(0f, 10f)]
    public float simulationTimeScale = 1.0f;


    [Header("★★★ SIMULATION TIME DISPLAY ★★★")]
    [SerializeField] private TextMeshProUGUI simulationTimeText;
    private float totalSimulationTime = 0.0f;

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

    void Update()
    {
        // Aktualizuj globalny mnożnik czasu na podstawie ustawienia symulacji
        Time.timeScale = simulationTimeScale;
        totalSimulationTime += Time.deltaTime;

        // 3. Zaktualizuj tekst na ekranie
        UpdateSimulationTimeDisplay();
    }

    // --- NOWA FUNKCJA DO FORMATOWANIA I WYŚWIETLANIA CZASU ---
    private void UpdateSimulationTimeDisplay()
    {
        if (simulationTimeText == null) return;

        // Przelicz totalSimulationTime na godziny, minuty i sekundy
        float time = totalSimulationTime;

        int hours = (int)(time / 3600);
        time %= 3600;
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);

        // Użyj String.Format, aby stworzyć elegancki, czytelny tekst
        simulationTimeText.text = string.Format("Czas Symulacji: {0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
    void Start()
    {
        // Spawnowanie ofiar w grupach
        SpawnInGroups(preyPrefab, totalPreyCount, preyGroupSize.x, preyGroupSize.y, preys);

        // Spawnowanie drapieżników w grupach
        SpawnInGroups(predatorPrefab, totalPredatorCount, predatorGroupSize, predatorGroupSize, predators);
    }
    private void SpawnInGroups(GameObject prefab, int totalCount, int minGroupSize, int maxGroupSize, List<GameObject> targetList)
    {
        int spawnedCount = 0;
        while (spawnedCount < totalCount)
        {
            // 1. Znajdź losowy środek dla nowej grupy wewnątrz dużej sfery
            Vector2 randomPointOnCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 groupCenter = new Vector3(randomPointOnCircle.x, spawnHeight, randomPointOnCircle.y);

            // 2. Określ losowy rozmiar dla tej grupy
            int currentGroupSize = Random.Range(minGroupSize, maxGroupSize + 1);

            // 3. Spawnowanie osobników w tej grupie
            for (int i = 0; i < currentGroupSize; i++)
            {
                // Sprawdź, czy nie przekroczyliśmy całkowitej liczby do zespawnowania
                if (spawnedCount >= totalCount) break;

                // Znajdź losową pozycję dla osobnika blisko środka grupy
                Vector2 randomPointInGroup = Random.insideUnitCircle * groupSpawnRadius;
                Vector3 spawnPosition = groupCenter + new Vector3(randomPointInGroup.x, 0, randomPointInGroup.y);

                // Stwórz instancję obiektu
                GameObject newAgent = Instantiate(prefab, spawnPosition, Quaternion.identity);
                targetList.Add(newAgent);

                spawnedCount++;
            }
        }
    }
    public void SetTimeScale(float newTimeScale)
    {
        // Ustaw globalny mnożnik czasu w Unity
        Time.timeScale = newTimeScale;

        // Możesz dodać log, aby widzieć, jaka jest aktualna wartość
        Debug.Log($"Time scale set to: {newTimeScale}");
    }
    private void OnDrawGizmos()
    {
        if (!enableBoundary) return;
         
        Vector3 center = transform.position;
         
        Gizmos.color = new Color(1f, 0f, 0f, 1f);  
        if (shape == BoundaryShape.Circle)
        {
            Gizmos.DrawWireSphere(center, boundaryRadius);
        }
        else  
        { 
            Vector3 size = new Vector3(boundarySize.x, 1f, boundarySize.y);
            Gizmos.DrawWireCube(center, size);
        }
         
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.8f); 
        if (shape == BoundaryShape.Circle)
        {
            float marginRadius = Mathf.Max(0, boundaryRadius - boundaryMargin);
            Gizmos.DrawWireSphere(center, marginRadius);
        }
        else  
        {
             float sizeX = Mathf.Max(0, boundarySize.x - (boundaryMargin * 2));
            float sizeZ = Mathf.Max(0, boundarySize.y - (boundaryMargin * 2));
            Vector3 marginSize = new Vector3(sizeX, 1f, sizeZ);
            Gizmos.DrawWireCube(center, marginSize);
        }
    }
}