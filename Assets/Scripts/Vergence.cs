#if WAVE_SDK_IMPORTED
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Android;
using UnityEngine;
using Wave.Essence.Eye;
using System.Threading.Tasks;

public class EyeDataCollector : MonoBehaviour
{
    public static EyeDataCollector Instance { get; private set; }

    private Queue<EyeLogEntry> logQueue = new Queue<EyeLogEntry>();
    private string logFilePath;

    private float captureInterval = 0.015f;
    private float lastCaptureTime = 0f;

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        RequestStoragePermissions();
        string dataPath = Application.persistentDataPath;
        logFilePath = Path.Combine(dataPath, "CombinedGameLog.json");

        if (!PlayerPrefs.HasKey("GameNumber"))
        {
            PlayerPrefs.SetInt("GameNumber", 1);  
            PlayerPrefs.Save();
        }

        if (EyeManager.Instance != null)
        {
            EyeManager.Instance.EnableEyeTracking = true;
            CheckEyeTrackingAvailability();
        }
    }

    void Update()
    {
        if (EyeManager.Instance == null || !EyeManager.Instance.IsEyeTrackingAvailable())
            return;

        if (Time.time - lastCaptureTime >= captureInterval)
        {
            lastCaptureTime = Time.time;
            CaptureEyeTrackingData();
        }
    }

    void CaptureEyeTrackingData()
    {
        Vector3 leftEyeOrigin = Vector3.zero, rightEyeOrigin = Vector3.zero;
        Vector3 combinedEyeOrigin = Vector3.zero, combinedEyeDirection = Vector3.forward;

        bool leftEyeAvailable = EyeManager.Instance.GetLeftEyeOrigin(out leftEyeOrigin);
        bool rightEyeAvailable = EyeManager.Instance.GetRightEyeOrigin(out rightEyeOrigin);
        bool combinedEyeAvailable = EyeManager.Instance.GetCombinedEyeOrigin(out combinedEyeOrigin) &&
                                    EyeManager.Instance.GetCombindedEyeDirectionNormalized(out combinedEyeDirection);

        if (leftEyeAvailable && rightEyeAvailable && combinedEyeAvailable)
        {
            Transform head = Camera.main.transform;

            Vector3 leftTransformed = head.TransformPoint(leftEyeOrigin);
            Vector3 rightTransformed = head.TransformPoint(rightEyeOrigin);
            Vector3 combinedTransformed = head.TransformPoint(combinedEyeOrigin);
            Vector3 combinedDirection = head.TransformDirection(combinedEyeDirection);

            float PD = Vector3.Distance(leftTransformed, rightTransformed);

            RaycastHit hit;
            if (Physics.Raycast(combinedTransformed, combinedDirection, out hit, Mathf.Infinity))
            {
                float distance = Vector3.Distance(combinedTransformed, hit.point);
                float vergence = CalculateVergenceAngle(PD, distance);

                string objectName = hit.collider.gameObject.name;

                logQueue.Enqueue(new EyeLogEntry(objectName, distance, vergence));
                _ = Task.Run(async () => await SaveLogAsync());
            }
        }
    }

    async Task SaveLogAsync()
    {
        List<EyeLogEntry> logsToSave = new List<EyeLogEntry>(logQueue);
        logQueue.Clear();

        foreach (var entry in logsToSave)
        {
            string jsonLine = JsonUtility.ToJson(entry, true); 
            await File.AppendAllTextAsync(logFilePath, jsonLine + "\n");
        }
    }

    public async Task SaveFinalStatsAsync()
    {
        try
        {
            int currentGameNumber = PlayerPrefs.GetInt("GameNumber", 1);

            string header = $"\n{{\n  \"Game\": {currentGameNumber},\n  \"Stats\": ";
            await File.AppendAllTextAsync(logFilePath, header);

            var stats = StatsTracker.Instance;
            var gameStats = new GameStats
            {
                piratesEliminated = stats.GetPiratesEliminated(),
                fishingEliminated = stats.GetFishingEliminated(),
                bestPirateStreak = stats.GetBestPirateStreak(),
                maxTimeWithoutFishing = stats.GetMaxTimeWithoutFishing(),
                shortestTimeToSinkPirate = stats.GetShortestTimeToSinkPirate(),
                avgTimeToSinkPirate = stats.GetAverageTimeToSinkPirate()
            };

            string statsJson = JsonUtility.ToJson(gameStats, true);
            await File.AppendAllTextAsync(logFilePath, statsJson + ",\n  \"EyeLogs\": [\n");

            List<EyeLogEntry> logsToSave = new List<EyeLogEntry>(logQueue);
            logQueue.Clear();

            for (int i = 0; i < logsToSave.Count; i++)
            {
                string logJson = JsonUtility.ToJson(logsToSave[i], true);
                if (i < logsToSave.Count - 1)
                    logJson += ",";

                await File.AppendAllTextAsync(logFilePath, logJson + "\n");
            }

            await File.AppendAllTextAsync(logFilePath, "  ]\n}\n");

            int nextGameNumber = currentGameNumber + 1;
            PlayerPrefs.SetInt("GameNumber", nextGameNumber);
            PlayerPrefs.Save();  

            Debug.Log("Game stats and eye logs saved.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving final stats: " + e.Message);
        }
    }

    float CalculateVergenceAngle(float interpupillaryDistance, float distanceToObject)
    {
        if (distanceToObject == 0f) return 0f;
        return Mathf.Rad2Deg * 2f * Mathf.Atan((interpupillaryDistance / 2f) / distanceToObject);
    }

    void RequestStoragePermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
    }

    void CheckEyeTrackingAvailability()
    {
        if (EyeManager.Instance.IsEyeTrackingAvailable())
            Debug.Log("Eye tracking is available.");
        else
            Debug.LogWarning("Eye tracking is NOT available.");
    }
}

[System.Serializable]
public class GameStats
{
    public int piratesEliminated;
    public int fishingEliminated;
    public int bestPirateStreak;
    public float maxTimeWithoutFishing;
    public float shortestTimeToSinkPirate;
    public float avgTimeToSinkPirate;
}

[System.Serializable]
public class EyeLogEntry
{
    public string objectName;
    public float distanceToObject;
    public float vergenceAngle;

    public EyeLogEntry(string name, float distance, float vergence)
    {
        objectName = name;
        distanceToObject = distance;
        vergenceAngle = vergence;
    }
}
#endif




