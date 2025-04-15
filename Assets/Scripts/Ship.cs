using UnityEngine;
using System.Collections;


public class Ship : MonoBehaviour
{
    private float speed;
    private Vector3 destination;
    private bool isSinking = false;

    private Material shipMaterial;
    private Color originalColor;

    public GameObject indicatorCircle;

    public void Initialize(bool pirate, float customSpeed)
    {
        speed = customSpeed;
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            shipMaterial = rend.material;
            originalColor = shipMaterial.color;
        }
    }

    public void SetDestination(Vector3 dest)
    {
        destination = dest;
    }

    public void Highlight(bool active)
    {
        if (indicatorCircle != null)
        {
            indicatorCircle.SetActive(active);  // Mostrar u ocultar el círculo
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

        // Desactivar todos los colliders del barco
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    public bool IsSinking()
    {
        return isSinking;
    }

}





