#if WAVE_SDK_IMPORTED

using UnityEngine;
using UnityEngine.SceneManagement;

public class CannonballShip : MonoBehaviour
{
    [HideInInspector] public Ship targetShip; // El barco objetivo

    void OnCollisionEnter(Collision collision)
    {
        GameObject hitObj = collision.gameObject;

        // 1. Si colisionamos con un barco válido
        if (hitObj.CompareTag("Ship"))
        {
            Ship ship = hitObj.GetComponent<Ship>();
            if (ship != null && !ship.IsSinking())
            {
                ship.Sink(); // Hundir el barco
            }

            Destroy(gameObject); // Destruir la bala
        }

        // 2. Si colisionamos con un botón
        else if (hitObj.CompareTag("Boton"))
        {
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
            Destroy(gameObject); // Destruir la bala tras interactuar
        }
    }
}

#endif
