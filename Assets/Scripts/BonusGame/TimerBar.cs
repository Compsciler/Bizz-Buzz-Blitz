using UnityEngine;
using UnityEngine.UI;

public class TimerBar : MonoBehaviour
{
    public float maxValue;
    private float currentValue;
    public Image mask;
    public Image fill;
    public bool isHueCyclingOnce;

    void Start()
    {
        currentValue = maxValue;
    }

    void Update()
    {
        if (GameManager2.instance.isGameActive)
        {
            currentValue -= Time.deltaTime;
            if (currentValue < 0)
            {
                currentValue = 0;
                GameManager2.instance.GameOver();
            }
            FillBar();
        }
    }

    void FillBar()
    {
        float fillAmount = currentValue / maxValue;
        mask.fillAmount = fillAmount;
        if (isHueCyclingOnce)
        {
            fill.color = Color.HSVToRGB(Mathf.Lerp(0f, 1f, fillAmount), 1, 1);
        }
    }
}