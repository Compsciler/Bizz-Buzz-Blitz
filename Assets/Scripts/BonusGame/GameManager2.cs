using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using GreatArcStudios;

public class GameManager2 : MonoBehaviour
{
    internal static GameManager2 instance;

    public GameObject[] lines;

    internal bool isGameActive = false;
    internal int score = 0;
    public TMP_Text scoreText;
    public TMP_Text gameOverScoreText;

    public GameObject pauseButton;
    public GameObject gameOverMenu;

    public AudioClip gameOverSound;
    public float gameOverSoundVolume;

    public GameObject[] disableAfterScoring1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void ValidateLineSelection(int lineIndex)
    {
        List<float> distances = new List<float>();
        foreach (GameObject line in lines)
        {
            distances.Add(line.GetComponent<LineController>().distance);
        }
        distances.Sort();
        
        if (lines[lineIndex].GetComponent<LineController>().distance == distances[distances.Count - 1])
        {
            foreach (GameObject line in lines)
            {
                line.GetComponent<LineController>().GenerateRandomLine();
            }
            score++;
            scoreText.text = score.ToString();
            if (score == 1)
            {
                isGameActive = true;
                foreach (GameObject go in disableAfterScoring1)
                {
                    go.SetActive(false);
                }
            }

            Debug.Log("1: " + isGameActive);
            Debug.Log("2: " + instance.isGameActive);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        pauseButton.GetComponent<Button>().interactable = false;

        gameOverMenu.SetActive(true);
        gameOverScoreText.text = "You scored " + score;
        AudioManager.instance.musicSource.Pause();
        AudioManager.instance.SFX_Source.PlayOneShot(gameOverSound, gameOverSoundVolume);

        UpdateHighScore(score, false);
    }

    public void UpdateHighScore(int newScore, bool isUpdatingToNewScore)
    {
        int highScore = PlayerPrefs.GetInt("BonusGameHighScore", 0);

        if ((newScore > highScore) || isUpdatingToNewScore)
        {
            PlayerPrefs.SetInt("BonusGameHighScore", newScore);
            Debug.Log("BonusGameHighScore changed from " + highScore + " to " + newScore);
        }
    }

    public void ResetStaticVariables()
    {
        PauseManager.isPaused = false;
        Time.timeScale = 1;  // Resetting time scale when restarting or quitting game
        Debug.Log("Static variables reset!");
    }

    public void OnSceneUnloaded(Scene currentScene)
    {
        ResetStaticVariables();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;  // Resets delegate
    }
}