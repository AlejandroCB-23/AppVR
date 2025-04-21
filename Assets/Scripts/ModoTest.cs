#if WAVE_SDK_IMPORTED

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class FixedSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform[] endPoints;
    public GameObject[] pirateShipPrefabs;
    public GameObject[] normalShipPrefabs;
    public GameObject circleIndicatorPrefab; 

    private List<ShipSpawnEvent> schedule = new List<ShipSpawnEvent>();
    private float timer = 0f;
    private int nextEventIndex = 0;

    public GameManager gameManager;

    private bool gameEnded = false;

    void Start()
    {
        StatsTracker.Instance.ResetAll();
        FindObjectOfType<GazeShipDetector>()?.ResetDetector();

        schedule.Clear();

        // First Minute (0-60s)
        schedule.Add(new ShipSpawnEvent(0.0f, 0, true, 0, 37f));   // Pirate
        schedule.Add(new ShipSpawnEvent(2.5f, 1, true, 1, 37f));   // Pirate
        schedule.Add(new ShipSpawnEvent(4.8f, 2, false, 2, 37f));  // Fishing
        schedule.Add(new ShipSpawnEvent(7.2f, 0, true, 1, 37f));   // Pirate
        schedule.Add(new ShipSpawnEvent(9.6f, 1, false, 0, 37f));  // Fishing
        schedule.Add(new ShipSpawnEvent(12.1f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(14.4f, 0, false, 1, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(16.9f, 1, true, 0, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(19.3f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(21.7f, 0, true, 1, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(24.0f, 1, false, 0, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(26.5f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(28.8f, 0, true, 1, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(31.2f, 1, true, 0, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(33.6f, 2, false, 2, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(36.0f, 0, false, 1, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(38.4f, 1, true, 0, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(40.9f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(43.2f, 0, true, 1, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(45.6f, 1, false, 0, 37f)); // Fishing

        // Second Minute (60-120s)
        schedule.Add(new ShipSpawnEvent(48.1f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(50.5f, 0, true, 1, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(52.8f, 1, false, 0, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(55.3f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(57.6f, 0, true, 1, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(60.0f, 1, true, 0, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(62.4f, 2, false, 2, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(64.9f, 0, false, 1, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(67.2f, 1, true, 0, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(69.6f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(72.1f, 0, true, 1, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(74.4f, 1, false, 0, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(76.8f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(79.3f, 0, true, 1, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(81.6f, 1, true, 0, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(84.0f, 2, false, 2, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(86.4f, 0, false, 1, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(88.9f, 1, true, 0, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(91.2f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(93.6f, 0, true, 1, 37f));  // Pirate

        // Third Minute (120-180s)
        schedule.Add(new ShipSpawnEvent(96.1f, 1, false, 0, 37f)); // Fishing
        schedule.Add(new ShipSpawnEvent(98.5f, 2, true, 2, 37f));  // Pirate
        schedule.Add(new ShipSpawnEvent(100.8f, 0, true, 1, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(103.2f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(105.6f, 2, false, 2, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(108.0f, 0, false, 1, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(110.4f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(112.9f, 2, true, 2, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(115.2f, 0, true, 1, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(117.6f, 1, false, 0, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(120.0f, 2, true, 2, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(122.4f, 0, true, 1, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(124.8f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(127.3f, 2, false, 2, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(129.6f, 0, false, 1, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(132.0f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(134.4f, 2, true, 2, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(136.9f, 0, true, 1, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(139.2f, 1, false, 0, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(141.6f, 2, true, 2, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(144.0f, 0, true, 1, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(146.4f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(148.8f, 2, false, 2, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(151.3f, 0, false, 1, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(153.6f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(156.0f, 2, true, 2, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(158.4f, 0, true, 1, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(160.9f, 1, false, 0, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(163.2f, 2, true, 2, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(165.6f, 0, true, 1, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(168.0f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(170.4f, 2, false, 2, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(172.8f, 0, false, 1, 37f));// Fishing
        schedule.Add(new ShipSpawnEvent(175.3f, 1, true, 0, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(177.6f, 2, true, 2, 37f)); // Pirate
        schedule.Add(new ShipSpawnEvent(180.0f, 0, true, 1, 37f)); // Pirate

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }


    void Update()
    {
        if (gameEnded)
        {
            return;
        }

        timer += Time.deltaTime;

        if (gameManager != null && timer >= gameManager.gameDuration)
        {
            gameEnded = true;  
            CancelShipSpawning();  
        }

        if (nextEventIndex < schedule.Count && timer >= schedule[nextEventIndex].time)
        {
            SpawnShip(schedule[nextEventIndex]);
            nextEventIndex++;
        }
    }

    void CancelShipSpawning()
    {
        schedule.Clear();  
    }

    public void RemoveAllShips()
    {
        foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
        {
            Destroy(ship);  
        }
    }

    void SpawnShip(ShipSpawnEvent spawnEvent)
    {
        Transform spawnPoint = spawnPoints[spawnEvent.lane];
        Transform endPoint = endPoints[spawnEvent.lane];

        GameObject[] prefabArray = spawnEvent.isPirate ? pirateShipPrefabs : normalShipPrefabs;
        GameObject prefab = prefabArray[spawnEvent.sizeIndex];

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
        shipScript.Initialize(spawnEvent.isPirate, spawnEvent.speed);
        shipScript.SetDestination(endPoint.position);


        float radius = GetIndicatorRadius(spawnEvent.sizeIndex, ship);  


        GameObject indicator = Instantiate(circleIndicatorPrefab, ship.transform.position, Quaternion.identity);
        indicator.transform.SetParent(ship.transform);

        float xOffset = 0f;
        if (spawnEvent.lane == 1)
            xOffset = 3f; 
        else if (spawnEvent.lane == 2)
            xOffset = -3f; 
        else if (spawnEvent.lane == 3)
            xOffset = 5f;

        indicator.transform.localPosition = new Vector3(xOffset, 3f, 0f);


        indicator.transform.localScale = new Vector3(radius * 1.1f, radius * 2.8f, radius * 2.8f);  

        indicator.SetActive(false);
        shipScript.indicatorCircle = indicator;
    }

    float GetIndicatorRadius(int sizeIndex, GameObject ship)
    {
        float shipSize = ship.transform.localScale.x;  
        return shipSize * 0.5f;  
    }


}
#endif








