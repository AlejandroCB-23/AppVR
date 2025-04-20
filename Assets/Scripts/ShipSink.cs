#if WAVE_SDK_IMPORTED
using UnityEngine;

public class ShipSink : MonoBehaviour
{
    private float sinkSpeed = 3f;
    private float rotationSpeed = 15f;
    private float sinkDepth = 15f; 
    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        //Ship sinking
        transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

        if (transform.position.y < startY - sinkDepth)
        {
            Destroy(gameObject);
        }
    }
}

#endif

