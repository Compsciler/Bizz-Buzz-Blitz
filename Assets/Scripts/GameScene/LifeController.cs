using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeController : MonoBehaviour
{
    [SerializeField] GameObject[] strikes;
    internal int maxLives;
    internal int lives;

    [SerializeField] Stopwatch stopwatch;

    void Start()
    {
        maxLives = strikes.Length;
        lives = maxLives;
    }

    
    void Update()
    {
        
    }

    public void LoseLife()
    {
        lives--;
        strikes[maxLives - lives - 1].SetActive(true);

        if (stopwatch != null)
        {
            stopwatch.AddStrikePenalty();
        }
    }
}