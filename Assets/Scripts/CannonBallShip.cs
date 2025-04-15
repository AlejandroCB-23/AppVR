#if WAVE_SDK_IMPORTED

using UnityEngine;

public class CannonballShip : MonoBehaviour
{
    [HideInInspector] public Ship targetShip; // El barco objetivo

    void OnCollisionEnter(Collision collision)
    {
        GameObject hitObj = collision.gameObject;

        // Verificamos que el objeto tenga la etiqueta "Ship"
        if (hitObj.CompareTag("Ship"))
        {
            Ship ship = hitObj.GetComponent<Ship>();
            if (ship != null)
            {
                ship.Sink(); // Hundir el barco
            }

            Destroy(gameObject); // Destruir la bala
        }
    }
}

#endif
