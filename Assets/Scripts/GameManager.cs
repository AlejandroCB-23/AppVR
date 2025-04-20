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

    public float TimeRemaining => timer;  

    
    [SerializeField] private GazeShipDetector gazeShipDetector;
    public InputAction fireAction; 

   
    private StatsUIManager statsUIManager;

    void Start()
    {
        timer = gameDuration;
        if (endStatsCanvas != null)
            endStatsCanvas.SetActive(false); 

        if (endStatsCanvas != null)
        {
            statsUIManager = endStatsCanvas.GetComponent<StatsUIManager>();
        }
    }

    void Update()
    {
        if (gameEnded)
            return;  

        timer -= Time.deltaTime;  

        if (timer <= 0f)
        {
            timer = 0f;  
            gameEnded = true;

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
                    StatsTracker.Instance.GetAverageTimeToSinkPirate()
                );
            }
        }
    }



}
#endif



