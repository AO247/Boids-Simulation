using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    // =====================================================================
    // ||             GŁÓWNE PARAMETRY DLA UŻYTKOWNIKA                    ||
    // =====================================================================

    [Header("▶ STAN SYMULACJI (TYLKO DO ODCZYTU)")]
    [SerializeField] private int currentPreyCount;
    [SerializeField] private int currentPredatorCount;

    [Header("▶ OGÓLNE USTAWIENIA SYMULACJI")]
    [Tooltip("Kontroluje globalną prędkość upływu czasu.")]
    [Range(0f, 10f)] public float simulationTimeScale = 1.0f;
    [Tooltip("Początkowa liczba ofiar w symulacji.")]
    [Range(1, 200)] public int totalPreyCount = 50;
    [Tooltip("Początkowa liczba drapieżników w symulacji.")]
    [Range(0, 50)] public int totalPredatorCount = 5;

    [Header("▶ ZACHOWANIE OFIAR (PREY)")]
    [Tooltip("Maksymalna prędkość poruszania się ofiary.")]
    [Range(1f, 15f)] public float maxSpeed = 10.0f;
    [Tooltip("Maksymalna siła sterująca ofiary (jak gwałtownie może skręcać).")]
    [Range(1f, 15f)] public float maxForce = 10f;
    [Tooltip("Z jakiej odległości ofiara wykrywa drapieżnika.")]
    [Range(5f, 70f)] public float predatorDetectionRadius = 50.0f;
    [Tooltip("Jak silnie ofiara unika drapieżnika (siła 'strachu').")]
    [Range(0f, 5f)] public float predatorAvoidanceWeight = 3.0f; // flee weight
    [Tooltip("Jak szybko ofiara regeneruje siły, gdy nie ucieka.")]
    [Range(0.001f, 0.1f)] public float fatigueRecoveryRate = 0.02f; // fatigue regeneration
    [Tooltip("Jak szybko ofiara męczy się podczas ucieczki.")]
    [Range(0.001f, 0.1f)] public float fatigueIncreaseRate = 0.05f; // fatigue rate

    [Header("▶ ZACHOWANIE DRAPIEŻNIKÓW (PREDATOR)")]
    [Tooltip("Maksymalna prędkość drapieżnika w pościgu.")]
    [Range(1f, 15f)] public float predatorMaxSpeed = 6.0f;
    [Tooltip("Maksymalna siła sterująca drapieżnika (jak gwałtownie może skręcać).")]
    [Range(1f, 15f)] public float predatorMaxForce = 10.0f;
    [Tooltip("Z jakiej odległości drapieżnik wykrywa ofiarę.")]
    [Range(10f, 100f)] public float preyDetectionRadius = 70.0f;
    [Tooltip("Jak agresywnie drapieżnik skupia się na celu.")]
    [Range(0f, 5f)] public float chaseWeight = 3.0f;
    [Tooltip("Jak szybko narasta głód drapieżnika.")]
    [Range(0.001f, 0.1f)] public float predatorHungerRate = 0.01f; // hunger rate
    [Tooltip("Jakie obrażenia drapieżnik zadaje przy jednym ataku.")]
    [Range(0.1f, 0.5f)] public float damagePerHit = 0.2f;
    [Tooltip("Czas w sekundach, jaki drapieżnik musi odczekać między atakami.")]
    [Range(0f, 5f)] public float hitCooldown = 3.0f; // cooldown attack

    [Header("▶ ZACHOWANIE STADNE (BOIDS)")]
    [Tooltip("W jakiej odległości zwierzęta zaczynają się odpychać (dotyczy ofiar i drapieżników).")]
    [Range(5f , 10f)] public float seperationRadius = 7.0f;
    [Tooltip("Jak silnie zwierzęta odpychają się od siebie.")]
    [Range(0f, 10f)] public float seperationWeight = 7.0f;
    [Tooltip("Z jakiej odległości zwierzęta dopasowują swój kierunek i prędkość.")]
    [Range(10f, 30f)] public float alignmentRadius = 20.0f;
    [Tooltip("Jak silnie zwierzęta dopasowują swój kierunek.")]
    [Range(0f, 5f)] public float alignmentWeight = 2.0f;
    [Tooltip("Z jakiej odległości zwierzęta próbują trzymać się środka stada.")]
    [Range(10f, 30f)] public float cohesionRadius = 20.0f;
    [Tooltip("Jak silnie zwierzęta przyciągają się do środka stada.")]
    [Range(0f, 5f)] public float cohesionWeight = 1.5f;


    // =====================================================================
    // ||                  USTAWIENIA ZAAWANSOWANE                        ||
    // =====================================================================
    [Space(20)]
    [Header("▼ USTAWIENIA ZAAWANSOWANE (DLA DEWELOPERÓW)")]

    [Header("Ogólne Ustawienia Agentów")]
    public float updateInterval = 0.1f;
    public float rotationSmoothSpeed = 5.0f;
    public float randomMovementWeight = 0.5f;

    [Header("Zaawansowane Ustawienia Wędrowania Ofiar")]
    public float wanderRadius = 1.5f;
    public float wanderDistance = 2.0f;
    public float wanderJitter = 80.0f;
    public float wanderWeight = 1.0f;

    [Header("Zaawansowane Ustawienia Drapieżników")]
    public float predatorFriction = 0.9f;

    [Header("Zaawansowane Ustawienia Stada Drapieżników")]
    public float predatorSeperationRadius = 5.0f;
    public float predatorAlignmentRadius = 10.0f;
    public float predatorCohesionRadius = 10.0f;
    public float predatorSeperationWeight = 2.5f;
    public float predatorAlignmentWeight = 1.0f;
    public float predatorCohesionWeight = 1.0f;
    public float hitDistance = 2.0f;

    [Header("Zaawansowane Ustawienia Polowania")]
    public float eatDistance = 1.0f;
    public float eatingCooldown = 2.0f;

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

    [Header("Zaawansowane Ustawienia Granic Świata")]
    public bool enableBoundary = true;
    public enum BoundaryShape { Circle, Box }
    public BoundaryShape shape = BoundaryShape.Circle;
    public float boundaryRadius = 100f;
    public Vector2 boundarySize = new Vector2(200, 200);
    public float boundaryMargin = 15f;
    public float boundaryAvoidanceWeight = 4.0f;
    public float boundaryForceMultiplier = 50.0f;

    [Header("Zaawansowane Ustawienia Przetrwania")]
    [Tooltip("Szansa na pojawienie się nowego potomstwa ofiar (na sekundę na parę).")]
    [Range(0f, 0.01f)] public float preyReproductionChance = 0.001f;

    // --- Zmienne systemowe (nie do edycji w Inspectorze) ---
    [HideInInspector] public List<GameObject> preys = new List<GameObject>();
    [HideInInspector] public List<GameObject> predators = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI simulationTimeText;
    [SerializeField] private GameObject preyPrefab;
    [SerializeField] private GameObject predatorPrefab;
    private float totalSimulationTime = 0.0f;

    public GameObject parameters;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            parameters.SetActive(!parameters.activeSelf);
        }
        Time.timeScale = simulationTimeScale;
        totalSimulationTime += Time.deltaTime;
        UpdateSimulationTimeDisplay();
        HandleReproduction();
        currentPreyCount = preys.Count;
        currentPredatorCount = predators.Count;
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

    #region UI_CALLBACK_FUNCTIONS

    // --- Zachowanie Ofiar (Prey) ---

    public void SetSimulationTimeScale(float value) => simulationTimeScale = value;
    public void SetPreyMaxSpeed(float value) => maxSpeed = value;
    public void SetPreyMaxForce(float value) => maxForce = value;
    public void SetPreyPredatorDetectionRadius(float value) => predatorDetectionRadius = value;
    public void SetPreyFleeWeight(float value) => predatorAvoidanceWeight = value;
    public void SetPreyFatigueRegeneration(float value) => fatigueRecoveryRate = value;
    public void SetPreyFatigueRate(float value) => fatigueIncreaseRate = value;

    // --- Zachowanie Drapieżników (Predator) ---
    public void SetPredatorMaxSpeed(float value) => predatorMaxSpeed = value;
    public void SetPredatorMaxForce(float value) => predatorMaxForce = value;
    public void SetPredatorPreyDetectionRadius(float value) => preyDetectionRadius = value;
    public void SetPredatorChaseWeight(float value) => chaseWeight = value;
    public void SetPredatorHungerRate(float value) => predatorHungerRate = value;
    public void SetPredatorDamagePerHit(float value) => damagePerHit = value;
    public void SetPredatorAttackCooldown(float value) => hitCooldown = value;

    // --- Zachowanie Stadne (Boids) ---
    public void SetSeparationRange(float value) => seperationRadius = value;
    public void SetSeparationWeight(float value) => seperationWeight = value;
    public void SetAlignmentRange(float value) => alignmentRadius = value;
    public void SetAlignmentWeight(float value) => alignmentWeight = value;
    public void SetCohesionRange(float value) => cohesionRadius = value;
    public void SetCohesionWeight(float value) => cohesionWeight = value;

    #endregion

}


