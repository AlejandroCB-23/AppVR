#if WAVE_SDK_IMPORTED

using System.Collections.Generic;
using UnityEngine;

public class ModoAleatorio : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform[] endPoints;
    public GameObject[] pirateShipPrefabs;
    public GameObject[] normalShipPrefabs;
    public GameObject redShipPrefab;
    public GameObject circleIndicatorPrefab;

    public GameManagerModoAleatorio gameManager;
    public GameObject[] heartLives;

    public float spawnIntervalMin = 0.1f;
    public float spawnIntervalMax = 0.18f;

    private float nextSpawnTime = 0f;
    private float timer = 0f;

    private int shipsGeneratedThisCycle = 0;
    private int maxShipsInCycle = 60;

    private int currentLaneIndex = 0;
    private System.Random pseudoRandom = new System.Random();
    private Dictionary<int, float> lastSpawnTimePerLane = new Dictionary<int, float>();

    private bool gameEnded = false;
    private int lastEliminatedCount = 0;

    private float shipSpeed = 32f;
    public float speedIncreaseRate = 10f;
    private float difficultyTimer = 0f;

    private int shipsSpawnedSinceLastRed = 0;
    private const int shipsPerRedShip = 12;

    public float baseMinDistance = 50f;
    public float sinkingExtraDistance = 30f;
    public float speedDistanceMultiplier = 1.6f;

    private float currentMinDistance;

    private List<GameObject> activeShips = new List<GameObject>();
    private int shipCounter = 0;

    private float globalSpawnCooldown = 0f;
    private const float minGlobalSpawnInterval = 0.02f;

    void Start()
    {
        StatsTracker.Instance.ResetAll();
        currentMinDistance = baseMinDistance;
        nextSpawnTime = Random.Range(spawnIntervalMin, spawnIntervalMax);

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManagerModoAleatorio>();
        }
    }

    void Update()
    {
        if (gameEnded) return;

        timer += Time.deltaTime;
        difficultyTimer += Time.deltaTime;
        globalSpawnCooldown -= Time.deltaTime;

        CleanupDestroyedShips();
        CheckEliminatedLives();

        if (GetLostLivesCount() >= heartLives.Length)
        {
            gameEnded = true;
            CancelShipSpawning();
            return;
        }

        if (difficultyTimer >= 2f)
        {
            difficultyTimer = 0f;

            if (Time.timeSinceLevelLoad < 120f)
            {
                float oldSpeed = shipSpeed;
                shipSpeed = Mathf.Min(85f, shipSpeed + speedIncreaseRate);
                maxShipsInCycle = Mathf.Min(200, maxShipsInCycle + 10);

                spawnIntervalMin = Mathf.Max(0.03f, spawnIntervalMin - 0.008f);
                spawnIntervalMax = Mathf.Max(0.08f, spawnIntervalMax - 0.008f);

                UpdateMinDistance();

                if (shipSpeed != oldSpeed)
                {
                    UpdateAllActiveShipsSpeed();
                }
            }
        }

        int shipsToTrySpawn = GetShipsToSpawn();
        int successfulSpawns = 0;

        for (int i = 0; i < shipsToTrySpawn && shipsGeneratedThisCycle < maxShipsInCycle; i++)
        {
            if (timer >= nextSpawnTime && globalSpawnCooldown <= 0f)
            {
                if (SpawnRandomShip())
                {
                    successfulSpawns++;
                    shipsGeneratedThisCycle++;
                    globalSpawnCooldown = minGlobalSpawnInterval;
                }
                else
                {
                    nextSpawnTime = timer + Random.Range(0.05f, 0.15f);
                }
            }
        }

        if (successfulSpawns > 0)
        {
            nextSpawnTime = timer + Mathf.Lerp(spawnIntervalMin, spawnIntervalMax, Random.value * 0.2f);
        }
        else if (timer >= nextSpawnTime)
        {
            nextSpawnTime = timer + spawnIntervalMin * 0.5f;
        }

        if (shipsGeneratedThisCycle >= maxShipsInCycle)
        {
            shipsGeneratedThisCycle = 0;
        }
    }

    int GetShipsToSpawn()
    {
        float timeElapsed = Time.timeSinceLevelLoad;
        int activeShipCount = activeShips.Count;

        int baseShips = 1;
        if (timeElapsed > 240f) baseShips = 6;
        else if (timeElapsed > 180f) baseShips = 5;
        else if (timeElapsed > 120f) baseShips = 4;
        else if (timeElapsed > 60f) baseShips = 3;
        else if (timeElapsed > 30f) baseShips = 2;

        if (activeShipCount < 8) baseShips += 2;
        else if (activeShipCount < 12) baseShips += 1;

        return Mathf.Min(baseShips, 8);
    }

    void UpdateMinDistance()
    {
        currentMinDistance = Mathf.Max(baseMinDistance, shipSpeed * speedDistanceMultiplier);
    }

    void UpdateAllActiveShipsSpeed()
    {
        foreach (var ship in activeShips)
        {
            if (ship != null)
            {
                Ship shipScript = ship.GetComponent<Ship>();
                if (shipScript != null && !shipScript.IsSinking())
                {
                    shipScript.UpdateSpeed(shipSpeed);
                }
            }
        }
    }

    void CleanupDestroyedShips()
    {
        for (int i = activeShips.Count - 1; i >= 0; i--)
        {
            if (activeShips[i] == null)
                activeShips.RemoveAt(i);
        }
    }

    void CheckEliminatedLives()
    {
        int eliminatedCount = StatsTracker.Instance.GetTotalLivesLost();
        if (eliminatedCount > lastEliminatedCount)
        {
            for (int i = 0; i < (eliminatedCount - lastEliminatedCount); i++)
            {
                RemoveNextActiveHeartLeftToRight();
            }
            lastEliminatedCount = eliminatedCount;
        }
    }

    public int GetLostLivesCount()
    {
        int lost = 0;
        foreach (var heart in heartLives)
        {
            if (!heart.activeSelf)
                lost++;
        }
        return lost;
    }

    void RemoveNextActiveHeartLeftToRight()
    {
        for (int i = 0; i < heartLives.Length; i++)
        {
            if (heartLives[i].activeSelf)
            {
                heartLives[i].SetActive(false);
                break;
            }
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
        lastEliminatedCount = StatsTracker.Instance.GetTotalLivesLost();
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
        activeShips.Clear();
    }

    bool CanSpawnInLane(int laneIndex, Vector3 spawnPosition)
    {
        foreach (var ship in activeShips)
        {
            if (ship == null) continue;

            if (Mathf.Abs(ship.transform.position.x - spawnPosition.x) < 8f)
            {
                float distance = Vector3.Distance(ship.transform.position, spawnPosition);
                Ship shipScript = ship.GetComponent<Ship>();

                if (shipScript != null && shipScript.IsSinking())
                {
                    if (distance < currentMinDistance + sinkingExtraDistance)
                        return false;
                }
                else
                {
                    float gameTimeMultiplier = Mathf.Max(1f, Time.timeSinceLevelLoad / 30f);
                    float adjustedMinDistance = currentMinDistance * (1.2f + (1f / gameTimeMultiplier));

                    if (ship.transform.position.z > spawnPosition.z - 25f)
                    {
                        adjustedMinDistance *= 1.4f;
                    }

                    if (distance < adjustedMinDistance)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    bool SpawnRandomShip()
    {
        const int maxAttempts = 50;
        int attempt = 0;
        List<int> attemptedLanes = new List<int>();
        List<int> availableLanes = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (CanSpawnInLane(i, spawnPoints[i].position))
            {
                availableLanes.Add(i);
            }
        }

        if (availableLanes.Count == 0)
        {
            return false;
        }

        availableLanes.Sort((a, b) => {
            float timeA = lastSpawnTimePerLane.ContainsKey(a) ? Time.time - lastSpawnTimePerLane[a] : float.MaxValue;
            float timeB = lastSpawnTimePerLane.ContainsKey(b) ? Time.time - lastSpawnTimePerLane[b] : float.MaxValue;
            return timeB.CompareTo(timeA);
        });

        while (attempt < maxAttempts && attemptedLanes.Count < spawnPoints.Length)
        {
            attempt++;

            int lane;
            if (availableLanes.Count > 0 && attempt <= availableLanes.Count)
            {
                lane = availableLanes[attempt - 1];
            }
            else
            {
                lane = currentLaneIndex;
                currentLaneIndex = (currentLaneIndex + 1) % spawnPoints.Length;
            }

            if (attemptedLanes.Contains(lane)) continue;

            Transform spawnPoint = spawnPoints[lane];
            Transform endPoint = endPoints[lane];

            if (!CanSpawnInLane(lane, spawnPoint.position))
            {
                attemptedLanes.Add(lane);
                continue;
            }

            if (lastSpawnTimePerLane.TryGetValue(lane, out float lastTime))
            {
                float earlyGameMultiplier = Time.timeSinceLevelLoad < 45f ? 1.5f : 1f;
                float minCooldown = (currentMinDistance / shipSpeed) * 0.7f * earlyGameMultiplier;
                if ((Time.time - lastTime) < minCooldown)
                {
                    attemptedLanes.Add(lane);
                    continue;
                }
            }

            bool isPirate = Random.value < 0.7f;
            int sizeIndex = Random.Range(0, 3);
            GameObject prefab;
            bool isRed = false;

            bool shouldSpawnRed = GetLostLivesCount() > 0 &&
                                 shipsSpawnedSinceLastRed >= shipsPerRedShip &&
                                 Random.value < 0.7f;

            if (shouldSpawnRed)
            {
                prefab = redShipPrefab;
                isRed = true;
                shipsSpawnedSinceLastRed = 0;
            }
            else
            {
                prefab = isPirate ? pirateShipPrefabs[sizeIndex] : normalShipPrefabs[sizeIndex];
                shipsSpawnedSinceLastRed++;
            }

            GameObject ship = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            ship.transform.localScale = new Vector3(12f, 12f, 12f);
            ship.tag = "Ship";
            ship.name = prefab.name + "_" + (++shipCounter);
            activeShips.Add(ship);

            Renderer rend = ship.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                float bottomY = rend.bounds.center.y - rend.bounds.extents.y;
                float heightOffset = spawnPoint.position.y - bottomY;
                ship.transform.position += new Vector3(0f, heightOffset, 0f);

                BoxCollider boxCollider = ship.GetComponentInChildren<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.center -= new Vector3(0, heightOffset, 0);
                }
            }

            if (ship.GetComponentInChildren<Collider>() == null)
            {
                ship.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent<BoxCollider>();
            }

            Ship shipScript = ship.GetComponent<Ship>();
            shipScript.Initialize(isPirate, shipSpeed);
            shipScript.SetDestination(endPoint.position);
            shipScript.isRedShip = isRed;

            GameObject indicator = Instantiate(circleIndicatorPrefab, ship.transform.position, Quaternion.identity);
            indicator.transform.SetParent(ship.transform);

            float xOffset = (lane == 1) ? 3f : (lane == 2) ? -3f : (lane == 3) ? 5f : 0f;
            float radius = ship.transform.localScale.x * 0.5f;
            indicator.transform.localPosition = new Vector3(xOffset, 3f, 0f);
            indicator.transform.localScale = new Vector3(radius * 1.1f, radius * 2.8f, radius * 2.8f);
            indicator.SetActive(false);

            shipScript.indicatorCircle = indicator;
            lastSpawnTimePerLane[lane] = Time.time;

            return true;
        }

        return false;
    }
}

#endif

































