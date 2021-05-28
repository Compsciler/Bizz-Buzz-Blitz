using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BizzBuzzFontManager : MonoBehaviour
{
    internal static BizzBuzzFontManager instance;
    
    internal bool isUsingSymbols = false;
    [SerializeField] TMP_Text[] ruleTexts;

    [SerializeField] TMP_FontAsset symbolFont;
    internal List<string> ruleSymbols = new List<string>() {"⑤", "⑦", "⑧", "⑨", "⎘", "m<sup>k</sup>", "p<sub>1</sub>p<sub>2</sub>", "◺"};

    [SerializeField] GameObject ruleSymbolToggleButtonGO;
    [SerializeField] Sprite isNotUsingSymbolsSprite;
    [SerializeField] Sprite isUsingSymbolsSprite;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DisplayCorrectRuleTexts();
        if (isUsingSymbols)
        {
            foreach (TMP_Text ruleText in ruleTexts)
            {
                ruleText.font = symbolFont;
                ruleText.fontSharedMaterial = symbolFont.material;
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public string GetRuleSymbolText(string rule)
    {
        return ruleSymbols[BizzBuzzClassification.ruleNames.IndexOf(rule)];
    }

    public void ToggleRuleTexts()
    {
        if (PlayerPrefs.GetInt("IsUsingRuleSymbols") == 1)
        {
            PlayerPrefs.SetInt("IsUsingRuleSymbols", 0);
        }
        else
        {
            PlayerPrefs.SetInt("IsUsingRuleSymbols", 1);
        }
        DisplayCorrectRuleTexts();
    }

    public void DisplayCorrectRuleTexts()
    {
        if (PlayerPrefs.GetInt("IsUsingRuleSymbols", 0) == 0)
        {
            isUsingSymbols = false;
            if (ruleSymbolToggleButtonGO != null)
            {
                ruleSymbolToggleButtonGO.GetComponent<Image>().sprite = isNotUsingSymbolsSprite;
            }
        }
        else
        {
            isUsingSymbols = true;
            if (ruleSymbolToggleButtonGO != null)
            {
                ruleSymbolToggleButtonGO.GetComponent<Image>().sprite = isUsingSymbolsSprite;
            }
        }
    }
}