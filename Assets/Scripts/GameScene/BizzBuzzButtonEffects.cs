using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BizzBuzzButtonEffects : MonoBehaviour
{
    [SerializeField] ParticleSystem correctParticle;
    [SerializeField] ParticleSystem incorrectParticle;

    [SerializeField] float clickScaleFactor;
    private Vector3 initialScale;
    private Vector3 clickScale;
    [SerializeField] float clickScalingTime;
    [SerializeField] LeanTweenType clickScaleEaseType;

    [SerializeField] RectTransform canvasRect;
    private RectTransform rect;

    public enum ParticleType
    {
        Correct,
        Incorrect
    }

    void Start()
    {
        rect = GetComponent<RectTransform>();

        initialScale = rect.localScale;
        clickScale = initialScale * clickScaleFactor;
    }

    void Update()
    {
        
    }

    public void PlayGenericEffects()
    {
        LeanTween.cancel(gameObject);
        rect.localScale = initialScale;
        LeanTween.scale(rect, clickScale, clickScalingTime).setEase(clickScaleEaseType);
    }

    public void PlayCorrectEffects()
    {
        PlayGenericEffects();
        if (GameManager.instance.areParticlesOn)
        {
            PlayParticle(ParticleType.Correct);
        }
    }

    public void PlayIncorrectEffects()
    {
        PlayGenericEffects();
        if (GameManager.instance.areParticlesOn)
        {
            PlayParticle(ParticleType.Incorrect);
        }
    }

    public void PlayParticle(ParticleType particleType)
    {
        ParticleSystem particle = null;
        switch (particleType)
        {
            case ParticleType.Correct:
                particle = correctParticle;
                break;
            case ParticleType.Incorrect:
                particle = incorrectParticle;
                break;
        }
 
        Vector2 particlePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, Camera.main, out particlePos);
        particle.transform.position = canvasRect.transform.TransformPoint(particlePos);
        
        particle.Play();
    }
}