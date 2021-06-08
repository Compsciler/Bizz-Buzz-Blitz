using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HighScoreLogger : MonoBehaviour
{
    internal static HighScoreLogger instance;
    [SerializeField] internal int gameMode = -1;

    internal string[] targetHighScoreStrings = {"GM0HighScore", "GM1HighScore", "GM2HighScore", "GM3HighScore", "GM4HighScore", "GM5HighScore", "GM6HighScore", "GM7HighScore", "GM8HighScore"};
    internal string[] endlessHighScoreStrings = {"GM50HighScore", "GM51HighScore", "GM52HighScore", "GM53HighScore", "GM54HighScore", "GM55HighScore", "GM56HighScore", "GM57HighScore", "GM58HighScore"};
    internal int endlessGameModeMinNum = 50;

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

    public int[] GetHighScores(bool isEndlessMode, bool isIncludingOverallHighScore)
    {
        int[] highScores;
        string[] highScoreStrings;
        if (isEndlessMode)
        {
            highScoreStrings = endlessHighScoreStrings;
        }
        else
        {
            highScoreStrings = targetHighScoreStrings;
        }
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
            highScores[highScores.Length - 1] = GetOverallHighScore(isEndlessMode);
        }
        return highScores;
    }

    public int GetHighScore(int gameMode)
    {
        if (IsEndlessGameMode(gameMode))
        {
            return GetHighScores(true, false)[gameMode - endlessGameModeMinNum];
        }
        return GetHighScores(false, false)[gameMode];
    }

    public int GetOverallHighScore(bool isEndlessMode)
    {
        return GetHighScores(isEndlessMode, false).Sum();
    }

    public void UpdateHighScore(bool isEndlessMode, int newScore, bool isUpdatingToNewScore)
    {
        string[] highScoreStrings;
        int highScoreStringsGameModeIndex;
        if (isEndlessMode)
        {
            highScoreStrings = endlessHighScoreStrings;
            highScoreStringsGameModeIndex = gameMode - endlessGameModeMinNum;
        }
        else
        {
            highScoreStrings = targetHighScoreStrings;
            highScoreStringsGameModeIndex = gameMode;
        }
        int highScore = PlayerPrefs.GetInt(highScoreStrings[highScoreStringsGameModeIndex], 0);

        if ((isEndlessMode && newScore > highScore) || (!isEndlessMode && newScore < highScore) || isUpdatingToNewScore)
        {
            PlayerPrefs.SetInt(highScoreStrings[highScoreStringsGameModeIndex], newScore);
            Debug.Log(highScoreStrings[highScoreStringsGameModeIndex] + " changed from " + highScore + " to " + newScore);
        }
    }

    public void ResetHighScores()
    {
        for (int i = 0; i < targetHighScoreStrings.Length; i++)
        {
            PlayerPrefs.SetInt(targetHighScoreStrings[i], 0);
        }
        for (int i = 0; i < endlessHighScoreStrings.Length; i++)
        {
            PlayerPrefs.SetInt(endlessHighScoreStrings[i], 0);
        }
        Debug.Log("High scores reset!");
    }

    public void UnlockAllGameModes(int value)
    {
        PlayerPrefs.SetInt("AreAllGameModesUnlocked", value);  // Restart needed if switching setting from 1 to 0
    }

    public bool IsEndlessGameMode(int gameMode)
    {
        return gameMode >= endlessGameModeMinNum;
    }
}