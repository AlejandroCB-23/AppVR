using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    private GameManager gameManager;
    private bool timeExpiredDisplayed = false;
    private float expiredDisplayTimer = 0f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    public float expiredMessageDuration = 2f;

    // Para restaurar estilos originales
    private TextAlignmentOptions originalAlignment;
    private bool originalAutoSize;
    private float originalFontSize;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        counterText.color = normalColor;

        // Guardar estilos originales para restaurarlos después
        originalAlignment = counterText.alignment;
        originalAutoSize = counterText.enableAutoSizing;
        originalFontSize = counterText.fontSize;
    }

    void Update()
    {
        float time = Mathf.Max(gameManager.TimeRemaining, 0f);

        if (time <= 0f && !timeExpiredDisplayed)
        {
            ShowExpiredMessage();
            return;
        }

        if (timeExpiredDisplayed)
        {
            expiredDisplayTimer -= Time.deltaTime;
            if (expiredDisplayTimer > 0f)
            {
                counterText.text = "Tiempo\nAgotado";
                counterText.color = warningColor;
            }
            else
            {
                counterText.text = "";
            }

            return;
        }

        // Restaurar estilos normales si no estamos mostrando el mensaje
        RestoreOriginalStyle();

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        counterText.color = time <= 30f ? warningColor : normalColor;
        counterText.text = $"{minutes} min\n{seconds:00} Sec";
    }

    void ShowExpiredMessage()
    {
        counterText.text = "TiEMPO\nAgotado";
        counterText.color = warningColor;

        // Ajustar el estilo temporal para el mensaje
        counterText.alignment = TextAlignmentOptions.Center;
        counterText.enableAutoSizing = true;
        counterText.fontSizeMin = 30f;
        counterText.fontSizeMax = 60f;

        timeExpiredDisplayed = true;
        expiredDisplayTimer = expiredMessageDuration;
    }

    void RestoreOriginalStyle()
    {
        if (counterText.alignment != originalAlignment)
            counterText.alignment = originalAlignment;

        if (counterText.enableAutoSizing != originalAutoSize)
            counterText.enableAutoSizing = originalAutoSize;

        if (!originalAutoSize)
            counterText.fontSize = originalFontSize;
    }
}






