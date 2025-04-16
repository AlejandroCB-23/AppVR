using System.Collections.Generic;
using UnityEngine;

public class FixedSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Transform[] endPoints;
    public GameObject[] pirateShipPrefabs;
    public GameObject[] normalShipPrefabs;
    public GameObject circleIndicatorPrefab; // Prefab del círculo indicador

    private List<ShipSpawnEvent> schedule = new List<ShipSpawnEvent>();
    private float timer = 0f;
    private int nextEventIndex = 0;

    // NUEVO: Referencia al GameManager
    public GameManager gameManager;

    // Nueva variable para saber si se debe detener el spawn
    private bool gameEnded = false;

    void Start()
    {
        FindObjectOfType<GazeShipDetector>()?.ResetDetector();

        schedule.Clear();
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

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }


    void Update()
    {
        // Si el juego terminó, no seguir procesando los eventos de spawn
        if (gameEnded)
        {
            return;
        }

        timer += Time.deltaTime;

        // Detener la aparición de barcos si el tiempo del juego es menor que la duración de algún evento
        if (gameManager != null && timer >= gameManager.gameDuration)
        {
            gameEnded = true;  // Marcar que el juego terminó
            CancelShipSpawning();  // Dejar de generar barcos
        }

        // Proceder con el spawn de barcos si no ha terminado el juego
        if (nextEventIndex < schedule.Count && timer >= schedule[nextEventIndex].time)
        {
            SpawnShip(schedule[nextEventIndex]);
            nextEventIndex++;
        }
    }

    // Método para cancelar el spawn de barcos (detiene la creación de nuevos barcos)
    void CancelShipSpawning()
    {
        // Detener la aparición de nuevos barcos
        schedule.Clear();  // Limpiar el plan de aparición de barcos
    }

    // Método para eliminar todos los barcos cuando termine el juego
    public void RemoveAllShips()
    {
        foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
        {
            Destroy(ship);  // Eliminar todos los barcos del escenario
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

        // Instanciar el prefab y ajustar el tamaño
        GameObject indicator = Instantiate(circleIndicatorPrefab, ship.transform.position, Quaternion.identity);
        indicator.transform.SetParent(ship.transform);

        float xOffset = 0f;
        if (spawnEvent.lane == 1)
            xOffset = 3f;  // derecha
        else if (spawnEvent.lane == 2)
            xOffset = -3f; // izquierda
        else if (spawnEvent.lane == 3)
            xOffset = 5f;

        indicator.transform.localPosition = new Vector3(xOffset, 3f, 0f);

        // Aumentamos el tamaño del radio
        indicator.transform.localScale = new Vector3(radius * 1.1f, radius * 2.8f, radius * 2.8f);  // Ajustar el tamaño del círculo (1.5x más grande)

        indicator.SetActive(false);

        // Guardamos una referencia para controlarla más tarde
        shipScript.indicatorCircle = indicator;
    }

    float GetIndicatorRadius(int sizeIndex, GameObject ship)
    {
        // Usamos la escala del barco para calcular el radio del círculo.
        float shipSize = ship.transform.localScale.x;  // Asumiendo que la escala es uniforme (X, Y, Z)

        // Ajustamos el radio del círculo según el tamaño del barco
        return shipSize * 0.5f;  // Por ejemplo, mitad del tamaño del barco
    }


}










