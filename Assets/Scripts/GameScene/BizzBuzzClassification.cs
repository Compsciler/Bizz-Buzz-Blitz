using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class BizzBuzzClassification : MonoBehaviour
{
    private static MyHashTable<string, List<object>> rules;
    private static List<Color> ruleColorsUsed;

    internal static List<string> ruleNames;
    private static List<string> ruleNamesAfterExcluding;
    private static List<string> isDivisbleByOrContainsDigitRuleNames;
    private static List<string> excludedRuleNames = new List<string>() {"Booz"};
    private static string isDivisbleByOrContainsDigitMethodInfoString = "Boolean IsDivisbleByOrContainsDigit(Int32, Int32)";

    internal static List<RuleInterval> ruleIntervalList = new List<RuleInterval>();
    internal static List<string> rulesUsed;
    internal static int rulesUsedIndex;

    void Awake()
    {
        if (rules == null)  // Remove if setting added to change excludedRuleNames
        {
            SetUpRules();
            SetUpRandomRuleLists();
        }

        /*
        for (int i = 1; i <= 100; i++)
        {
            Debug.Log(string.Join(", ", ClassifyNum(i)));
            // Debug.Log(i + ": " + string.Join(", ", ClassifyNum(i)));
        }
        */

        /*
        string s = "";
        for (int i = 1; i <= 200; i++)
        {
            if (ClassifyNum(i).All(b => b))
            {
                s += i + ", ";
            }
        }
        Debug.Log(s);
        */
    }

    void Start()
    {
        UpdateRulesUsed();
        if (BizzBuzzButton.nextRuleChangeRound <= GameManager.instance.playerTotal + 1)  // Condition equivalent to (BizzBuzzButton.roundNum >= BizzBuzzButton.nextRuleChangeRound - GameManager.instance.playerTotal)
        {
            foreach (BizzBuzzButton button in BizzBuzzButton.buttonsByPlayer[0])
            {
                button.preRuleChangeEffectTweenID = button.GetComponent<BizzBuzzButtonEffects>().PlayPreRuleChangeEffects();
            }
        }
    }

    public static bool[] ClassifyNum(int n)
    {
        bool[] res = new bool[rulesUsed.Count];
        for (int i = 0; i < rulesUsed.Count; i++)
        {
            res[i] = ClassifyNum(n, rulesUsed[i]);
        }
        return res;
    }

    private static bool ClassifyNum(int n, string rule)
    {
        List<object> ruleMethodList = rules.Get(rule);
        MethodInfo ruleMethod = (MethodInfo)ruleMethodList[0];
        object[] ruleMethodParams = (object[])ruleMethodList[1];
        object[] newRuleMethodParams = new object[ruleMethodParams.Length + 1];
        newRuleMethodParams[0] = n;
        ruleMethodParams.CopyTo(newRuleMethodParams, 1);
        return (bool)ruleMethod.Invoke(null, newRuleMethodParams);
    }

    public void SetUpRules()
    {
        rules = new MyHashTable<string, List<object>>();

        rules.Put("Booz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 3));
        rules.Put("Bizz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 5));
        rules.Put("Buzz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 7));
        rules.Put("Fuzz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 8));
        rules.Put("Bazz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 9));
        rules.Put("Dupe", SetUpRuleMethodList("ContainsDuplicateDigit"));
        rules.Put("Pow", SetUpRuleMethodList("IsPerfectPower"));
        rules.Put("Semi", SetUpRuleMethodList("IsSemiprime"));
        rules.Put("Pyth", SetUpRuleMethodList("IsSumOf2NonzeroSquares"));

        ruleColorsUsed = new List<Color>();
        ruleColorsUsed.Add(new Color(1f, 0, 0));
        ruleColorsUsed.Add(new Color(0, 0, 1f));
    }

    public List<object> SetUpRuleMethodList(string ruleMethodName, params object[] ruleMethodParams)
    {
        Type classType = Type.GetType(GetType().Name);
        MethodInfo ruleMethod = classType.GetMethod(ruleMethodName);
        return new List<object>() {ruleMethod, ruleMethodParams};
    }

    public static bool IsDivisbleByOrContainsDigit(int n, int digit)
    {
        return n % digit == 0 || ContainsDigit(n, digit);
    }

    private static bool ContainsDigit(int n, int digit)
    {
        if (n == 0 && digit == 0)
        {
            return true;
        }
        while (n > 0)
        {
            if (n % 10 == digit)
            {
                return true;
            }
            n /= 10;
        }
        return false;
    }

    public static bool ContainsDuplicateDigit(int n)
    {
        HashSet<int> digits = new HashSet<int>();
        while (n > 0)
        {
            int digit = n % 10;
            if (digits.Contains(digit))
            {
                return true;
            }
            digits.Add(digit);
            n /= 10;
        }
        return false;
    }

    public static bool IsPerfectPower(int n)
    {
        if (n == 1)
        {
            return true;
        }
        for (int base_ = 2; base_ <= Mathf.Sqrt(n); base_++)
        {
            for (int exponent = 2; exponent <= Mathf.Log(n, 2); exponent++)
            {
                if (Mathf.Pow(base_, exponent) == n)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsSemiprime(int n)
    {
        return NumPrimeDivisors(n) == 2;
    }

    private static int NumPrimeDivisors(int n)
    {
        int dividedN = n;
        int primeDivisorTotal = 0;
        for (int divisor = 2; divisor <= n; divisor++)
        {
            while (dividedN % divisor == 0)
            {
                dividedN /= divisor;
                primeDivisorTotal++;
            }
        }
        return primeDivisorTotal;
    }

    public static bool IsSumOf2NonzeroSquares(int n)
    {
        for (int i = 1; i < Mathf.Sqrt(n); i++)
        {
            for (int j = 1; j < Mathf.Sqrt(n); j++)
            {
                if (i * i + j * j == n)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static string GetClassificationText(bool[] ruleValues)
    {
        string res = "";
        for (int i = 0; i < ruleValues.Length; i++)
        {
            if (ruleValues[i])
            {
                string ruleText;
                if (BizzBuzzFontManager.instance.isUsingSymbols)
                {
                    ruleText = BizzBuzzFontManager.instance.GetRuleSymbolText(rulesUsed[i]);
                }
                else
                {
                    ruleText = rulesUsed[i];
                }
                res += ExtensionMethods.GetColoredRichText(ruleText + " ", ruleColorsUsed[i]);
            }
        }
        if (res.Equals(""))
        {
            res = "Neither";
        }
        else
        {
            int lastSpaceIndex = res.LastIndexOf(" ");
            res = res.Remove(lastSpaceIndex, 1);  // " ".Length = 1
        }
        return res;
    }

    public static string GetClassificationText(int n)
    {
        return GetClassificationText(ClassifyNum(n));
    }

    public static void AddRuleInterval(List<string> ruleList, int roundInterval)
    {
        ruleIntervalList.Add(new RuleInterval(ruleList, roundInterval));
    }

    public static void UpdateRulesUsed()
    {
        if (rulesUsed == null)
        {
            rulesUsedIndex = 0;
        }
        else
        {
            rulesUsedIndex = (rulesUsedIndex + 1) % ruleIntervalList.Count;
        }
        rulesUsed = new List<string>(ruleIntervalList[rulesUsedIndex].ruleList);
        SetRandomRules();

        int roundInterval = ruleIntervalList[rulesUsedIndex].roundInterval;
        BizzBuzzButton.nextRuleChangeRound = BizzBuzzButton.roundNum + roundInterval;
        if (BizzBuzzButton.nextRuleChangeRound < 0)
        {
            BizzBuzzButton.nextRuleChangeRound = int.MaxValue;
        }

        int currentPlayer = GetCurrentPlayer();
        List<BizzBuzzButton> ruleChangeBizzBuzzButtons;
        if (GameManager.instance.playerTotal > 1 && roundInterval == 1 && BizzBuzzButton.roundNum != 1)
        {
            ruleChangeBizzBuzzButtons = BizzBuzzButton.buttonsByPlayer[currentPlayer - 1];
        }
        else
        {
            ruleChangeBizzBuzzButtons = BizzBuzzButton.buttons;
        }
        foreach (BizzBuzzButton button in ruleChangeBizzBuzzButtons)
        {
            button.GetComponent<BizzBuzzButtonEffects>().CancelPreRuleChangeEffects(button.preRuleChangeEffectTweenID);
            button.SetRuleButtonText();
            if (BizzBuzzButton.roundNum != 1)
            {
                // button.GetComponent<BizzBuzzButtonEffects>().PlayRuleChangeEffects();
            }
        }
        if (BizzBuzzButton.roundNum == 1)
        {
            BizzBuzzButton.SetPlayerNeitherRuleButtonText(1);
        }
    }

    public static int GetCurrentPlayer()
    {
        return (BizzBuzzButton.roundNum - 1) % GameManager.instance.playerTotal + 1;
    }

    public static void SetRandomRules()
    {
        List<string> ruleNamesAfterExcludingClone = new List<string>(ruleNamesAfterExcluding);
        List<string> isDivisbleByOrContainsDigitRuleNamesClone = new List<string>(isDivisbleByOrContainsDigitRuleNames);

        for (int i = 0; i < rulesUsed.Count; i++)
        {
            switch (rulesUsed[i]) {
                case "Random":
                    rulesUsed[i] = ruleNamesAfterExcludingClone[UnityEngine.Random.Range(0, ruleNamesAfterExcludingClone.Count)];
                    break;
                case "RandomIsDivisbleByOrContainsDigit":
                    rulesUsed[i] = isDivisbleByOrContainsDigitRuleNamesClone[UnityEngine.Random.Range(0, isDivisbleByOrContainsDigitRuleNamesClone.Count)];
                    break;
            }
            ruleNamesAfterExcludingClone.Remove(rulesUsed[i]);
            isDivisbleByOrContainsDigitRuleNamesClone.Remove(rulesUsed[i]);
        }
        rulesUsed = rulesUsed.OrderBy(s => ruleNames.IndexOf(s)).ToList();
    }

    public static void SetUpRandomRuleLists()
    {
        ruleNames = new List<string>();
        ruleNamesAfterExcluding = new List<string>();
        isDivisbleByOrContainsDigitRuleNames = new List<string>();
        foreach (string ruleName in rules.GetKeySet())
        {
            ruleNames.Add(ruleName);
            if (excludedRuleNames.Contains(ruleName))
            {
                continue;
            }
            if (rules.Get(ruleName)[0].ToString().Equals(isDivisbleByOrContainsDigitMethodInfoString))
            {
                isDivisbleByOrContainsDigitRuleNames.Add(ruleName);
            }
            ruleNamesAfterExcluding.Add(ruleName);
        }
    }
}

public struct RuleInterval
{
    public List<string> ruleList;
    public int roundInterval;

    public RuleInterval(List<string> ruleList, int roundInterval)
    {
        this.ruleList = ruleList;
        this.roundInterval = roundInterval;
    }
}