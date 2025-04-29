#if WAVE_SDK_IMPORTED

using UnityEngine;

public class GameManagerModoAleatorio : MonoBehaviour
{
    public GameObject endStatsCanvas;
    public float delayBeforeShowingStats = 1f;

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
    }

    void Update()
    {
        // Verifica si ya se destruyeron 3 barcos pesqueros en modo aleatorio
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

        if (bellSource != null)
            bellSource.Play();

        // Eliminar todos los barcos restantes
        foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
        {
            Destroy(ship);
        }

        Invoke(nameof(ShowEndStats), delayBeforeShowingStats);
    }

    void ShowEndStats()
    {
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


