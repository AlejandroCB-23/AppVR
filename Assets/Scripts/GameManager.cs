#if WAVE_SDK_IMPORTED

using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public float gameDuration = 120f;
    private float timer;
    private bool gameEnded = false;

    public GameObject endStatsCanvas;
    public GameObject Timer;
    public float delayBeforeShowingStats = 1f;

    [Header("Tic Tac Sound")]
    public AudioClip ticTacClip;
    private AudioSource ticTacSource;
    private bool ticTacStarted = false;

    [Header("End Bell Sound")]
    public AudioClip bellClip;
    private AudioSource bellSource;

    public float TimeRemaining => timer;

    [SerializeField] private GazeShipDetector gazeShipDetector;
    public InputAction fireAction;

    private StatsUIManager statsUIManager;


    void Start()
    {
        timer = gameDuration;

        ticTacSource = gameObject.AddComponent<AudioSource>();
        ticTacSource.clip = ticTacClip;
        ticTacSource.loop = true;
        ticTacSource.playOnAwake = false;

        bellSource = gameObject.AddComponent<AudioSource>();
        bellSource.clip = bellClip;
        bellSource.loop = false;
        bellSource.playOnAwake = false;

        if (endStatsCanvas != null)
            endStatsCanvas.SetActive(false);

        if (endStatsCanvas != null)
            statsUIManager = endStatsCanvas.GetComponent<StatsUIManager>();
    }

    void Update()
    {
        if (gameEnded)
            return;

        timer -= Time.deltaTime;

        if (!ticTacStarted && timer <= 30f)
        {
            ticTacStarted = true;
            ticTacSource.Play();  
        }


        if (timer <= 0f)
        {
            timer = 0f;
            gameEnded = true;

            if (ticTacSource.isPlaying)
                ticTacSource.Stop();


            bellSource.Play();


            foreach (var ship in GameObject.FindGameObjectsWithTag("Ship"))
            {
                Destroy(ship);
            }


            Invoke(nameof(ShowEndStats), delayBeforeShowingStats);
            StatsTracker.Instance.gameOver = true;
        }
    }

    void ShowEndStats()
    {
        GameSettings.CurrentShootingMode = GameSettings.DisparoMode.Both;

        Timer.SetActive(false);

        if (endStatsCanvas != null)
        {
            endStatsCanvas.SetActive(true);

            if (gazeShipDetector != null)
            {
                gazeShipDetector.EnableControls();
            }

            if (statsUIManager != null)
            {
                statsUIManager.UpdateStats(
                    StatsTracker.Instance.GetPiratesEliminated(),
                    StatsTracker.Instance.GetFishingEliminated(),
                    StatsTracker.Instance.GetBestPirateStreak(),
                    StatsTracker.Instance.GetMaxTimeWithoutFishing(),
                    StatsTracker.Instance.GetShortestTimeToSinkPirate(),
                    StatsTracker.Instance.GetAverageTimeToSinkPirate(),
                    StatsTracker.Instance.GetPiratesEscaped()
                );
            }
        }
    }
}
#endif




