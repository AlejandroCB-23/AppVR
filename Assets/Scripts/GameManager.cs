using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public float gameDuration = 120f;  // Duraci�n del juego
    private float timer;  // Temporizador del juego
    private bool gameEnded = false;  // Estado del juego (si ha terminado)

    public GameObject endStatsCanvas; // Canvas que se activa al final
    public GameObject Timer;  // Referencia al temporizador en pantalla
    public float delayBeforeShowingStats = 1f;  // Retraso para mostrar las estad�sticas

    public float TimeRemaining => timer;  // Propiedad para obtener el tiempo restante

    // Referencias a controles
    [SerializeField] private GazeShipDetector gazeShipDetector;
    public InputAction fireAction; // Acci�n del gatillo

    // Referencia al StatsUIManager
    private StatsUIManager statsUIManager;

    void Start()
    {
        timer = gameDuration;
        if (endStatsCanvas != null)
            endStatsCanvas.SetActive(false); // Ocultar el canvas de estad�sticas al inicio

        // Obtener la referencia al StatsUIManager dentro del canvas
        if (endStatsCanvas != null)
        {
            statsUIManager = endStatsCanvas.GetComponent<StatsUIManager>();
        }
    }

    void Update()
    {
        if (gameEnded)
            return;  // Si el juego termin�, no hacer nada m�s

        timer -= Time.deltaTime;  // Reducir el tiempo del temporizador

        if (timer <= 0f)
        {
            timer = 0f;  // Asegurarnos de que el temporizador no se vuelva negativo
            gameEnded = true;

            foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
            {
                Destroy(ship);
            }

            Invoke(nameof(ShowEndStats), delayBeforeShowingStats);  // Mostrar las estad�sticas despu�s de un peque�o retraso
        }
    }

    void ShowEndStats()
    {
        GameSettings.CurrentShootingMode = GameSettings.DisparoMode.Both;

        Timer.SetActive(false); // Desactivar el temporizador

        // Mostrar el canvas de estad�sticas
        if (endStatsCanvas != null)
        {
            endStatsCanvas.SetActive(true);  // Activar el canvas de estad�sticas

            // Habilitar los controles de disparo para que el jugador pueda interactuar con los botones
            if (gazeShipDetector != null)
            {
                gazeShipDetector.EnableControls();
            }

            // Verificar que statsUIManager no sea null antes de actualizar las estad�sticas
            if (statsUIManager != null)
            {
                // Actualizar las estad�sticas en la UI usando StatsUIManager
                statsUIManager.UpdateStats(
                    StatsTracker.Instance.GetPiratesEliminated(),
                    StatsTracker.Instance.GetFishingEliminated(),
                    StatsTracker.Instance.GetBestPirateStreak(),
                    StatsTracker.Instance.GetMaxTimeWithoutFishing(),
                    StatsTracker.Instance.GetShortestTimeToSinkPirate(),
                    StatsTracker.Instance.GetAverageTimeToSinkPirate()
                );
            }
        }
    }



}






