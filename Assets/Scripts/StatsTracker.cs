using System.Collections.Generic;
using UnityEngine;

public class StatsTracker : MonoBehaviour
{
    public static StatsTracker Instance;

    private int piratesEliminated = 0;
    private int fishingEliminated = 0;
    private int currentPirateStreak = 0;
    private int bestPirateStreak = 0;

    private float lastFishingEliminatedTime = -1f;
    private float maxTimeWithoutFishing = 0f;

    private List<float> pirateSinkTimes = new List<float>();
    private float shortestPirateSinkTime = float.MaxValue;

    private float gameStartTime;

    void Awake()
    {
        Instance = this;
        gameStartTime = Time.timeSinceLevelLoad;
        ResetAll();
    }

    public void RegisterShipElimination(bool isPirate, float spawnTime)
    {
        float currentTime = Time.timeSinceLevelLoad;

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

            if (lastFishingEliminatedTime >= 0f)
            {
                float interval = currentTime - lastFishingEliminatedTime;
                maxTimeWithoutFishing = Mathf.Max(maxTimeWithoutFishing, interval);
            }
            else
            {
                float interval = currentTime - gameStartTime;
                maxTimeWithoutFishing = interval;
            }

            lastFishingEliminatedTime = currentTime;
            currentPirateStreak = 0; // Reset pirate streak on fishing elimination
        }
    }

    // Métodos para obtener estadísticas
    public int GetPiratesEliminated() => piratesEliminated;
    public int GetFishingEliminated() => fishingEliminated;
    public int GetBestPirateStreak() => bestPirateStreak;

    // Tiempo más rápido para hundir un pirata
    public float GetShortestTimeToSinkPirate()
    {
        return pirateSinkTimes.Count == 0 ? 0f : shortestPirateSinkTime;
    }

    // Promedio del tiempo para hundir un pirata
    public float GetAverageTimeToSinkPirate()
    {
        if (pirateSinkTimes.Count == 0) return 0f;

        float total = 0f;
        foreach (var time in pirateSinkTimes)
            total += time;

        return total / pirateSinkTimes.Count;
    }

    // Tiempo máximo sin eliminar pesqueros
    public float GetMaxTimeWithoutFishing()
    {
        float now = Time.timeSinceLevelLoad;

        if (fishingEliminated == 0)
            return now - gameStartTime; // Si nunca se eliminó un pesquero, devuelve el tiempo transcurrido desde el inicio del juego

        float sinceLastFishing = now - lastFishingEliminatedTime;
        return Mathf.Max(maxTimeWithoutFishing, sinceLastFishing); // Compara el máximo entre el último intervalo y el histórico
    }

    // Reinicia todas las estadísticas
    public void ResetAll()
    {
        piratesEliminated = 0;
        fishingEliminated = 0;
        currentPirateStreak = 0;
        bestPirateStreak = 0;
        lastFishingEliminatedTime = -1f;
        maxTimeWithoutFishing = 0f;
        pirateSinkTimes.Clear();
        shortestPirateSinkTime = float.MaxValue;
        gameStartTime = Time.timeSinceLevelLoad;
    }
}






