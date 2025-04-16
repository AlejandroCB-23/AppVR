using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public float gameDuration = 120f;  // Duración del juego
    private float timer;  // Temporizador del juego
    private bool gameEnded = false;  // Estado del juego (si ha terminado)

    public GameObject endStatsCanvas; // Canvas que se activa al final
    public GameObject Timer;  // Referencia al temporizador en pantalla
    public float delayBeforeShowingStats = 1f;  // Retraso para mostrar las estadísticas

    public float TimeRemaining => timer;  // Propiedad para obtener el tiempo restante

    // Referencias a controles
    [SerializeField] private GazeShipDetector gazeShipDetector;
    public InputAction fireAction; // Acción del gatillo

    // Referencia al StatsUIManager
    private StatsUIManager statsUIManager;

    void Start()
    {
        timer = gameDuration;
        if (endStatsCanvas != null)
            endStatsCanvas.SetActive(false); // Ocultar el canvas de estadísticas al inicio

        // Obtener la referencia al StatsUIManager dentro del canvas
        if (endStatsCanvas != null)
        {
            statsUIManager = endStatsCanvas.GetComponent<StatsUIManager>();
        }
    }

    void Update()
    {
        if (gameEnded)
            return;  // Si el juego terminó, no hacer nada más

        timer -= Time.deltaTime;  // Reducir el tiempo del temporizador

        if (timer <= 0f)
        {
            timer = 0f;  // Asegurarnos de que el temporizador no se vuelva negativo
            gameEnded = true;

            foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
            {
                Destroy(ship);
            }

            Invoke(nameof(ShowEndStats), delayBeforeShowingStats);  // Mostrar las estadísticas después de un pequeño retraso
        }
    }

    void ShowEndStats()
    {
        GameSettings.CurrentShootingMode = GameSettings.DisparoMode.Both;

        Timer.SetActive(false); // Desactivar el temporizador

        // Mostrar el canvas de estadísticas
        if (endStatsCanvas != null)
        {
            endStatsCanvas.SetActive(true);  // Activar el canvas de estadísticas

            // Habilitar los controles de disparo para que el jugador pueda interactuar con los botones
            if (gazeShipDetector != null)
            {
                gazeShipDetector.EnableControls();
            }

            // Verificar que statsUIManager no sea null antes de actualizar las estadísticas
            if (statsUIManager != null)
            {
                // Actualizar las estadísticas en la UI usando StatsUIManager
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






