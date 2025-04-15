using UnityEngine;

public class ShipSink : MonoBehaviour
{
    private float sinkSpeed = 1f;
    private float rotationSpeed = 15f;

    void Update()
    {
        // Hundirse hacia abajo e inclinar el barco lentamente
        transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }
}

