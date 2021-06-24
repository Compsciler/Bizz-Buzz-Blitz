using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuEffects : MonoBehaviour
{
    private Vector2 tutorialArrowStartPos;
    [SerializeField] Vector2 tutorialArrowTranslation;
    [SerializeField] float tutorialArrowTranslationTime;
    [SerializeField] RectTransform tutorialArrowRect;
    private GameObject tutorialArrowGO;
    [SerializeField] LeanTweenType tutorialArrowEaseType;

    void Awake()
    {
        tutorialArrowGO = tutorialArrowRect.gameObject;
        tutorialArrowStartPos = tutorialArrowRect.anchoredPosition;
    }

    void OnEnable()
    {
        if (PlayerPrefs.GetInt("IsFirstTimePlaying", 1) == 1)
        {
            PlayTutorialArrowEffects();
        }
    }

    void OnDisable()
    {
        LeanTween.cancel(tutorialArrowGO);
        tutorialArrowRect.anchoredPosition = tutorialArrowStartPos;
        tutorialArrowGO.SetActive(false);
    }

    public void PlayTutorialArrowEffects()
    {
        tutorialArrowGO.SetActive(true);
        Vector2 tutorialArrowEndPos = tutorialArrowStartPos + tutorialArrowTranslation;
        LeanTween.move(tutorialArrowRect, tutorialArrowEndPos, tutorialArrowTranslationTime).setEase(tutorialArrowEaseType).setLoopPingPong();
    }
}