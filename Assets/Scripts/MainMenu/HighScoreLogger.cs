using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HighScoreLogger : MonoBehaviour
{
    internal static HighScoreLogger instance;
    [SerializeField] internal int gameMode = -1;

    internal string[] highScoreStrings = {"GM0HighScore", "GM1HighScore", "GM2HighScore", "GM3HighScore", "GM4HighScore", "GM5HighScore", "GM6HighScore", "GM7HighScore"};  //{Optional: rename}

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public int[] GetHighScores(bool isIncludingOverallHighScore)
    {
        int[] highScores;
        if (isIncludingOverallHighScore)
        {
            highScores = new int[highScoreStrings.Length + 1];
        }
        else
        {
            highScores = new int[highScoreStrings.Length];
        }
        for (int i = 0; i < highScoreStrings.Length; i++)
        {
            highScores[i] = PlayerPrefs.GetInt(highScoreStrings[i], 0);
        }
        if (isIncludingOverallHighScore)
        {
            highScores[highScores.Length - 1] = GetOverallHighScore();
        }
        return highScores;
    }

    public int GetOverallHighScore()
    {
        return GetHighScores(false).Sum();
    }

    public void UpdateHighScore(int newScore, bool isUpdatingToNewScore)
    {
        int highScore = PlayerPrefs.GetInt(highScoreStrings[gameMode], 0);
        if (SceneManager.GetActiveScene().buildIndex == Constants.gameSceneBuildIndex)
        {
            FindObjectOfType<SpawnPeople>().UpdateUnlockedModeText(highScore);
        }

        if ((newScore > highScore) || isUpdatingToNewScore)
        {
            PlayerPrefs.SetInt(highScoreStrings[gameMode], newScore);
            Debug.Log(highScoreStrings[gameMode] + " changed from " + highScore + " to " + newScore);
        }
    }

    public void ResetHighScores()
    {
        for (int i = 0; i < highScoreStrings.Length; i++)
        {
            PlayerPrefs.SetInt(highScoreStrings[i], 0);
        }
        Debug.Log("High scores reset!");
    }

    public void UnlockAllGameModes(int value)
    {
        PlayerPrefs.SetInt("AreAllGameModesUnlocked", value);  // Restart needed if switching setting from 1 to 0
    }
}