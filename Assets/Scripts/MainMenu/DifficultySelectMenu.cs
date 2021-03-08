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

    // https://stackoverflow.com/questions/5849548/is-this-array-initialization-incorrect
    //{Game mode unlock requirements, in order by the game mode (starting from tutorial here) considered for requirements}
    internal static int[][,] gameModeUnlockReqs = new int[][,]{
        new int[,] {{}},
        new int[,] {{}},
        new int[,] {{0, 20}},
        new int[,] {{1, 25}},
        new int[,] {{0, 20}},
        new int[,] {{3, 25}},
        new int[,] {{0, 20}},
        new int[,] {{5, 25}},
        new int[,] {{2, 30}, {4, 20}, {6, 35}}
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

    private void SetUpUnlocksAndScores()
    {
        int[] highScores = HighScoreLogger.instance.GetHighScores(false);

        for (int i = 0; i < gameModeUnlockReqs.Length; i++)
        {
            // GameObject difficultyButton = difficultyButtons[i];
            int[,] currentUnlockReqs = gameModeUnlockReqs[i];
            bool currentUnlockReqsMet = true;
            for (int j = 0; j < currentUnlockReqs.Length / 2; j++)  // Foreach loop doesn't work somehow, probably because C# Length property returns total number of integers in array
            {
                int highScoreForReq = highScores[currentUnlockReqs[j, 0]];
                int minScoreReq = currentUnlockReqs[j, 1];
                if (highScoreForReq < minScoreReq)
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
                if (i >= 1 && i < highScores.Length + 1)  // Does not access scores of Tutorial and Custom Mode  //{Optional: change if needed}
                {
                    int highScore = highScores[i - 1];
                    if (highScore > 0)
                    {
                        TMP_Text scoreText = scoreTexts[i].GetComponent<TMP_Text>();
                        scoreText.text = highScore.ToString();
                    }
                }
            }
        }
    }

    private void SetDifficultyButtonRelatedUI()
    {
        pressedButtonImages = pressedButtonImagesHolder.GetChildren();
        scoreTexts = scoreTextsHolder.GetChildren();
        lockIcons = lockIconsHolder.GetChildren();
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
}