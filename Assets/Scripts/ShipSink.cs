using UnityEngine;

public class ShipSink : MonoBehaviour
{
    private float sinkSpeed = 1f;
    private float rotationSpeed = 15f;
    private float sinkDepth = 15f; // Profundidad tras la cual destruir
    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // Hundirse hacia abajo e inclinar el barco lentamente
        transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

        // Si ha bajado lo suficiente, destruirlo
        if (transform.position.y < startY - sinkDepth)
        {
            Destroy(gameObject);
        }
    }
}


