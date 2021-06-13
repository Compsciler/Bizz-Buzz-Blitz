using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MEC;
using TMPro;

public class DifficultySelectMenuBasic : MonoBehaviour
{

    [SerializeField] GameObject pressedButtonImagesHolder;
    [SerializeField] GameObject startButtonsHolder;
    [SerializeField] GameObject descriptionTextsHolder;

    private GameObject[] pressedButtonImages;
    private GameObject[] startButtons;
    private GameObject[] descriptionTexts;

    [SerializeField] AudioClip playButtonSound;
    [SerializeField] GameObject fadingMask;
    [SerializeField] float fadeTime;
    [SerializeField] GameObject[] enableAfterFading;

    [SerializeField] GameObject[] enableOnFirstTimePlaying;

    // Non-essential fields below
    [SerializeField] TMP_Text timerTimeToggleButtonText;
    private string[] timerTimeToggleButtonTextStrings = {".75", "1.5", "3", "5", "10"};
    private string[] timerTimeToggleButtonTextColorHexes = {"F08600", "FFCE54", "A0D568", "4FC1E8", "AC92EB"};
    internal static int timerTimeToggleButtonTextStringIndex = 3;

    [SerializeField] TMP_Text nonResettingTimerTimeToggleButtonText;
    private string[] nonResettingTimerTimeToggleButtonTextStrings = {"10", "30", "60", "120", "240"};
    internal static int nonResettingTimerTimeToggleButtonTextStringIndex = 3;

    [SerializeField] TMP_Text nonResettingMaxTimeDelayAddedEachRoundToggleButtonText;
    private string[] nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStrings = {"0", ".75", "1.5", "3", "5", "10"};
    private string[] nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextColorHexes = {"FFFFFF", "F08600", "FFCE54", "A0D568", "4FC1E8", "AC92EB"};
    internal static int nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStringIndex = 0;

    [SerializeField] Image isResettingTimerEachRoundButtonImage;
    [SerializeField] GameObject[] enableOnToggleOffIsResettingTimerEachRound;
    [SerializeField] GameObject[] disableOnToggleOffIsResettingTimerEachRound;

    [SerializeField] Image isOneLifeOnlyButtonImage;
    private int isOneLifeOnlyOffLives = 3;
    private int isOneLifeOnlyOnLives = 1;

    [SerializeField] Sprite toggleOffSprite;
    [SerializeField] Sprite toggleOnSprite;

    void Awake()
    {
        SetDifficultyButtonRelatedUI();
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("IsFirstTimePlaying", 1) == 1)
        {
            SetEachActive(enableOnFirstTimePlaying, true);
            PlayerPrefs.SetInt("IsFirstTimePlaying", 0);
        }

        ApplyIsResettingTimerEachRoundSettings();
        ApplyTimerTimeSettings();
        ApplyNonResettingTimerTimeSettings();
        ApplyIsOneLifeOnlySettings();
    }

    public void Play(int gameMode)
    {
        Timing.RunCoroutine(PlayStartCoroutine());
        AudioManager.instance.musicSource.Stop();

        HighScoreLogger.instance.gameMode = gameMode;
    }

    IEnumerator<float> PlayStartCoroutine()
    {
        fadingMask.SetActive(true);
        CoroutineHandle fadeBackgroundCoroutine = Timing.RunCoroutine(FadeBackground());
        AudioManager.instance.SFX_Source.PlayOneShot(playButtonSound);
        yield return Timing.WaitUntilDone(fadeBackgroundCoroutine);
        SetEachActive(enableAfterFading, true);
        SceneManager.LoadSceneAsync(Constants.gameSceneBuildIndex);
    }

    IEnumerator<float> FadeBackground()
    {
        float timer = 0;
        while (timer < fadeTime)
        {
            Color maskColor = fadingMask.GetComponent<Image>().color;
            fadingMask.GetComponent<Image>().color = new Color(maskColor.r, maskColor.g, maskColor.b, Mathf.Lerp(0, 1, timer / fadeTime));
            timer += Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }
    }

    public void ResetMenuPresses()
    {
        SetEachActive(pressedButtonImages, false);
        try
        {
            SetEachActive(startButtons, false);
        }
        catch (NullReferenceException)
        {

        }
        SetEachActive(descriptionTexts, false);
    }

    private void SetDifficultyButtonRelatedUI()
    {
        pressedButtonImages = pressedButtonImagesHolder.GetChildren();
        startButtons = startButtonsHolder.GetChildren();
        descriptionTexts = descriptionTextsHolder.GetChildren();
    }

    private void SetEachActive(GameObject[] gameObjects, bool value)
    {
        foreach (GameObject go in gameObjects)
        {
            go.SetActive(value);
        }
    }

    // Non-essential methods below
    public void ToggleTimerTime()
    {
        timerTimeToggleButtonTextStringIndex = (timerTimeToggleButtonTextStringIndex + 1) % timerTimeToggleButtonTextStrings.Length;
        ApplyTimerTimeSettings();
    }

    public void ApplyTimerTimeSettings()
    {
        string newTimerTimeText = timerTimeToggleButtonTextStrings[timerTimeToggleButtonTextStringIndex];
        string newTimerTimeToggleButtonTextColorHex = timerTimeToggleButtonTextColorHexes[timerTimeToggleButtonTextStringIndex];
        timerTimeToggleButtonText.text = ExtensionMethods.GetColoredRichText(newTimerTimeText, newTimerTimeToggleButtonTextColorHex);
        GameManager.resettingTimerTime = float.Parse(newTimerTimeText);
    }

    public void ToggleNonResettingTimerTime()
    {
        nonResettingTimerTimeToggleButtonTextStringIndex = (nonResettingTimerTimeToggleButtonTextStringIndex + 1) % nonResettingTimerTimeToggleButtonTextStrings.Length;
        ApplyNonResettingTimerTimeSettings();
    }

    public void ApplyNonResettingTimerTimeSettings()
    {
        string newNonResettingTimerTimeText = nonResettingTimerTimeToggleButtonTextStrings[nonResettingTimerTimeToggleButtonTextStringIndex];
        nonResettingTimerTimeToggleButtonText.text = newNonResettingTimerTimeText;
        GameManager.nonResettingTimerTime = float.Parse(newNonResettingTimerTimeText);
    }

    public void ToggleNonResettingMaxTimeDelayAddedEachRound()
    {
        nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStringIndex = (nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStringIndex + 1) % nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStrings.Length;
        ApplyNonResettingMaxTimeDelayAddedEachRoundSettings();
    }

    public void ApplyNonResettingMaxTimeDelayAddedEachRoundSettings()
    {
        string newTimerTimeText = nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStrings[nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStringIndex];
        string newTimerTimeToggleButtonTextColorHex = nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextColorHexes[nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStringIndex];
        nonResettingMaxTimeDelayAddedEachRoundToggleButtonText.text = ExtensionMethods.GetColoredRichText(newTimerTimeText, newTimerTimeToggleButtonTextColorHex);
        GameManager.nonResettingMaxTimeDelayAddedEachRound = float.Parse(newTimerTimeText);
        Debug.Log(GameManager.nonResettingMaxTimeDelayAddedEachRound);
    }

    public void ToggleIsResettingTimerEachRound()
    {
        GameManager.isResettingTimerEachRound = !GameManager.isResettingTimerEachRound;
        ApplyIsResettingTimerEachRoundSettings();
    }

    public void ApplyIsResettingTimerEachRoundSettings()
    {
        if (GameManager.isResettingTimerEachRound)
        {
            isResettingTimerEachRoundButtonImage.sprite = toggleOnSprite;
        }
        else
        {
            isResettingTimerEachRoundButtonImage.sprite = toggleOffSprite;
        }
        foreach (GameObject go in enableOnToggleOffIsResettingTimerEachRound)
        {
            go.SetActive(!GameManager.isResettingTimerEachRound);
        }
        foreach (GameObject go in disableOnToggleOffIsResettingTimerEachRound)
        {
            go.SetActive(GameManager.isResettingTimerEachRound);
        }
    }

    public void ToggleIsOneLifeOnly()
    {
        if (GameManager.maxLives == isOneLifeOnlyOnLives)
        {
            GameManager.maxLives = isOneLifeOnlyOffLives;
        }
        else
        {
            GameManager.maxLives = isOneLifeOnlyOnLives;
        }
        ApplyIsOneLifeOnlySettings();
    }

    public void ApplyIsOneLifeOnlySettings()
    {
        if (GameManager.maxLives == isOneLifeOnlyOnLives)
        {
            isOneLifeOnlyButtonImage.sprite = toggleOnSprite;
        }
        else
        {
            isOneLifeOnlyButtonImage.sprite = toggleOffSprite;
        }
    }
}