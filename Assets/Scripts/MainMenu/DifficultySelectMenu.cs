using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MEC;
using TMPro;

public class DifficultySelectMenu : MonoBehaviour
{
    [SerializeField] GameObject[] difficultyButtons;

    [SerializeField] GameObject pressedButtonImagesHolder;
    [SerializeField] GameObject scoreTextsHolder;
    [SerializeField] GameObject lockIconsHolder;
    [SerializeField] GameObject startButtonsHolder;
    [SerializeField] GameObject descriptionTextsHolder;

    private GameObject[] pressedButtonImages;
    private GameObject[] scoreTexts;
    private GameObject[] lockIcons;
    private GameObject[] startButtons;
    private GameObject[] descriptionTexts;

    [SerializeField] AudioClip playButtonSound;
    [SerializeField] GameObject fadingMask;
    [SerializeField] float fadeTime;
    [SerializeField] GameObject[] enableAfterFading;

    [SerializeField] GameObject[] enableOnFirstTimePlaying;

    [SerializeField] GameObject targetDifficultyButtonsHolder;
    [SerializeField] GameObject endlessDifficultyButtonsHolder;

    private GameObject[] targetDifficultyButtons;
    private GameObject[] endlessDifficultyButtons;

    [SerializeField] Button endlessModeToggleButton;
    [SerializeField] TMP_Text endlessModeToggleButtonText;
    [SerializeField] float endlessModeToggleButtonNoninteractableTime;

    // https://stackoverflow.com/questions/5849548/is-this-array-initialization-incorrect
    //{Game mode unlock requirements, in order by the game mode (starting from tutorial here) considered for requirements}
    internal static int[][,] targetGameModeUnlockReqTargetRounds = new int[][,]{
        new int[,] {},
        new int[,] {{0, 50}},
        new int[,] {{1, 50}},
        new int[,] {{1, 50}},
        new int[,] {{1, 50}},
        new int[,] {{2, 100}},
        new int[,] {{4, 100}},
        new int[,] {{5, 100}, {6, 50}},
        new int[,] {{7, 100}}
    };
    private static int targetGameModeUnlockReqTimePerRound = 10;
    internal static int[][,] targetGameModeUnlockReqs = new int[targetGameModeUnlockReqTargetRounds.Length][,];

    internal static int[][,] endlessGameModeUnlockReqs = new int[][,]{
        new int[,] {},
        new int[,] {{0, 5}},
        new int[,] {{1, 5}},
        new int[,] {{1, 5}},
        new int[,] {{1, 5}},
        new int[,] {{2, 5}},
        new int[,] {{4, 5}},
        new int[,] {{5, 5}, {6, 5}},
        new int[,] {{7, 5}}
    };

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
    }

    void OnEnable()
    {
        SetUpUnlocksAndScores();
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

    public void ResetScoreTextsAndLockIcons()
    {
        for (int i = 0; i < scoreTexts.Length; i++)
        {
            scoreTexts[i].SetActive(false);
            lockIcons[i].SetActive(false);
        }
    }

    private void SetUpUnlocksAndScores()
    {
        for (int i = 0; i < targetGameModeUnlockReqTargetRounds.Length; i++){
            int currentUnlockReqCount = targetGameModeUnlockReqTargetRounds[i].GetLength(0);
            targetGameModeUnlockReqs[i] = new int[currentUnlockReqCount, 2];
            for (int j = 0; j < currentUnlockReqCount; j++)
            {
                int targetGameModeUnlockReqTime = targetGameModeUnlockReqTargetRounds[i][j, 1] * targetGameModeUnlockReqTimePerRound;
                targetGameModeUnlockReqs[i][j, 0] = targetGameModeUnlockReqTargetRounds[i][j, 0];
                targetGameModeUnlockReqs[i][j, 1] = targetGameModeUnlockReqTime;
                Debug.Log(targetGameModeUnlockReqs[i][j, 0] + " " + targetGameModeUnlockReqs[i][j, 1]);
            }
        }
        GetUnlockStatus(false, 0, 0);
        GetUnlockStatus(false, targetGameModeUnlockReqs.Length, HighScoreLogger.instance.endlessGameModeMinNum);
    }

    public void GetUnlockStatus(bool isEndlessMode, int rawOffset, int gameModeOffset)
    {
        int[] highScores = HighScoreLogger.instance.GetHighScores(isEndlessMode, false);

        int[][,] gameModeUnlockReqs;
        if (isEndlessMode)
        {
            gameModeUnlockReqs = endlessGameModeUnlockReqs;
        }
        else
        {
            gameModeUnlockReqs = targetGameModeUnlockReqs;
        }

        for (int i = rawOffset; i < gameModeUnlockReqs.Length + rawOffset; i++)
        {
            // GameObject difficultyButton = difficultyButtons[i];
            int[,] currentUnlockReqs = gameModeUnlockReqs[i - rawOffset];
            bool currentUnlockReqsMet = true;
            for (int j = 0; j < currentUnlockReqs.Length / 2; j++)  // Foreach loop doesn't work somehow, probably because C# Length property returns total number of integers in array, could change to .GetLength(0)
            {
                int highScoreForReq = highScores[currentUnlockReqs[j, 0]];
                int scoreReq = currentUnlockReqs[j, 1];
                if ((isEndlessMode && highScoreForReq < scoreReq) || (!isEndlessMode && highScoreForReq > scoreReq && highScoreForReq != 0))
                {
                    currentUnlockReqsMet = false;
                }
            }
            if (currentUnlockReqsMet || PlayerPrefs.GetInt("AreAllGameModesUnlocked", 0) == 1)
            {
                lockIcons[i].SetActive(false);
                try
                {
                    startButtons[i].GetComponent<Button>().interactable = true;
                }
                catch (NullReferenceException)
                {

                }
                int highScore = highScores[i - rawOffset];
                if (highScore > 0)
                {
                    TMP_Text scoreText = scoreTexts[i].GetComponent<TMP_Text>();
                    scoreText.text = highScore.ToString();
                    scoreTexts[i].tag = "ActivateOnToggleEndlessMode";
                }
            }
            else
            {
                lockIcons[i].tag = "ActivateOnToggleEndlessMode";
            }
        }
    }

    public void ToggleEndlessMode()
    {
        if (endlessDifficultyButtonsHolder.activeSelf)
        {
            endlessDifficultyButtonsHolder.SetActive(false);
            targetDifficultyButtonsHolder.SetActive(true);
            Timing.RunCoroutine(EndlessDifficultyButtonsTextChange(false));
        }
        else
        {
            targetDifficultyButtonsHolder.SetActive(false);
            endlessDifficultyButtonsHolder.SetActive(true);
            Timing.RunCoroutine(EndlessDifficultyButtonsTextChange(true));
        }
    }

    public IEnumerator<float> EndlessDifficultyButtonsTextChange(bool isChangingToEndlessMode)
    {
        endlessModeToggleButton.interactable = false;
        yield return Timing.WaitForSeconds(endlessModeToggleButtonNoninteractableTime);
        int iStart;
        int iEnd;
        if (isChangingToEndlessMode)
        {
            endlessModeToggleButtonText.text = "Switch to\nTarget Mode";
            iStart = targetGameModeUnlockReqs.Length;
            iEnd = targetGameModeUnlockReqs.Length + endlessGameModeUnlockReqs.Length;
        }
        else
        {
            endlessModeToggleButtonText.text = "Switch to\nEndless Mode";
            iStart = 0;
            iEnd = targetGameModeUnlockReqs.Length;
        }
        ResetScoreTextsAndLockIcons();
        for (int i = iStart; i < iEnd; i++)
        {
            if (scoreTexts[i].CompareTag("ActivateOnToggleEndlessMode"))
            {
                scoreTexts[i].SetActive(true);
            }
            if (lockIcons[i].CompareTag("ActivateOnToggleEndlessMode"))
            {
                lockIcons[i].SetActive(true);
            }
        }
        ResetMenuPresses();
        endlessModeToggleButton.interactable = true;
    }

    private void SetDifficultyButtonRelatedUI()
    {
        pressedButtonImages = pressedButtonImagesHolder.GetChildren();
        scoreTexts = scoreTextsHolder.GetChildren();
        lockIcons = lockIconsHolder.GetChildren();
        startButtons = startButtonsHolder.GetChildren();
        descriptionTexts = descriptionTextsHolder.GetChildren();

        targetDifficultyButtons = targetDifficultyButtonsHolder.GetChildren();
        endlessDifficultyButtons = endlessDifficultyButtonsHolder.GetChildren();
    }

    private void SetEachActive(GameObject[] gameObjects, bool value)
    {
        foreach (GameObject go in gameObjects)
        {
            go.SetActive(value);
        }
    }
}