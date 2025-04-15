using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float gameDuration = 120f; // Duración del juego en segundos
    private float timer;

    void Start()
    {
        timer = gameDuration;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        SceneManager.LoadScene("Menu");
    }
}
