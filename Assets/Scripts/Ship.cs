#if WAVE_SDK_IMPORTED

using UnityEngine;

public class Ship : MonoBehaviour
{
    private float speed;
    private Vector3 destination;
    private bool isSinking = false;

    private bool isPirate;
    private float spawnTime;

    public GameObject indicatorCircle;

    public void Initialize(bool pirate, float customSpeed)
    {
        speed = customSpeed;
        isPirate = pirate;
        spawnTime = Time.timeSinceLevelLoad; 
    }

    public void SetDestination(Vector3 dest)
    {
        destination = dest;
    }

    public void Highlight(bool active)
    {
        if (indicatorCircle != null)
        {
            indicatorCircle.SetActive(active);
        }
    }

    void Update()
    {
        if (isSinking) return;

        if (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            Vector3 direction = (destination - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Sink()
    {
        isSinking = true;
        gameObject.AddComponent<ShipSink>();

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;

        if (StatsTracker.Instance != null)
        {
            StatsTracker.Instance.RegisterShipElimination(isPirate, spawnTime);
        }
    }

    public bool IsSinking()
    {
        return isSinking;
    }
}
#endif




