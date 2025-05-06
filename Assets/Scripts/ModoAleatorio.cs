#if WAVE_SDK_IMPORTED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModoAleatorio : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform[] endPoints;
    public GameObject[] pirateShipPrefabs;
    public GameObject[] normalShipPrefabs;
    public GameObject redShipPrefab; // Prefab del barco rojo
    public GameObject circleIndicatorPrefab;

    public GameManagerModoAleatorio gameManager;
    public GameObject[] heartLives;

    public float spawnIntervalMin = 1.0f;
    public float spawnIntervalMax = 2.5f;
    public float minSpawnDistance = 60f;

    private float nextSpawnTime = 0f;
    private float timer = 0f;
    private bool gameEnded = false;
    private int lastEliminatedCount = 0;

    private int shipsSpawnedSinceLastRed = 0;
    private const int shipsPerRedShip = 5;

    private List<GameObject> activeShips = new List<GameObject>();

    void Start()
    {
        StatsTracker.Instance.ResetAll();
        FindObjectOfType<GazeShipDetector>()?.ResetDetector();

        nextSpawnTime = Random.Range(spawnIntervalMin, spawnIntervalMax);

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManagerModoAleatorio>();
        }
    }

    void Update()
    {
        if (gameEnded)
            return;

        timer += Time.deltaTime;

        int eliminatedCount = StatsTracker.Instance.GetFishingEliminatedAleatorio();

        if (eliminatedCount > lastEliminatedCount)
        {
            RemoveHeartLife(eliminatedCount - 1);
            lastEliminatedCount = eliminatedCount;
        }

        if (eliminatedCount >= 3)
        {
            gameEnded = true;
            CancelShipSpawning();
            return;
        }

        if (timer >= nextSpawnTime)
        {
            SpawnRandomShip();
            nextSpawnTime = timer + Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    void RemoveHeartLife(int index)
    {
        if (index >= 0 && index < heartLives.Length)
        {
            heartLives[index].SetActive(false);
        }
    }

    public void RestoreLife()
    {
        for (int i = heartLives.Length - 1; i >= 0; i--)
        {
            if (!heartLives[i].activeSelf)
            {
                heartLives[i].SetActive(true);
                break;
            }
        }
    }

    int GetLostLivesCount()
    {
        int lost = 0;
        foreach (var heart in heartLives)
        {
            if (!heart.activeSelf)
                lost++;
        }
        return lost;
    }

    void CancelShipSpawning()
    {
        Debug.Log("Spawning stopped: 3 fishing ships destroyed.");
    }

    public void RemoveAllShips()
    {
        foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
        {
            Destroy(ship);
        }
    }

    void SpawnRandomShip()
    {
        const int maxAttempts = 10;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;

            int lane = Random.Range(0, spawnPoints.Length);
            bool isPirate = Random.value < 0.7f;
            int sizeIndex = Random.Range(0, 3);

            Transform spawnPoint = spawnPoints[lane];
            Transform endPoint = endPoints[lane];

            bool tooClose = false;
            foreach (GameObject existing in activeShips)
            {
                if (existing != null && Vector3.Distance(existing.transform.position, spawnPoint.position) < minSpawnDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            GameObject prefab;
            bool isRed = false;

            if (GetLostLivesCount() > 0 && shipsSpawnedSinceLastRed >= shipsPerRedShip)
            {
                prefab = redShipPrefab;
                isRed = true;
                shipsSpawnedSinceLastRed = 0;
            }
            else
            {
                GameObject[] prefabArray = isPirate ? pirateShipPrefabs : normalShipPrefabs;
                prefab = prefabArray[sizeIndex];
                shipsSpawnedSinceLastRed++;
            }

            GameObject ship = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            ship.transform.localScale = new Vector3(12f, 12f, 12f);
            ship.tag = "Ship";
            activeShips.Add(ship);

            Renderer rend = ship.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                Bounds bounds = rend.bounds;
                float bottomY = bounds.center.y - bounds.extents.y;
                float heightOffset = spawnPoint.position.y - bottomY;
                ship.transform.position += new Vector3(0f, heightOffset, 0f);

                BoxCollider boxCollider = ship.GetComponentInChildren<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y - heightOffset, boxCollider.center.z);
                }
            }

            if (ship.GetComponentInChildren<Collider>() == null)
            {
                ship.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent<BoxCollider>();
            }

            Ship shipScript = ship.GetComponent<Ship>();
            shipScript.Initialize(isPirate, 37f);
            shipScript.SetDestination(endPoint.position);

            if (isRed)
            {
                shipScript.isRedShip = true;
            }

            float radius = ship.transform.localScale.x * 0.5f;

            GameObject indicator = Instantiate(circleIndicatorPrefab, ship.transform.position, Quaternion.identity);
            indicator.transform.SetParent(ship.transform);

            float xOffset = 0f;
            if (lane == 1) xOffset = 3f;
            else if (lane == 2) xOffset = -3f;
            else if (lane == 3) xOffset = 5f;

            indicator.transform.localPosition = new Vector3(xOffset, 3f, 0f);
            indicator.transform.localScale = new Vector3(radius * 1.1f, radius * 2.8f, radius * 2.8f);
            indicator.SetActive(false);

            shipScript.indicatorCircle = indicator;

            return;
        }

        Debug.Log("No se pudo generar barco tras varios intentos (por solapamiento).");
    }
}
#endif







