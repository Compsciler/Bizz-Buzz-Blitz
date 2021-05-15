using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class BizzBuzzClassification : MonoBehaviour
{
    private static MyHashTable<string, List<object>> rules;
    private static List<Color> ruleColorsUsed;

    internal static List<string> rulesUsed;

    internal static int number = 1;

    void Awake()
    {
        if (rules == null)
        {
            SetUpRules();
        }
        rulesUsed = new List<string>() {"Bizz", "Buzz"};

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

    void Update()
    {
        
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
                res += GetColoredRichText(rulesUsed[i] + " ", ruleColorsUsed[i]);
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

    public static string GetColoredRichText(string s, Color color)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + s + "</color>";
    }
}