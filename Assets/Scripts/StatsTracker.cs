using UnityEngine;
using System.Collections.Generic;

public class StatsTracker : MonoBehaviour
{
    public static StatsTracker Instance;

    private int piratesEliminated = 0;
    private int fishingEliminated = 0;
    private int currentPirateStreak = 0;
    private int bestPirateStreak = 0;
    private float lastFishingEliminatedTime = 0f;
    private float maxTimeWithoutFishing = 0f;
    private List<float> pirateSinkTimes = new List<float>();
    private float shortestPirateSinkTime = float.MaxValue;

    void Awake()
    {
        // Sólo una instancia en esta escena: se sobrescribe cada vez que cargues ModoTest
        Instance = this;
    }

    public void RegisterShipElimination(bool isPirate, float spawnTime)
    {
        float currentTime = Time.time;

        if (isPirate)
        {
            piratesEliminated++;
            currentPirateStreak++;
            bestPirateStreak = Mathf.Max(bestPirateStreak, currentPirateStreak);

            float sinkTime = currentTime - spawnTime;
            pirateSinkTimes.Add(sinkTime);
            if (sinkTime < shortestPirateSinkTime)
                shortestPirateSinkTime = sinkTime;
        }
        else
        {
            fishingEliminated++;
            float timeSinceLast = currentTime - lastFishingEliminatedTime;
            if (lastFishingEliminatedTime > 0)
                maxTimeWithoutFishing = Mathf.Max(maxTimeWithoutFishing, timeSinceLast);
            lastFishingEliminatedTime = currentTime;
            currentPirateStreak = 0;
        }
    }

    // Getters…
    public int GetPiratesEliminated() => piratesEliminated;
    public int GetFishingEliminated() => fishingEliminated;
    public int GetBestPirateStreak() => bestPirateStreak;
    public float GetMaxTimeWithoutFishing() => maxTimeWithoutFishing;
    public float GetShortestTimeToSinkPirate() => shortestPirateSinkTime == float.MaxValue ? 0f : shortestPirateSinkTime;
    public float GetAverageTimeToSinkPirate() => pirateSinkTimes.Count > 0 ? Average(pirateSinkTimes) : 0f;

    private float Average(List<float> values)
    {
        float sum = 0f;
        foreach (var v in values)
            sum += v;
        return sum / values.Count;
    }

    public void ResetAll()
    {
        piratesEliminated = 0;
        fishingEliminated = 0;
        currentPirateStreak = 0;
        bestPirateStreak = 0;
        lastFishingEliminatedTime = 0f;
        maxTimeWithoutFishing = 0f;
        pirateSinkTimes.Clear();
        shortestPirateSinkTime = float.MaxValue;
    }

}


