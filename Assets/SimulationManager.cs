using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [Header("▶ STAN SYMULACJI (TYLKO DO ODCZYTU)")]
    [SerializeField, ReadOnly] private int currentPreyCount;
    [SerializeField, ReadOnly] private int currentPredatorCount;

    [Header("▶ OGÓLNE USTAWIENIA SYMULACJI")]
    [Tooltip("Kontroluje globalną prędkość upływu czasu.")]
    [Range(0f, 10f)] public float simulationTimeScale = 1.0f;
    [Tooltip("Początkowa liczba ofiar w symulacji.")]
    [Range(1, 200)] public int totalPreyCount = 50;
    [Tooltip("Początkowa liczba drapieżników w symulacji.")]
    [Range(0, 50)] public int totalPredatorCount = 5;

    [Header("▶ USTAWIENIA PRZETRWANIA (SURVIVAL)")]
    [Tooltip("Szansa na pojawienie się nowego potomstwa ofiar (na sekundę na parę).")]
    [Range(0f, 0.01f)] public float preyReproductionChance = 0.001f;
    [Tooltip("Jak szybko drapieżniki stają się głodne (jednostki na sekundę).")]
    [Range(0.001f, 0.1f)] public float predatorHungerRate = 0.01f;
    [Tooltip("Jak szybko ofiary męczą się podczas ucieczki (jednostki na sekundę).")]
    [Range(0.01f, 0.2f)] public float preyFatigueRate = 0.05f;

    [Header("▶ USTAWIENIA ZACHOWAŃ (BEHAVIOR)")]
    [Tooltip("Maksymalna prędkość poruszania się ofiary.")]
    [Range(1f, 15f)] public float maxSpeed = 3.0f;
    [Tooltip("Maksymalna prędkość poruszania się drapieżnika w pościgu.")]
    [Range(1f, 15f)] public float predatorMaxSpeed = 6.0f;
    [Tooltip("Z jakiej odległości ofiara może wykryć drapieżnika i zacząć uciekać.")]
    [Range(5f, 70f)] public float predatorDetectionRadius = 20.0f;
    [Tooltip("Jak silnie ofiara unika drapieżnika (siła 'strachu').")]
    [Range(0f, 10f)] public float predatorAvoidanceWeight = 2.0f;
    [Tooltip("Jak silnie ofiary trzymają się razem w stadzie (instynkt stadny).")]
    [Range(0f, 5f)] public float cohesionWeight = 1.5f;
    [Tooltip("Z jakiej odległości drapieżnik może wykryć ofiarę i zacząć polować.")]
    [Range(10f, 100f)] public float preyDetectionRadius = 50.0f;
    [Tooltip("Jak agresywnie drapieżnik skupia się na celu (vs. trzymanie się stada).")]
    [Range(0f, 5f)] public float chaseWeight = 3.0f;

    [Header("▶ USTAWIENIA ŚWIATA")]
    [Tooltip("Czy symulacja ma być ograniczona granicami mapy.")]
    public bool enableBoundary = true;
    [Tooltip("Rozmiar (promień) dostępnego dla zwierząt obszaru.")]
    [Range(50f, 1000f)] public float boundaryRadius = 500f;

    // =====================================================================
    // ||                  USTAWIENIA ZAAWANSOWANE                        ||
    // =====================================================================
    [Space(20)]
    [Header("▼ USTAWIENIA ZAAWANSOWANE (DLA DEWELOPERÓW)")]

    [Header("Ogólne Ustawienia Agentów")]
    public float updateInterval = 0.1f;
    public float rotationSmoothSpeed = 5.0f;

    [Header("Zaawansowane Ustawienia Ofiar (Prey)")]
    public float seperationRadius = 2.0f;
    public float alignmentRadius = 3.0f;
    public float cohesionRadius = 4.0f;
    public float maxForce = 0.5f;
    public float seperationWeight = 6.0f;
    public float alignmentWeight = 4.0f;
    public float randomMovementWeight = 0.5f;
    public float fatigueRecoveryRate = 0.02f;
    public float fatigueIncreaseRate = 0.05f;
    public float wanderRadius = 1.5f;
    public float wanderDistance = 2.0f;
    public float wanderJitter = 80.0f;
    public float wanderWeight = 1.0f;

    [Header("Zaawansowane Ustawienia Drapieżników (Predator)")]
    public float predatorMaxForce = 10.0f;
    public float predatorFriction = 0.9f;

    [Header("Zaawansowane Ustawienia Stada Drapieżników")]
    public float predatorSeperationRadius = 5.0f;
    public float predatorAlignmentRadius = 10.0f;
    public float predatorCohesionRadius = 10.0f;
    public float predatorSeperationWeight = 2.5f;
    public float predatorAlignmentWeight = 1.0f;
    public float predatorCohesionWeight = 1.0f;

    [Header("Zaawansowane Ustawienia Polowania")]
    public float eatDistance = 1.0f;
    public float hitDistance = 3.0f;
    public float eatingCooldown = 2.0f;
    public float hitCooldown = 3.0f;
    public float damagePerHit = 0.2f;

    [Header("Zaawansowane Ustawienia Omijania Przeszkód")]
    public float obstacleAvoidanceDistance = 10.0f;
    public float obstacleAvoidanceWeight = 5.0f;
    public float whiskerAngle = 30.0f;

    [Header("Zaawansowane Ustawienia Spawnowania")]
    [SerializeField] private Vector2Int preyGroupSize = new Vector2Int(5, 6);
    [SerializeField] private int predatorGroupSize = 3;
    [SerializeField] private float spawnRadius = 500f;
    [SerializeField] private float groupSpawnRadius = 5.0f;
    [SerializeField] private float spawnHeight = 10.0f;

    //[Header("Zaawansowane Ustawienia Granic Świata")]
    public enum BoundaryShape { Circle, Box }
    public BoundaryShape shape = BoundaryShape.Circle;
    public Vector2 boundarySize = new Vector2(200, 200);
    public float boundaryMargin = 15f;
    public float boundaryAvoidanceWeight = 4.0f;
    public float boundaryForceMultiplier = 50.0f;

    // --- Zmienne systemowe (nie do edycji w Inspectorze) ---
    [HideInInspector] public List<GameObject> preys = new List<GameObject>();
    [HideInInspector] public List<GameObject> predators = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI simulationTimeText;
    [SerializeField] private GameObject preyPrefab;
    [SerializeField] private GameObject predatorPrefab;
    private float totalSimulationTime = 0.0f;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        SpawnInGroups(preyPrefab, totalPreyCount, preyGroupSize.x, preyGroupSize.y, preys);
        SpawnInGroups(predatorPrefab, totalPredatorCount, predatorGroupSize, predatorGroupSize, predators);
    }

    void Update()
    {
        Time.timeScale = simulationTimeScale;
        totalSimulationTime += Time.deltaTime;
        UpdateSimulationTimeDisplay();
        HandleReproduction();
    }

    void HandleReproduction()
    {
        if (Time.frameCount % 10 != 0) return;

        int preyPairs = preys.Count / 2;
        if (preyPairs > 0)
        {
            if (Random.value < preyReproductionChance * preyPairs * Time.deltaTime * 10)
            {
                if (preys.Count > 0)
                {
                    GameObject parent = preys[Random.Range(0, preys.Count)];
                    Vector3 spawnPos = parent.transform.position + Random.insideUnitSphere * 2;
                    spawnPos.y = spawnHeight;
                    Instantiate(preyPrefab, spawnPos, Quaternion.identity);
                }
            }
        }
    }

    private void UpdateSimulationTimeDisplay()
    {
        if (simulationTimeText == null) return;
        float time = totalSimulationTime;
        int hours = (int)(time / 3600);
        time %= 3600;
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        simulationTimeText.text = string.Format("Czas Symulacji: {0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    private void SpawnInGroups(GameObject prefab, int totalCount, int minGroupSize, int maxGroupSize, List<GameObject> targetList)
    {
        int spawnedCount = 0;
        while (spawnedCount < totalCount)
        {
            Vector2 randomPointOnCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 groupCenter = new Vector3(randomPointOnCircle.x, spawnHeight, randomPointOnCircle.y);
            int currentGroupSize = Random.Range(minGroupSize, maxGroupSize + 1);

            for (int i = 0; i < currentGroupSize; i++)
            {
                if (spawnedCount >= totalCount) break;
                Vector2 randomPointInGroup = Random.insideUnitCircle * groupSpawnRadius;
                Vector3 spawnPosition = groupCenter + new Vector3(randomPointInGroup.x, 0, randomPointInGroup.y);
                GameObject newAgent = Instantiate(prefab, spawnPosition, Quaternion.identity);
                // Nie dodajemy do listy tutaj, bo agent robi to sam w swoim Starcie
            }
            spawnedCount += currentGroupSize; // Optymalizacja, aby uniknąć pętli w pętli
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        if (!enableBoundary) return;
        Vector3 center = transform.position;
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        if (shape == BoundaryShape.Circle) { Gizmos.DrawWireSphere(center, boundaryRadius); }
        else { Gizmos.DrawWireCube(center, new Vector3(boundarySize.x, 1f, boundarySize.y)); }

        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.8f);
        if (shape == BoundaryShape.Circle) { Gizmos.DrawWireSphere(center, Mathf.Max(0, boundaryRadius - boundaryMargin)); }
        else { Gizmos.DrawWireCube(center, new Vector3(Mathf.Max(0, boundarySize.x - (boundaryMargin * 2)), 1f, Mathf.Max(0, boundarySize.y - (boundaryMargin * 2)))); }
    }
}




public class ReadOnlyAttribute : PropertyAttribute { }
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        bool wasEnabled = GUI.enabled;
        GUI.enabled = false;
        UnityEditor.EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = wasEnabled;
    }
}