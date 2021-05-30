using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeController : MonoBehaviour
{
    [SerializeField] GameObject[] strikes;
    [SerializeField] int maxLives;
    internal int lives;

    [SerializeField] Stopwatch stopwatch;

    void Start()
    {
        SetMaxLives(maxLives);
        lives = maxLives;
    }

    
    void Update()
    {
        
    }

    public void LoseLife()
    {
        lives--;
        strikes[lives].SetActive(true);

        if (stopwatch != null)
        {
            stopwatch.AddStrikePenalty();
        }
    }

    public void SetMaxLives(int maxLives)
    {
        this.maxLives = maxLives;
        for (int i = maxLives; i < strikes.Length; i++)
        {
            strikes[i].transform.parent.gameObject.SetActive(false);
        }
    }
}