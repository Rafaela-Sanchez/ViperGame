using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    const int MaxLives = 3;
    const string HighScoreKey = "HighScore";

    public Snake snake;
    public GameObject hud, hint, menuPanel, pausePanel, resultPanel;
    public Text scoreText, highScoreText, menuHighScoreText, resultTitleText, resultScoreText;
    public Image[] hearts;
    public AudioSource music, sfx;
    public AudioClip loseLifeClip, gameOverClip, winClip;

    int score, lives, highScore;
    bool playing;

    void Start()
    {
        snake.AteFood += () => { score++; UpdateHud(); };
        snake.HitSelf += LoseLife;
        snake.Won += () => { score++; End("VOCÊ VENCEU!", winClip); };
        highScore = PlayerPrefs.GetInt(HighScoreKey);
        snake.NewGame();
        ShowMenu();
    }

    void Update()
    {
        hint.SetActive(playing && snake.enabled && snake.Waiting);
        var k = Keyboard.current;
        if (playing && k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame)) SetPaused(snake.enabled);
    }

    public void Play()
    {
        score = 0;
        lives = MaxLives;
        snake.NewGame();
        menuPanel.SetActive(false);
        resultPanel.SetActive(false);
        hud.SetActive(true);
        playing = true;
        SetPaused(false);
        music.Play();
        UpdateHud();
    }

    public void Resume() => SetPaused(false);

    public void ShowMenu()
    {
        playing = false;
        snake.enabled = false;
        hud.SetActive(false);
        pausePanel.SetActive(false);
        resultPanel.SetActive(false);
        menuPanel.SetActive(true);
        menuHighScoreText.text = "Recorde: " + highScore;
        music.Stop();
    }

    void SetPaused(bool paused)
    {
        pausePanel.SetActive(paused);
        snake.enabled = !paused;
    }

    void LoseLife()
    {
        lives--;
        UpdateHud();
        if (lives == 0)
        {
            End("GAME OVER", gameOverClip);
            return;
        }
        sfx.PlayOneShot(loseLifeClip);
        snake.Respawn();
    }

    void End(string title, AudioClip clip)
    {
        playing = false;
        snake.enabled = false;
        music.Stop();
        sfx.PlayOneShot(clip);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, score);
            PlayerPrefs.Save();
        }
        UpdateHud();
        resultTitleText.text = title;
        resultScoreText.text = $"Pontos: {score}\nRecorde: {highScore}";
        resultPanel.SetActive(true);
    }

    void UpdateHud()
    {
        scoreText.text = "Pontos: " + score;
        highScoreText.text = "Recorde: " + Mathf.Max(score, highScore);
        for (int i = 0; i < hearts.Length; i++) hearts[i].enabled = i < lives;
    }
}
