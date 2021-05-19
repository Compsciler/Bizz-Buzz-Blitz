using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BizzBuzzButtonEffects : MonoBehaviour
{
    [SerializeField] ParticleSystem correctParticle;
    [SerializeField] ParticleSystem incorrectParticle;

    [SerializeField] RectTransform canvasRect;

    public enum ParticleType
    {
        Correct,
        Incorrect
    }

    void Start()
    {
        
    }

    void Update()
    {
        
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