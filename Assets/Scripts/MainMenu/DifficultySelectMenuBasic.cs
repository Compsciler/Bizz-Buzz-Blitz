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
    private static int timerTimeToggleButtonTextStringIndex = 3;

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

        ApplyTimerTimeSettings();
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
        GameManager.timerTime = float.Parse(newTimerTimeText);
    }
}