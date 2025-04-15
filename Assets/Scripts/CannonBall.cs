#if WAVE_SDK_IMPORTED

using UnityEngine;


public class Cannonball : MonoBehaviour
{
    [HideInInspector]
    public GameObject targetButton; // El bot?n objetivo detectado previamente
    [HideInInspector]
    public menu.GazeMenuVive menuController;

    void OnCollisionEnter(Collision collision)
    {
        // Comprobamos si la bola colisiona con el bot?n esperado o con un objeto con la tag "Boton"
        if (collision.gameObject == targetButton || collision.gameObject.CompareTag("Boton"))
        {
            // Ejecutamos la acci?n correspondiente al bot?n
            menuController.ExecuteButtonAction(targetButton);
            Destroy(gameObject); // Destruir la bola de ca??n tras la colisi?n
        }
    }
}

#endif




