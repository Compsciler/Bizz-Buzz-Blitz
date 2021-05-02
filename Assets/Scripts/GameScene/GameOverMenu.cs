using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] TMP_Text gameOverScoreText;

    public void Restart()
    {
        Timing.KillCoroutines();
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        // ResetStaticVariables() delegate in GameManager.cs on scene unload
    }

    public void GoToMainMenu()
    {
        Timing.KillCoroutines();
        SceneManager.LoadSceneAsync(Constants.mainMenuBuildIndex);
        // ResetStaticVariables() delegate in GameManager.cs on scene unload
    }

    public void UpdateGameOverScoreText(int losingPlayer)
    {
        UpdateGameOverScoreText(true, losingPlayer, null);
    }

    public void UpdateGameOverScoreText(int losingPlayer, bool[] losingButtonRuleValues)
    {
        UpdateGameOverScoreText(false, losingPlayer, losingButtonRuleValues);
    }

    private void UpdateGameOverScoreText(bool isLostOnTime, int losingPlayer, bool[] losingButtonRuleValues)
    {
        int losingNumber = BizzBuzzClassification.number;
        if (GameManager.instance.isMultiplayer)
        {
            gameOverScoreText.text = "Player " + losingPlayer + " lost!\n";  
        }
        else
        {
            gameOverScoreText.text = "";
        }
        gameOverScoreText.text += losingNumber + " is " +
            BizzBuzzClassification.GetClassificationText(losingNumber);
        if (!isLostOnTime)
        {
            gameOverScoreText.text += ", not " +
                BizzBuzzClassification.GetClassificationText(losingButtonRuleValues);
        }
    }
}