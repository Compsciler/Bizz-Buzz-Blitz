using UnityEngine;
using UnityEngine.SceneManagement;

public class BeforeMainMenuLoaded : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;

    internal bool isReadyToLoadMainMenu = false;
    [SerializeField] GameObject usernameCreationMenu;

    internal static bool isFirstTimeLoadingSinceAppOpened = true;

    void Update()
    {
        if (isReadyToLoadMainMenu)
        {
            isReadyToLoadMainMenu = false;
            if (PlayerPrefs.GetString("Username", "").Equals("") && !LeaderboardManager.isPlayingAsGuest)
            {
                StartCoroutine(usernameCreationMenu.GetComponent<UsernameCreation>().CreateUsername());
            }
            else
            {
                if (LeaderboardManager.isPlayingAsGuest)
                {
                    LeaderboardManager.username = "Guest";
                }
                else
                {
                    LeaderboardManager.username = PlayerPrefs.GetString("Username");
                }
                if (PlayerPrefs.GetInt("IsAllClear", 0) == 1 || !usernameCreationMenu.GetComponent<UsernameCreation>().isCheckingIfAllClear)
                {
                    mainMenu.SetActive(true);
                    AudioManager.instance.musicSource.Play();
                }
                else
                {
                    SceneManager.LoadScene(Constants.bonusGameBuildIndex);
                }
            }
        }
    }
}