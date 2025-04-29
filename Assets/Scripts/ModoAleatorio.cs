#if WAVE_SDK_IMPORTED

using System.Collections;
using UnityEngine;

public class ModoAleatorio : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform[] endPoints;
    public GameObject[] pirateShipPrefabs;
    public GameObject[] normalShipPrefabs;
    public GameObject circleIndicatorPrefab;

    public GameManagerModoAleatorio gameManager;

    public float spawnIntervalMin = 2.0f;
    public float spawnIntervalMax = 5.0f;
    private float nextSpawnTime = 0f;

    private float timer = 0f;
    private bool gameEnded = false;

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

        if (StatsTracker.Instance.GetFishingEliminatedAleatorio() >= 3)
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

    void CancelShipSpawning()
    {
        // Optional: Any logic when stopping
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
        int lane = Random.Range(0, spawnPoints.Length);
        bool isPirate = Random.value < 0.7f; // 70% pirate, 30% fishing
        int sizeIndex = Random.Range(0, 3); // Assumes 3 sizes

        Transform spawnPoint = spawnPoints[lane];
        Transform endPoint = endPoints[lane];

        GameObject[] prefabArray = isPirate ? pirateShipPrefabs : normalShipPrefabs;
        GameObject prefab = prefabArray[sizeIndex];

        GameObject ship = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        ship.transform.localScale = new Vector3(12f, 12f, 12f);

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
        shipScript.Initialize(isPirate, 37f); // Fixed speed
        shipScript.SetDestination(endPoint.position);

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
    }
}
#endif



