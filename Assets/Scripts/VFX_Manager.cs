using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VFX_Manager : MonoBehaviour
{
    internal static VFX_Manager instance;

    [SerializeField] GameObject VFX_ToggleButtonGO;
    [SerializeField] Sprite VFX_OnSprite;
    [SerializeField] Sprite VFX_OffSprite;

    [SerializeField] TMP_Text VFX_SpriteText;
    [SerializeField] Color VFX_OnSpriteColor;
    [SerializeField] Color VFX_OffSpriteColor;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        DisplayCorrectVFX();
    }

    public void ToggleVFX()
    {
        if (PlayerPrefs.GetInt("IsVFX_On") == 1)
        {
            PlayerPrefs.SetInt("IsVFX_On", 0);
        }
        else
        {
            PlayerPrefs.SetInt("IsVFX_On", 1);
        }
        DisplayCorrectVFX();
    }

    public void DisplayCorrectVFX()
    {
        if (PlayerPrefs.GetInt("IsVFX_On", 1) == 0)
        {
            GameManager.instance.isSFX_On = false;
            if (VFX_ToggleButtonGO != null)
            {
                VFX_ToggleButtonGO.GetComponent<Image>().sprite = VFX_OffSprite;
                VFX_SpriteText.color = VFX_OffSpriteColor;
            }
        }
        else
        {
            GameManager.instance.isSFX_On = true;
            if (VFX_ToggleButtonGO != null)
            {
                VFX_ToggleButtonGO.GetComponent<Image>().sprite = VFX_OnSprite;
                VFX_SpriteText.color = VFX_OnSpriteColor;
            }
        }
    }
}