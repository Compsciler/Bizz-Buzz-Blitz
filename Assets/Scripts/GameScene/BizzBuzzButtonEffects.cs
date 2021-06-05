using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using MEC;
using TMPro;

public class BizzBuzzButtonEffects : MonoBehaviour
{
    [SerializeField] ParticleSystem correctParticle;
    [SerializeField] ParticleSystem incorrectParticle;

    [SerializeField] float clickScaleFactor;
    private Vector3 initialScale;
    private Vector3 clickScale;
    [SerializeField] float clickScalingTime;
    [SerializeField] LeanTweenType clickScaleEaseType;

    [SerializeField] LeanTweenType ruleChangeEaseType;

    [SerializeField] AudioClip correctButtonClickSound;
    [SerializeField] float correctButtonClickSoundVolume;
    [SerializeField] AudioClip maxCorrectButtonClickSound;
    [SerializeField] float maxCorrectButtonClickSoundVolume;
    [SerializeField] float correctButtonClickPitchMultiplier;
    [SerializeField] float maxConsecutiveCorrectButtonClicks;
    internal static float correctButtonClickPitch;
    private static float initialCorrectButtonClickPitch;
    private static float consecutiveCorrectButtonClicks = 0;

    [SerializeField] AudioClip incorrectButtonClickSound;
    [SerializeField] float incorrectButtonClickSoundVolume;

    [SerializeField] Color correctButtonHighlightColor;
    [SerializeField] float correctButtonHighlightTime;
    private Color buttonInitialColor;

    [SerializeField] TraumaInducer traumaInducer;

    [SerializeField] float blinkMinColorA;
    [SerializeField] float blinkPeriodTime;
    [SerializeField] AudioClip ruleChangeSound;
    [SerializeField] float ruleChangeSoundVolume;
    [SerializeField] int minRoundIntervalForRuleChangeSound;

    [SerializeField] Camera mainCamera;
    private ButtonClickSound buttonClickSoundScript;
    [SerializeField] RectTransform canvasRect;
    private RectTransform rect;

    [SerializeField] TMP_Text buttonText;

    public enum ButtonType
    {
        Correct,
        Incorrect
    }

    void Start()
    {
        mainCamera = Camera.main;
        buttonClickSoundScript = mainCamera.GetComponent<ButtonClickSound>();
        rect = GetComponent<RectTransform>();

        initialCorrectButtonClickPitch = AudioManager.instance.SFX_SourceVariablePitch.pitch;
        correctButtonClickPitch = initialCorrectButtonClickPitch;

        initialScale = rect.localScale;
        clickScale = initialScale * clickScaleFactor;

        buttonInitialColor = gameObject.GetComponent<Image>().color;
    }

    void Update()
    {
        
    }

    public void PlayGenericClickEffects()
    {
        LeanTween.cancel(gameObject);
        rect.localScale = initialScale;
        LeanTween.scale(rect, clickScale, clickScalingTime).setEase(clickScaleEaseType);
    }

    public void PlayCorrectEffects()
    {
        PlayButtonSound(ButtonType.Correct);
        
        if (!GameManager.instance.isSFX_On)
        {
            return;
        }

        PlayGenericClickEffects();
        if (GameManager.instance.areParticlesOn)
        {
            PlayParticle(ButtonType.Correct);
        }
    }

    public void PlayIncorrectEffects(bool isIncorrectOnTime)
    {
        int player = GetComponent<BizzBuzzButton>().player;
        Timing.KillCoroutines("HighlightCorrectButton" + player);
        Timing.RunCoroutine(HighlightCorrectButton(), "HighlightCorrectButton" + player);
        PlayButtonSound(ButtonType.Incorrect);
        
        if (!AudioManager.instance.SFX_Source.mute)
        {
            Handheld.Vibrate();
        }

        if (!GameManager.instance.isSFX_On)
        {
            return;
        }

        if (!isIncorrectOnTime)
        {
            PlayGenericClickEffects();
        }
        if (!isIncorrectOnTime && GameManager.instance.areParticlesOn)
        {
            PlayParticle(ButtonType.Incorrect);
        }
        traumaInducer.InduceTrauma();
    }

    public void PlayParticle(ButtonType buttonType)
    {
        ParticleSystem particle = null;
        switch (buttonType)
        {
            case ButtonType.Correct:
                particle = correctParticle;
                break;
            case ButtonType.Incorrect:
                particle = incorrectParticle;
                break;
        }
 
        Vector2 particlePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, Camera.main, out particlePos);
        particle.transform.position = canvasRect.transform.TransformPoint(particlePos);
        
        particle.Play();
    }

    public void PlayButtonSound(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.Correct:
                consecutiveCorrectButtonClicks++;
                if (consecutiveCorrectButtonClicks >= maxConsecutiveCorrectButtonClicks)
                {
                    buttonClickSoundScript.PlaySound(maxCorrectButtonClickSound, maxCorrectButtonClickSoundVolume);
                    correctButtonClickPitch = initialCorrectButtonClickPitch;
                    consecutiveCorrectButtonClicks = 0;
                }
                else
                {
                    buttonClickSoundScript.PlaySound(correctButtonClickSound, correctButtonClickSoundVolume, correctButtonClickPitch);
                }
                correctButtonClickPitch *= correctButtonClickPitchMultiplier;
                break;
            case ButtonType.Incorrect:
                buttonClickSoundScript.PlaySound(incorrectButtonClickSound, incorrectButtonClickSoundVolume);
                correctButtonClickPitch = initialCorrectButtonClickPitch;
                consecutiveCorrectButtonClicks = 0;
                break;
        }
    }

    public IEnumerator<float> HighlightCorrectButton()
    {
        int incorrectNumber = BizzBuzzButton.number;
        bool[] correctRuleValues = BizzBuzzClassification.ClassifyNum(incorrectNumber);
        BizzBuzzButton[] buttonSetBizzBuzzButtons = transform.parent.gameObject.GetComponentsInChildren<BizzBuzzButton>();
        
        Image correctButtonImage = null;
        foreach (BizzBuzzButton buttonSetBizzBuzzButton in buttonSetBizzBuzzButtons)
        {
            GameObject buttonGO = buttonSetBizzBuzzButton.gameObject;
            buttonGO.GetComponent<Image>().color = buttonInitialColor;
            if (buttonSetBizzBuzzButton.buttonRuleValues.SequenceEqual(correctRuleValues))
            {
                correctButtonImage = buttonGO.GetComponent<Image>();
                correctButtonImage.color = correctButtonHighlightColor;
            }
        }
        yield return Timing.WaitForSeconds(correctButtonHighlightTime);
        correctButtonImage.color = buttonInitialColor;
    }

    public int PlayPreRuleChangeEffects()
    {
        int tweenID = LeanTween.value(1f, blinkMinColorA, blinkPeriodTime / 2)
            .setOnUpdate((float value) =>
            {
                Color buttonTextColor = buttonText.color;
                buttonText.color = new Color(buttonTextColor.r, buttonTextColor.g, buttonTextColor.a, value);
            }).setLoopPingPong().id;
        return tweenID;
    }

    public void CancelPreRuleChangeEffects(int tweenID)
    {
        LeanTween.cancel(tweenID);
        Color buttonTextColor = buttonText.color;
        buttonText.color = new Color(buttonTextColor.r, buttonTextColor.g, buttonTextColor.b, 1f);
    }

    public void PlayRuleChangeEffects()
    {
        int roundInterval = BizzBuzzClassification.ruleIntervalList[BizzBuzzClassification.rulesUsedIndex].roundInterval;
        if (roundInterval >= minRoundIntervalForRuleChangeSound)
        {
            AudioManager.instance.SFX_Source.PlayOneShot(ruleChangeSound, ruleChangeSoundVolume);
        }
        /*
        LeanTween.cancel(gameObject);
        rect.localScale = initialScale;
        LeanTween.move(rect, Vector3.up * -665f, 0.4f).setEase(ruleChangeEaseType);
        */
    }
}