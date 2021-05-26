using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descriptionText;

    [SerializeField] AudioClip restartButtonClickSound;
    [SerializeField] AudioClip goToMainMenuButtonClickSound;

    [SerializeField] Stopwatch stopwatch;
    
    [SerializeField] GameObject mainCamera;

    public void Restart()
    {
        Timing.KillCoroutines();
        mainCamera.GetComponent<ButtonClickSound>().PlaySound(restartButtonClickSound);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);

        // ResetStaticVariables() delegate in GameManager.cs on scene unload
    }

    public void GoToMainMenu()
    {
        Timing.KillCoroutines();
        mainCamera.GetComponent<ButtonClickSound>().PlaySound(goToMainMenuButtonClickSound);
        SceneManager.LoadSceneAsync(Constants.mainMenuBuildIndex);

        // ResetStaticVariables() delegate in GameManager.cs on scene unload
    }

    public void UpdateLoseText(int losingPlayer)
    {
        UpdateLoseText(true, losingPlayer, null);
    }

    public void UpdateLoseText(int losingPlayer, bool[] losingButtonRuleValues)
    {
        UpdateLoseText(false, losingPlayer, losingButtonRuleValues);
    }

    private void UpdateLoseText(bool isLostOnTime, int losingPlayer, bool[] losingButtonRuleValues)
    {
        int losingNumber = BizzBuzzButton.number;
        if (GameManager.instance.isMultiplayer)
        {
            float zRotation = (float)(losingPlayer - 1) / GameManager.instance.playerTotal * 360;
            GetComponent<RectTransform>().Rotate(0, 0, zRotation);
            descriptionText.text = "Player " + losingPlayer + " lost!\n";
        }
        else
        {
            descriptionText.text = "";
        }
        descriptionText.text += losingNumber + " is " +
            BizzBuzzClassification.GetClassificationText(losingNumber);
        if (!isLostOnTime)
        {
            descriptionText.text += ", not " +
                BizzBuzzClassification.GetClassificationText(losingButtonRuleValues);
        }
    }

    public void UpdateWinText()
    {
        titleText.text = "LEVEL COMPLETE";
        descriptionText.text = stopwatch.stopwatchWithPenaltyAddedText;
    }
}