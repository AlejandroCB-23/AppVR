using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixedSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform[] endPoints;
    public GameObject[] pirateShipPrefabs;
    public GameObject[] normalShipPrefabs;

    private List<ShipSpawnEvent> schedule = new List<ShipSpawnEvent>();
    private float timer = 0f;
    private int nextEventIndex = 0;

    void Start()
    {
        schedule.Add(new ShipSpawnEvent(1f, 0, true, 0, 14f));
        schedule.Add(new ShipSpawnEvent(3f, 2, false, 2, 13f));
        schedule.Add(new ShipSpawnEvent(5f, 1, true, 1, 15f));
        schedule.Add(new ShipSpawnEvent(7f, 0, false, 0, 14f));
        schedule.Add(new ShipSpawnEvent(9f, 2, true, 2, 13.5f));
        schedule.Add(new ShipSpawnEvent(11f, 1, false, 1, 14.2f));
        schedule.Add(new ShipSpawnEvent(13f, 0, true, 2, 15f));
        schedule.Add(new ShipSpawnEvent(15f, 2, false, 0, 13.8f));
        schedule.Add(new ShipSpawnEvent(17f, 1, true, 1, 14.5f));
        schedule.Add(new ShipSpawnEvent(19f, 0, false, 1, 14f));
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (nextEventIndex < schedule.Count && timer >= schedule[nextEventIndex].time)
        {
            SpawnShip(schedule[nextEventIndex]);
            nextEventIndex++;
        }

        if (nextEventIndex >= schedule.Count && timer > 30f)
        {
            SceneManager.LoadScene("Menu");
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

        if (ship.GetComponentInChildren<Collider>() == null)
        {
            ship.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent<BoxCollider>();
        }

        Ship shipScript = ship.GetComponent<Ship>();
        shipScript.Initialize(spawnEvent.isPirate, spawnEvent.speed);
        shipScript.SetDestination(endPoint.position);
    }
}








