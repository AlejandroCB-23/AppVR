using UnityEngine;
using UnityEngine.UI;
using TMPro; // Si estás usando TextMeshPro

public class StatsUIManager : MonoBehaviour
{
    [Header("Referencias a los textos")]
    public TMP_Text piratesEliminatedText;
    public TMP_Text fishingVesselsEliminatedText;
    public TMP_Text pirateStreakText;
    public TMP_Text majorTimeDeleteFishingText;
    public TMP_Text shortestTimeSinkPirateText;
    public TMP_Text timeHalfDeletePirateText;

    public void UpdateStats(int piratesEliminated, int fishingEliminated, int pirateStreak,
                            float maxTimeWithoutFishing, float minTimeToSinkPirate, float avgTimeToSinkPirate)
    {
        piratesEliminatedText.text = $"Piratas Eliminados: {piratesEliminated}";
        fishingVesselsEliminatedText.text = $"Pesqueros Eliminados: {fishingEliminated}";
        pirateStreakText.text = $"Mejor Racha Pirata: {pirateStreak}";

        majorTimeDeleteFishingText.text = $"Mayor Tiempo Sin Eliminar Pesquero:\n{FormatTime(maxTimeWithoutFishing)}";
        shortestTimeSinkPirateText.text = $"Menor Tiempo En Eliminar Pirata:\n{FormatTime(minTimeToSinkPirate)}";
        timeHalfDeletePirateText.text = $"Tiempo Medio En Eliminar Pirata:\n{FormatTime(avgTimeToSinkPirate)}";
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes}:{secs:D2}";
    }
}

