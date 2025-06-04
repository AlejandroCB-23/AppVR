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

    private float captureInterval = 0.015f;
    private float lastCaptureTime = 0f;

    private EyeVergenceEvent currentEvent = null;
    private List<EyeVergenceEvent> completedEvents = new List<EyeVergenceEvent>();

    private string eyeLogPath;
    private string statsPath; 

    private int currentGameNumber;

    private float lastSaveTime = 0f;
    private float saveInterval = 5f; 

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        eyeLogPath = Path.Combine(dataPath, $"VergenceEvents_Game{currentGameNumber}.json");
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

        if (Time.time - lastSaveTime > saveInterval)
        {
            lastSaveTime = Time.time;
            _ = SaveEventsAsync();
        }
    }

    void CaptureEyeTrackingData()
    {
        if (!VergenceFunctions.TryGetInterpupillaryDistance(out float interpupillaryDistance))
            return;

        if (!VergenceFunctions.TryGetCombinedEyeRay(out Ray ray))
            return;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            string stimulusName = hit.collider.gameObject.name;
            string stimulusType = ClassifyStimulus(stimulusName);

            float distance = Vector3.Distance(ray.origin, hit.point);
            float vergenceAngle = VergenceFunctions.CalculateVergenceAngle(interpupillaryDistance, distance);
            float currentTime = Time.time;

            if (currentEvent != null && currentEvent.stimulus == stimulusName)
            {
                currentEvent.vergenceSamples.Add(new VergenceSample(currentTime, vergenceAngle));
                currentEvent.endTime = currentTime;
            }
            else
            {
                if (currentEvent != null)
                {
                    completedEvents.Add(currentEvent);
                }

                currentEvent = new EyeVergenceEvent
                {
                    stimulus = stimulusName,
                    type = stimulusType,
                    wasShot = false,
                    startTime = currentTime,
                    endTime = currentTime,
                    vergenceSamples = new List<VergenceSample> { new VergenceSample(currentTime, vergenceAngle) }
                };
            }
        }
        else
        {
            if (currentEvent != null)
            {
                completedEvents.Add(currentEvent);
                currentEvent = null;
            }
        }
    }

    private string ClassifyStimulus(string name)
    {
        if (name.StartsWith("ship-pirate-small") || name.StartsWith("ship-pirate-medium") || name.StartsWith("ship-pirate-large"))
            return "Go";

        if (name.StartsWith("ship-small") || name.StartsWith("ship-medium") || name.StartsWith("ship-large"))
            return "NoGo";

        if (name.StartsWith("Water") || name.StartsWith("ship-large-health"))
            return "Other";

        return "Unknown";
    }

    public void MarkShot()
    {
        if (currentEvent != null)
        {
            currentEvent.wasShot = true;
        }
    }

    public async Task SaveEventsAsync()
    {
        try
        {
            if (currentEvent != null)
            {
                completedEvents.Add(currentEvent);
                currentEvent = null;
            }

            if (completedEvents.Count == 0) return;

            List<EyeVergenceEvent> existingEvents = new List<EyeVergenceEvent>();
            if (File.Exists(eyeLogPath))
            {
                string existingJson = await File.ReadAllTextAsync(eyeLogPath);
                existingEvents = JsonUtilityWrapper.FromJsonArray<EyeVergenceEvent>(existingJson);
            }

            existingEvents.AddRange(completedEvents);
            completedEvents.Clear();

            string json = JsonUtilityWrapper.ToJsonArray(existingEvents, true);
            await File.WriteAllTextAsync(eyeLogPath, json);

            Debug.Log($"Saved {existingEvents.Count} vergence events.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving vergence events: " + e.Message);
        }
    }

    public async Task SaveFinalStatsAsync()
    {
        await SaveEventsAsync();

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
                avgTimeToSinkPirate = stats.GetAverageTimeToSinkPirate(),
                piratesEscaped = stats.GetPiratesEscaped()
            };



            List<GameStats> allStats = new List<GameStats>();

            if (File.Exists(statsPath))
            {
                string existingJson = await File.ReadAllTextAsync(statsPath);
                allStats = JsonUtilityWrapper.FromJsonArray<GameStats>(existingJson);
            }

            allStats.Add(gameStats);

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

[Serializable]
public class EyeVergenceEvent
{
    public string stimulus;
    public string type;
    public bool wasShot;
    public float startTime;
    public float endTime;
    public List<VergenceSample> vergenceSamples;
}

[Serializable]
public class VergenceSample
{
    public float time;
    public float vergence;

    public VergenceSample(float t, float v)
    {
        time = t;
        vergence = v;
    }
}

[Serializable]
public class GameStats
{
    public int piratesEliminated;
    public int fishingEliminated;
    public int bestPirateStreak;
    public float maxTimeWithoutFishing;
    public float shortestTimeToSinkPirate;
    public float avgTimeToSinkPirate;
    public int piratesEscaped;
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
        if (string.IsNullOrEmpty(json)) return new List<T>();
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper?.Items ?? new List<T>();
    }
}

#endif








