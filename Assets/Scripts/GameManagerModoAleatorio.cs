#if WAVE_SDK_IMPORTED

using UnityEngine;

public class GameManagerModoAleatorio : MonoBehaviour
{
    public GameObject endStatsCanvas;
    public GameObject gameOverPanel; // Nuevo: Referencia al panel GameOver
    public float delayBeforeShowingStats = 3f;

    [Header("End Bell Sound")]
    public AudioClip bellClip;
    private AudioSource bellSource;

    private bool gameEnded = false;
    private StatsUIManager statsUIManager;

    void Start()
    {
        bellSource = gameObject.AddComponent<AudioSource>();
        bellSource.clip = bellClip;
        bellSource.loop = false;
        bellSource.playOnAwake = false;

        if (endStatsCanvas != null)
        {
            endStatsCanvas.SetActive(false);
            statsUIManager = endStatsCanvas.GetComponent<StatsUIManager>();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // Ocultamos el panel por defecto
    }

    void Update()
    {
        if (gameEnded) return;

        if (StatsTracker.Instance.GetFishingEliminatedAleatorio() >= 3)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        StatsTracker.Instance.gameOver = true;

        // Mostrar panel GameOver
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Reproducir campana
        if (bellSource != null)
            bellSource.Play();

        // Eliminar todos los barcos restantes
        foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
        {
            Destroy(ship);
        }

        // Después del delay, mostrar estadísticas
        Invoke(nameof(ShowEndStats), delayBeforeShowingStats);
    }

    void ShowEndStats()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // Ocultar GameOver panel

        if (endStatsCanvas != null)
        {
            endStatsCanvas.SetActive(true);

            statsUIManager?.UpdateStats(
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
#endif



