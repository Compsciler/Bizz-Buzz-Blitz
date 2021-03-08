using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;
using TMPro;
using System;

public class LogoAnimation : MonoBehaviour
{
    [SerializeField] GameObject svgImageGO;
    private Unity.VectorGraphics.SVGImage svgImage;
    [SerializeField] bool isShowingLogoScreen;
    private bool isLogoScreenSpedUp;
    [SerializeField] float fadeInTime;
    [SerializeField] float fastFadeInTime;
    [SerializeField] float startImageAdditionalDelay;
    [SerializeField] float individualImageTime;
    [SerializeField] bool isDisablingBetweenImages;
    [SerializeField] float disabledBetweenImagesTime;
    [SerializeField] float endDelay;
    [SerializeField] bool isDisplayingTextOnLastImage;
    [SerializeField] Sprite[] logoImages;
    [SerializeField] TMP_Text nameText;

    [SerializeField] GameObject logoSpeedButton;
    [SerializeField] Sprite logoScreenSpedUpSprite;
    [SerializeField] Sprite logoScreenNormalSpeedSprite;
    [SerializeField] int defaultSpeed = 1;

    [SerializeField] GameObject logoScreen;

    [SerializeField] BeforeMainMenuLoaded beforeMainMenuLoadedScript;

    // Current normal-speed animation time: 1.2 + 7 * (0.35 + 0.05) + 2 = 6 seconds

    void Start()
    {
        svgImage = svgImageGO.GetComponent<Unity.VectorGraphics.SVGImage>();

        Timing.RunCoroutine(ImmediatelyPauseMusic());
        if (isShowingLogoScreen && BeforeMainMenuLoaded.isFirstTimeLoadingSinceAppOpened)
        {
            gameObject.SetActive(false);
            logoScreen.SetActive(true);
            isLogoScreenSpedUp = (PlayerPrefs.GetInt("IsLogoScreenSpedUp", defaultSpeed) == 1);
            Timing.RunCoroutine(AnimationProcess());
        }
        else
        {
            beforeMainMenuLoadedScript.isReadyToLoadMainMenu = true;
        }
        BeforeMainMenuLoaded.isFirstTimeLoadingSinceAppOpened = false;

        DisplayCorrectLogoScreenSpeedUI();
    }

    IEnumerator<float> AnimationProcess()
    {
        float newFadeInTime;
        if (isLogoScreenSpedUp)
        {
            svgImage.sprite = logoImages[logoImages.Length - 1];
            if (isDisplayingTextOnLastImage)
            {
                nameText.gameObject.SetActive(true);
            }
            newFadeInTime = fastFadeInTime;
        }
        else
        {
            svgImage.sprite = logoImages[0];
            newFadeInTime = fadeInTime;
        }
        Color svgImageColor = svgImage.color;
        svgImage.color = new Color(svgImageColor.r, svgImageColor.g, svgImageColor.b, 0);
        float timer = 0;
        while (timer < newFadeInTime)
        {
            svgImage.color = new Color(svgImageColor.r, svgImageColor.g, svgImageColor.b, Mathf.Lerp(0, 1, timer / newFadeInTime));
            if (isLogoScreenSpedUp)
            {
                nameText.color = new Color(svgImageColor.r, svgImageColor.g, svgImageColor.b, Mathf.Lerp(0, 1, timer / newFadeInTime));
            }
            timer += Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }
        if (isLogoScreenSpedUp)
        {
            nameText.color = new Color(svgImageColor.r, svgImageColor.g, svgImageColor.b, 1);
        }
        svgImage.color = new Color(svgImageColor.r, svgImageColor.g, svgImageColor.b, 1);

        if (!isLogoScreenSpedUp)
        {
            yield return Timing.WaitForSeconds(startImageAdditionalDelay);
            for (int i = 1; i < logoImages.Length; i++)
            {
                yield return Timing.WaitForSeconds(individualImageTime);
                if (isDisablingBetweenImages)
                {
                    svgImageGO.SetActive(false);
                    yield return Timing.WaitForSeconds(disabledBetweenImagesTime);
                    svgImageGO.SetActive(true);
                }
                svgImage.sprite = logoImages[i];
                if (i == (logoImages.Length - 1) && isDisplayingTextOnLastImage)
                {
                    nameText.gameObject.SetActive(true);
                }
            }
        }

        yield return Timing.WaitForSeconds(endDelay);
        logoScreen.SetActive(false);
        beforeMainMenuLoadedScript.isReadyToLoadMainMenu = true;
    }

    IEnumerator<float> ImmediatelyPauseMusic()
    {
        while (true)
        {
            try
            {
                if (AudioManager.instance.musicSource.isPlaying)
                {
                    AudioManager.instance.musicSource.Pause();
                    break;
                }
            }
            catch (NullReferenceException)
            {

            }
            yield return Timing.WaitForOneFrame;
        }
    }

    public void ToggleLogoScreenSpeed()
    {
        if (PlayerPrefs.GetInt("IsLogoScreenSpedUp") == 0)
        {
            PlayerPrefs.SetInt("IsLogoScreenSpedUp", 1);
        }
        else
        {
            PlayerPrefs.SetInt("IsLogoScreenSpedUp", 0);
        }
        DisplayCorrectLogoScreenSpeedUI();
    }

    public void DisplayCorrectLogoScreenSpeedUI()
    {
        if (PlayerPrefs.GetInt("IsLogoScreenSpedUp", defaultSpeed) == 1)
        {
            logoSpeedButton.GetComponent<Image>().sprite = logoScreenSpedUpSprite;
            PlayerPrefs.SetInt("IsLogoScreenSpedUp", 1);
        }
        else
        {
            logoSpeedButton.GetComponent<Image>().sprite = logoScreenNormalSpeedSprite;
            PlayerPrefs.SetInt("IsLogoScreenSpedUp", 0);
        }
    }
}