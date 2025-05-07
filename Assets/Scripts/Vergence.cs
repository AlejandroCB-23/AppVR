#if WAVE_SDK_IMPORTED

using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Android;
using UnityEngine;
using Wave.Essence.Eye;
using System.Threading.Tasks;
using Alex.OcularVergenceLibrary;

public class EyeDataCollector : MonoBehaviour
{
    public static EyeDataCollector Instance { get; private set; }

    private Queue<EyeLogEntry> logQueue = new Queue<EyeLogEntry>();
    private string eyeLogPath;
    private string statsPath;
    private int currentGameNumber;

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

        if (!PlayerPrefs.HasKey("GameNumber"))
        {
            PlayerPrefs.SetInt("GameNumber", 1);
            PlayerPrefs.Save();
        }

        currentGameNumber = PlayerPrefs.GetInt("GameNumber", 1);
        eyeLogPath = Path.Combine(dataPath, $"EyeLog_Game{currentGameNumber}.json");
        statsPath = Path.Combine(dataPath, "Stats.json"); 

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
        if (VergenceFunctions.TryGetInterpupillaryDistance(out float interpupillaryDistance))
        {
            if (VergenceFunctions.TryGetCombinedEyeRay(out Ray ray))
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    float distance = Vector3.Distance(ray.origin, hit.point);
                    float vergence = VergenceFunctions.CalculateVergenceAngle(interpupillaryDistance, distance);

                    string objectName = hit.collider.gameObject.name;

                    logQueue.Enqueue(new EyeLogEntry(objectName, distance, vergence));
                    _ = Task.Run(async () => await SaveLogAsync());
                }
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
            await File.AppendAllTextAsync(eyeLogPath, jsonLine + "\n");
        }
    }

    public async Task SaveFinalStatsAsync()
    {
        try
        {
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

            List<GameStats> allStats = new List<GameStats>();

            // Leer las estadísticas existentes, si existen, para mantener un único archivo
            if (File.Exists(statsPath))
            {
                string existingJson = await File.ReadAllTextAsync(statsPath);
                allStats = JsonUtilityWrapper.FromJsonArray<GameStats>(existingJson);
            }

            // Agregar la nueva estadística
            allStats.Add(gameStats);

            // Convertir la lista a JSON y guardar el archivo
            string updatedStatsJson = JsonUtilityWrapper.ToJsonArray(allStats, true);
            await File.WriteAllTextAsync(statsPath, updatedStatsJson);

            currentGameNumber++;
            PlayerPrefs.SetInt("GameNumber", currentGameNumber);
            PlayerPrefs.Save();

            Debug.Log("Game stats and eye logs saved.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving final stats: " + e.Message);
        }
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

public static class JsonUtilityWrapper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }

    public static string ToJsonArray<T>(List<T> list, bool prettyPrint = false)
    {
        Wrapper<T> wrapper = new Wrapper<T> { Items = list };
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    public static List<T> FromJsonArray<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items ?? new List<T>();
    }
}

#endif






