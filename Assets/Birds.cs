using UnityEngine;

public class Birds : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject birdPrefab;
    [Range(0, 500)] // U¿ycie suwaka dla bezpieczeñstwa i wygody
    public int numberOfBirds = 100;
    public Vector3 spawnBounds = new Vector3(30, 30, 30);

    [Header("Bird Settings")]
    [Range(0, 20)]
    public float maxSpeed = 5f;
    [Range(0, 5)]
    public float maxForce = 0.5f;
    [Tooltip("Zasiêg, w którym ptak widzi inne ptaki.")]
    [Range(0, 10)]
    public float perceptionRadius = 3.0f;
    [Tooltip("Minimalny dystans, jaki ptak stara siê utrzymaæ od innych.")]
    [Range(0, 10)]
    public float separationRadius = 1.5f;

    [Header("Rule Weights")]
    [Range(0, 5)]
    public float separationWeight = 1.5f;
    [Range(0, 5)]
    public float alignmentWeight = 1.0f;
    [Range(0, 5)]
    public float cohesionWeight = 1.0f;

    [Header("Live Info")]
    [SerializeField] // Pozwala zobaczyæ prywatn¹ zmienn¹ w Inspectorze
    private int currentBirdCount = 0;

    void Start()
    {
        // Pocz¹tkowe stworzenie ptaków
        UpdateBirdCount();
    }

    void Update()
    {
        // W ka¿dej klatce sprawdzaj, czy trzeba dostosowaæ liczbê ptaków
        UpdateBirdCount();
        currentBirdCount = BirdController.BoidsCount;
    }

    void UpdateBirdCount()
    {
        // Jeœli jest za ma³o ptaków, dodaj brakuj¹ce
        while (BirdController.BoidsCount < numberOfBirds)
        {
            SpawnBird();
        }

        // Jeœli jest za du¿o ptaków, usuñ nadmiarowe
        while (BirdController.BoidsCount > numberOfBirds)
        {
            RemoveBird();
        }
    }

    void SpawnBird()
    {
        Vector3 randomPos = transform.position + new Vector3(
            Random.Range(-spawnBounds.x / 2, spawnBounds.x / 2),
            Random.Range(-spawnBounds.y / 2, spawnBounds.y / 2),
            Random.Range(-spawnBounds.z / 2, spawnBounds.z / 2)
        );

        GameObject birdGO = Instantiate(birdPrefab, randomPos, Quaternion.identity);
        birdGO.GetComponent<BirdController>()?.Initialize(this);
    }

    void RemoveBird()
    {
        if (BirdController.BoidsCount > 0)
        {
            // Pobierz ostatniego ptaka z listy i zniszcz jego obiekt
            BirdController birdToRemove = BirdController.GetBoid(BirdController.BoidsCount - 1);
            Destroy(birdToRemove.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawCube(transform.position, spawnBounds);
    }
}