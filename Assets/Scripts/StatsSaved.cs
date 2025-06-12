#if WAVE_SDK_IMPORTED

using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Android;
using UnityEngine;
using Wave.Essence.Eye;
using System.Threading.Tasks;
using Alex.OcularVergenceLibrary;
using Unity.VisualScripting;
using System.Text;

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

    private bool isSaving = false;
    private bool isFirstWrite = true;

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
        eyeLogPath = Path.Combine(dataPath, $"EyeData_Game{currentGameNumber}.json");
        statsPath = Path.Combine(dataPath, "Stats.json");

        InitializeJsonFile();

        if (EyeManager.Instance != null)
        {
            EyeManager.Instance.EnableEyeTracking = true;
            CheckEyeTrackingAvailability();
        }
    }

    private void InitializeJsonFile()
    {
        if (!File.Exists(eyeLogPath))
        {
            File.WriteAllText(eyeLogPath, "{\"Items\":[]}");
            isFirstWrite = true;
        }
        else
        {
            string content = File.ReadAllText(eyeLogPath);
            isFirstWrite = string.IsNullOrEmpty(content) || content.Trim() == "{\"Items\":[]}";
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

        if (Time.time - lastSaveTime > saveInterval && !isSaving)
        {
            lastSaveTime = Time.time;
            _ = AppendEventsAsync();
        }
    }

    void CaptureEyeTrackingData()
    {
        if (!VergenceFunctions.TryGetInterpupillaryDistance(out float interpupillaryDistance))
            return;

        if (!VergenceFunctions.TryGetCombinedEyeRay(out Ray ray))
            return;

        bool hitCollider = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

        if (hitCollider)
        {
            string stimulusName = hit.collider.gameObject.name;
            string stimulusType = ClassifyStimulus(stimulusName);

            float distance = Vector3.Distance(ray.origin, hit.point);
            float vergenceAngle = VergenceFunctions.CalculateVergenceAngle(interpupillaryDistance, distance);
            float currentTime = Time.time;

            Vector3 combinedOrigin = Vector3.zero;
            Vector3 combinedDirection = Vector3.forward;
            EyeData.TryGetCombinedEyeWorldData(out combinedOrigin, out combinedDirection);

            EyeDataSample eyeDataSample = new EyeDataSample(
                currentTime,
                vergenceAngle,
                distance,
                combinedOrigin,
                combinedDirection
            );

            if (currentEvent != null && currentEvent.stimulus == stimulusName)
            {
                currentEvent.eyeDataSamples.Add(eyeDataSample);
                currentEvent.endTime = currentTime;
            }
            else
            {
                FinalizePreviousEvent();

                currentEvent = new EyeVergenceEvent
                {
                    stimulus = stimulusName,
                    type = stimulusType,
                    wasShot = false,
                    startTime = currentTime,
                    endTime = currentTime,
                    eyeDataSamples = new List<EyeDataSample> { eyeDataSample }
                };
            }
        }
        else
        {
            FinalizePreviousEvent();
        }
    }

    private void FinalizePreviousEvent()
    {
        if (currentEvent != null)
        {
            if (currentEvent.eyeDataSamples != null && currentEvent.eyeDataSamples.Count > 0)
            {
                completedEvents.Add(currentEvent);
                Debug.Log($"Evento finalizado: {currentEvent.stimulus} con {currentEvent.eyeDataSamples.Count} muestras");
                _ = AppendEventsAsync();
            }
            currentEvent = null;
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

    public async Task AppendEventsAsync()
    {
        if (isSaving || completedEvents.Count == 0) return;

        try
        {
            isSaving = true;

            StringBuilder jsonBuilder = new StringBuilder();

            for (int i = 0; i < completedEvents.Count; i++)
            {
                string eventJson = JsonUtility.ToJson(completedEvents[i]);

                if (!isFirstWrite || i > 0)
                {
                    jsonBuilder.Append(",");
                }

                jsonBuilder.Append(eventJson);
            }

            await AppendToJsonFileAsync(jsonBuilder.ToString());

            isFirstWrite = false;
            completedEvents.Clear();

            Debug.Log($"Eventos guardados directamente por append");
        }
        catch (Exception e)
        {
            Debug.LogError("Error appending vergence events: " + e.Message);
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task AppendToJsonFileAsync(string jsonContent)
    {
        using (FileStream fs = new FileStream(eyeLogPath, FileMode.Open, FileAccess.ReadWrite))
        {
            fs.Seek(-2, SeekOrigin.End);

            byte[] contentBytes = Encoding.UTF8.GetBytes(jsonContent + "]}");
            await fs.WriteAsync(contentBytes, 0, contentBytes.Length);
            await fs.FlushAsync();
        }
    }

    public async Task SaveFinalStatsAsync()
    {
        FinalizePreviousEvent();
        await AppendEventsAsync();

        try
        {
            var stats = StatsTracker.Instance;
            var gameStats = new GameStats
            {
                gameNumber = currentGameNumber,
                piratesEliminated = stats.GetPiratesEliminated(),
                fishingEliminated = stats.GetFishingEliminated(),
                bestPirateStreak = stats.GetBestPirateStreak(),
                maxTimeWithoutFishing = stats.GetMaxTimeWithoutFishing(),
                shortestTimeToSinkPirate = stats.GetShortestTimeToSinkPirate(),
                avgTimeToSinkPirate = stats.GetAverageTimeToSinkPirate(),
                piratesEscaped = stats.GetPiratesEscaped()
            };

            await AppendStatsAsync(gameStats);

            currentGameNumber++;
            PlayerPrefs.SetInt("GameNumber", currentGameNumber);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving final stats: " + e.Message);
        }
    }

    private async Task AppendStatsAsync(GameStats gameStats)
    {
        string statsJson = JsonUtility.ToJson(gameStats);

        if (!File.Exists(statsPath))
        {
            File.WriteAllText(statsPath, "{\"Items\":[" + statsJson + "]}");
        }
        else
        {
            using (FileStream fs = new FileStream(statsPath, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.Seek(-2, SeekOrigin.End); 
                byte[] contentBytes = Encoding.UTF8.GetBytes("," + statsJson + "]}");
                await fs.WriteAsync(contentBytes, 0, contentBytes.Length);
                await fs.FlushAsync();
            }
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

public class SimpleAppendDataCollector : MonoBehaviour
{
    private string eyeLogPath;

    void Start()
    {
        eyeLogPath = Path.Combine(Application.persistentDataPath, $"EyeData_Simple.txt");
    }

    public async Task AppendEventSimpleAsync(EyeVergenceEvent evt)
    {
        try
        {
            string eventLine = JsonUtility.ToJson(evt) + Environment.NewLine;

            // Escribir línea directamente al final del archivo
            using (StreamWriter writer = new StreamWriter(eyeLogPath, append: true))
            {
                await writer.WriteLineAsync(eventLine);
                await writer.FlushAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error appending event: " + e.Message);
        }
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
    public List<EyeDataSample> eyeDataSamples;
}

[Serializable]
public class EyeDataSample
{
    public float time;
    public float vergence;
    public float distanceToTarget;
    public Vector3 combinedEyeOrigin;
    public Vector3 combinedEyeDirection;

    public EyeDataSample(float t, float v, float distance, Vector3 origin, Vector3 direction)
    {
        time = t;
        vergence = v;
        distanceToTarget = distance;
        combinedEyeOrigin = origin;
        combinedEyeDirection = direction;
    }
}

[Serializable]
public class GameStats
{
    public int gameNumber;
    public int piratesEliminated;
    public int fishingEliminated;
    public int bestPirateStreak;
    public float maxTimeWithoutFishing;
    public float shortestTimeToSinkPirate;
    public float avgTimeToSinkPirate;
    public int piratesEscaped;
}

#endif









