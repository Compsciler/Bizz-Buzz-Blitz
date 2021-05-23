using System.Linq;
using TMPro;
using UnityEngine;

public class ReadyManager : MonoBehaviour
{
    [SerializeField] GameObject[] readyButtonGOs;
    [SerializeField] TMP_Text[] readyTexts;
    [SerializeField] GameObject timerBar1GO;
    [SerializeField] GameObject stopwatchGO;

    private ButtonState[] buttonStates;

    [SerializeField] AudioClip buttonDownSound;
    [SerializeField] float buttonDownSoundVolume;
    [SerializeField] AudioClip gameStartSound;
    [SerializeField] float gameStartSoundVolume;

    private enum ReadyState
    {
        NotReady = 0,
        Holding = 1,
        ReadyToRelease = 2,
        Released = 3,
    }
    private ReadyState[] readyStates;
    private bool isAllReady = false;

    private string[] readyStateTextStrings =    {
                                                    "Hold the <color=#00A86B>READY</color> button",
                                                    "<color=#00A86B>READY!</color>",
                                                    "Release to start",
                                                    ""
                                                };

    void Start()
    {
        readyButtonGOs = readyButtonGOs.CloneSubarray(0, GameManager.instance.playerTotal);
        readyTexts = readyTexts.CloneSubarray(0, GameManager.instance.playerTotal);

        buttonStates = new ButtonState[readyButtonGOs.Length];
        for (int i = 0; i < readyButtonGOs.Length; i++)
        {
            buttonStates[i] = readyButtonGOs[i].GetComponent<ButtonState>();
        }
        readyStates = new ReadyState[readyButtonGOs.Length];
    }

    void Update()
    {
        if (!isAllReady)
        {
            isAllReady = readyStates.All(x => x == ReadyState.Holding);
        }

        for (int i = 0; i < buttonStates.Length; i++)
        {
            if (!buttonStates[i].gameObject.activeSelf)
            {
                readyStates[i] = ReadyState.Holding;
            }
            switch (buttonStates[i].clickState)
            {
                case ButtonState.ClickState.PointerDown:
                    readyStates[i] = ReadyState.Holding;
                    AudioManager.instance.SFX_Source.PlayOneShot(buttonDownSound, buttonDownSoundVolume);
                    break;
                case ButtonState.ClickState.PointerUp:
                    if (isAllReady)
                    {
                        readyStates[i] = ReadyState.Released;
                    }
                    else
                    {
                        readyStates[i] = ReadyState.NotReady;
                    }
                    break;
            }
            if (isAllReady && readyStates[i] != ReadyState.Released)
            {
                readyStates[i] = ReadyState.ReadyToRelease;
            }

            readyTexts[i].text = readyStateTextStrings[(int)readyStates[i]];
            if (readyStates[i] == ReadyState.Released && readyButtonGOs[i].activeSelf)
            {
                readyButtonGOs[i].SetActive(false);
                TimerBar timerBar1 = timerBar1GO.GetComponent<TimerBar>();
                if (StateManager.instance.StateEquals<ReadyingState>())
                {
                    timerBar1.ResetTimer();
                    timerBar1.isTimerActive = true;
                    stopwatchGO.GetComponent<Stopwatch>().isStopwatchActive = true;
                    StateManager.instance.SetState(new Player1ActiveState());
                    AudioManager.instance.SFX_Source.PlayOneShot(gameStartSound, gameStartSoundVolume);
                }
            }
        }
    }
}