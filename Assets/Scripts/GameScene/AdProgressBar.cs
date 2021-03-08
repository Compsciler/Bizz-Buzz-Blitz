using UnityEngine;
using UnityEngine.UI;

public class AdProgressBar : MonoBehaviour
{
    [SerializeField] float progressTime = 5f;
    internal float progressTimer = 0;
    [SerializeField] bool isFillReversed;
    [SerializeField] Image fill;

    void OnEnable()
    {
        progressTimer = 0;
    }

    void Update()
    {
        progressTimer += Time.deltaTime;
        FillBar();
        if (progressTimer > progressTime)
        {
            AdManager.instance.CloseAdMenu();
        }
    }

    void FillBar()
    {
        float fillAmount = progressTimer / progressTime;
        if (isFillReversed)
        {
            fillAmount = 1 - fillAmount;
        }
        fill.fillAmount = fillAmount;
    }
}