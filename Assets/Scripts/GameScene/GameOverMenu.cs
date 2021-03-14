using MEC;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    // public Text averageFPS_Text;

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
}