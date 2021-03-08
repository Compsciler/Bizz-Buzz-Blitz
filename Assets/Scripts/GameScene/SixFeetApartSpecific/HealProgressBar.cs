using UnityEngine;
using UnityEngine.UI;

public class HealProgressBar : MonoBehaviour
{
    private float healTime = 8f;  // Another constant that should be written in a separate script
    private float healTimer = 0;
    public Image fill;
    
    public Color fillColorOnInfectedHealing = new Color32(216, 43, 43, 255);  // HSV(0, 80, 85)
    public Color fillColorOnHealthyHealing = new Color32(255, 160, 160, 255);

    internal static bool isNewlyOccupied = false;
    internal static bool isOccupiedByInfectedPerson = true;

    // Update is called once per frame
    void Update()
    {
        if (isNewlyOccupied && GameManager.instance.isGameActive)
        {
            if (healTimer == 0)
            {
                if (isOccupiedByInfectedPerson)
                {
                    fill.color = fillColorOnInfectedHealing;
                }
                else
                {
                    fill.color = fillColorOnHealthyHealing;
                }
            }
            healTimer += Time.deltaTime;
            FillBar();
            if (healTimer > healTime)
            {
                healTimer = 0;
                FillBar();
                isNewlyOccupied = false;
            }
        }
    }

    void FillBar()
    {
        float fillAmount = healTimer / healTime;
        fill.fillAmount = fillAmount;
    }
}