using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    private GameManager gameManager;
    private bool TimeSoldOutDisplayed = false;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        counterText.color = normalColor;
    }

    void Update()
    {
        float time = Mathf.Max(gameManager.TimeRemaining, 0f);

        if (time <= 0f && !TimeSoldOutDisplayed)
        {
            counterText.text = "Time\nexpired";
            counterText.color = warningColor;
            TimeSoldOutDisplayed = true;
            return;
        }

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        // Change to warning color if time is below 30 seconds
        counterText.color = time <= 30f ? warningColor : normalColor;

        counterText.text = $"{minutes} min\n{seconds:00} Sec";
    }
}



