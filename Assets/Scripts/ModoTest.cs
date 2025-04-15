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
        schedule.Add(new ShipSpawnEvent(1f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(4f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(7f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(10f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(13f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(16f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(19f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(22f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(25f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(28f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(31f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(34f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(37f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(40f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(43f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(46f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(49f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(52f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(55f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(58f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(61f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(64f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(67f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(70f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(73f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(76f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(79f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(82f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(85f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(88f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(91f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(94f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(97f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(100f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(103f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(106f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(109f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(112f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(115f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(118f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(121f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(124f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(127f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(130f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(133f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(136f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(139f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(142f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(145f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(148f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(151f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(154f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(157f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(160f, 2, false, 2, 20f));
        schedule.Add(new ShipSpawnEvent(163f, 0, true, 0, 20f));
        schedule.Add(new ShipSpawnEvent(166f, 1, false, 1, 20f));
        schedule.Add(new ShipSpawnEvent(169f, 2, true, 2, 20f));
        schedule.Add(new ShipSpawnEvent(172f, 0, false, 0, 20f));
        schedule.Add(new ShipSpawnEvent(175f, 1, true, 1, 20f));
        schedule.Add(new ShipSpawnEvent(178f, 2, false, 2, 20f));
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

        // Ajustar escala uniforme
        ship.transform.localScale = new Vector3(12f, 12f, 12f);

        // Obtener el centro del mesh
        Renderer rend = ship.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Bounds bounds = rend.bounds;
            float bottomY = bounds.center.y - bounds.extents.y;

            // Desplazamos el barco ligeramente hacia abajo para que quede un poco enterrado
            float heightOffset = spawnPoint.position.y - bottomY;  // -0.5f para enterrarlo un poco
            ship.transform.position += new Vector3(0f, heightOffset, 0f);

            // Ajustamos el BoxCollider después de mover el barco
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

        // Añadir el círculo indicador alrededor del barco
        float radius = GetIndicatorRadius(spawnEvent.sizeIndex, ship);  // Tamaño dinámico
        GameObject indicator = CreateRedCircleIndicator(radius); // Crear el círculo
        indicator.transform.SetParent(ship.transform);
        indicator.transform.localPosition = Vector3.zero;  // Colocamos el círculo en el centro del barco
    }


    float GetIndicatorRadius(int sizeIndex, GameObject ship)
    {
        // Usamos la escala del barco para calcular el radio del círculo.
        float shipSize = ship.transform.localScale.x;  // Asumiendo que la escala es uniforme (X, Y, Z)

        // Ajustamos el radio del círculo según el tamaño del barco
        return shipSize * 0.5f;  // Por ejemplo, mitad del tamaño del barco
    }

    GameObject CreateRedCircleIndicator(float radius)
    {
        GameObject circle = new GameObject("ShipIndicator");
        LineRenderer lr = circle.AddComponent<LineRenderer>();

        int segments = 100;
        lr.positionCount = segments + 1;
        lr.widthMultiplier = 0.25f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.loop = true;
        lr.useWorldSpace = false;

        // Generamos el círculo alrededor del barco
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, 0f, z));  // El círculo se coloca en el plano XY
        }

        return circle;
    }
}








