
#if WAVE_SDK_IMPORTED

using UnityEngine;


public class Cannonball : MonoBehaviour
{
    [HideInInspector]
    public GameObject targetButton; // El botón objetivo detectado previamente
    [HideInInspector]
    public menu.GazeMenuVive menuController;

    void OnCollisionEnter(Collision collision)
    {
        // Comprobamos si la bola colisiona con el botón esperado o con un objeto con la tag "Boton"
        if (collision.gameObject == targetButton || collision.gameObject.CompareTag("Boton"))
        {
            // Ejecutamos la acción correspondiente al botón
            menuController.ExecuteButtonAction(targetButton);
            Destroy(gameObject); // Destruir la bola de cañón tras la colisión
        }
    }
}

#endif