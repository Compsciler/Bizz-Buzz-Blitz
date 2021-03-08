using MEC;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsEasterEgg : MonoBehaviour
{
    private bool isAllShowing = false;

    private bool isShowingPerfect = false;
    private bool isShowingSpot = false;
    private bool isShowingFor = false;
    private bool isShowingAn = false;
    private bool isShowingEaster = false;

    [Header("Easter Egg Puzzle Components")]
    [SerializeField] Toggle toggleComponent;
    [SerializeField] Scrollbar scrollbarComponent;
    [SerializeField] Slider sliderComponent;
    [SerializeField] TMP_InputField inputFieldComponent;
    [SerializeField] TMP_Text yearTextComponent;
    [SerializeField] TMP_Text backButtonTextComponent;
    private string defaultInputText;

    [Space(15)]
    [SerializeField] float backButtonHoldTime = 1f;
    private float backButtonHoldTimer;

    [SerializeField] GameObject[] enableOnEasterEgg;
    [SerializeField] GameObject[] disableOnEasterEgg;

    private string linkUrl = "https://www.who.int/emergencies/diseases/novel-coronavirus-2019/advice-for-public";  //{"Developer Mode" link}

    [SerializeField] AudioClip playButtonSound;
    [SerializeField] GameObject fadingMask;
    [SerializeField] float fadeTime;
    [SerializeField] GameObject[] enableAfterFading;

    void Start()
    {
        defaultInputText = inputFieldComponent.text;  // Should be 36 spaces followed by "an"
        backButtonHoldTimer = backButtonHoldTime;
    }

    void Update()
    {
        if (!isAllShowing)
        {
            isShowingPerfect = toggleComponent.isOn;
            isShowingSpot = (scrollbarComponent.value > 0.95f);  // 0.95 is threshold
            isShowingFor = (sliderComponent.value == 4);
            string inputFieldText = inputFieldComponent.text;
            isShowingAn = (inputFieldText.Trim().Equals("an") && inputFieldText.Length <= 34);  // 34 is maximum length of valid string
            isShowingEaster = yearTextComponent.text.Equals("Easter");

            if (backButtonTextComponent.text.Equals("Egg"))
            {
                backButtonHoldTimer -= Time.deltaTime;
                if (backButtonHoldTimer < 0)
                {
                    isAllShowing = isShowingPerfect && isShowingSpot && isShowingFor && isShowingAn && isShowingEaster;
                    if (isAllShowing)
                    {
                        foreach (GameObject go in enableOnEasterEgg)
                        {
                            go.SetActive(true);
                        }
                        foreach (GameObject go in disableOnEasterEgg)
                        {
                            go.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                backButtonHoldTimer = backButtonHoldTime;
            }

            if (inputFieldText.Equals(""))
            {
                inputFieldComponent.text = defaultInputText;  // Resets input text if all deleted
            }
        }
    }

    public void ShowValues()  // Unused
    {
        Debug.Log("Perfect: " + isShowingPerfect);
        Debug.Log("Spot: " + isShowingSpot);
        Debug.Log("For: " + isShowingFor);
        Debug.Log("An: " + isShowingAn);
        Debug.Log("Easter: " + isShowingEaster);
    }

    public void OpenKeyboard()  // Unused except for in Testing Zone Menu
    {
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

    public void OpenLink()
    {
        Application.OpenURL(linkUrl);
    }

    public void PlayBonusGame()
    {
        Timing.RunCoroutine(PlayBonusGameStartCoroutine());
        AudioManager.instance.musicSource.Stop();
    }

    IEnumerator<float> PlayBonusGameStartCoroutine()
    {
        fadingMask.SetActive(true);
        CoroutineHandle fadeBackgroundCoroutine = Timing.RunCoroutine(FadeBackground());
        AudioManager.instance.SFX_Source.PlayOneShot(playButtonSound);
        yield return Timing.WaitUntilDone(fadeBackgroundCoroutine);
        foreach (GameObject go in enableAfterFading)
        {
            go.SetActive(true);
        }
        SceneManager.LoadSceneAsync(Constants.bonusGameBuildIndex);
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
}