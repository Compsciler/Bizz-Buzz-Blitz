using UnityEngine;
using UnityEngine.UI;

public class WaveProgressBar : MonoBehaviour
{
    [SerializeField] float maxValue;
    [SerializeField] float currentValue;
    public Image mask;
    public Image fill;
    public Color fillHealthyColor;
    public Color fillInfectedColor;
    public Color fillUnknownColor;
    private SpawnPeople spawnPeopleScript;

    public GameObject spawnManager;

    void Start()
    {
        spawnPeopleScript = spawnManager.GetComponent<SpawnPeople>();
    }

    void Update()
    {
        if (GameManager.instance.hasGameStarted)
        {
            maxValue = spawnPeopleScript.repeatRate;
            currentValue = maxValue - spawnPeopleScript.timer;
            FillBar();
        }
    }

    void FillBar()
    {
        float fillAmount = currentValue / maxValue;
        mask.fillAmount = fillAmount;
        if (GameManager.instance.areSymptomsDelayed)
        {
            fill.color = fillUnknownColor;
        }
        else
        {
            if (spawnPeopleScript.isInfectedWave)
            {
                fill.color = fillInfectedColor;
            }
            else
            {
                fill.color = fillHealthyColor;
            }
        }
    }
}