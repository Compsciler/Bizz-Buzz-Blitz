using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BizzBuzzClassification : MonoBehaviour
{
    private MyHashTable<string, List<object>> rules;
    private List<string> rulesUsed;

    void Start()
    {
        SetUpRules();
        /*
        rulesUsed = new List<string>() {"Bizz", "Buzz"};
        for (int i = 1; i <= 35; i++)
        {
            Debug.Log(i + ": " + string.Join(", ", ClassifyNum(i)));
        }
        */
    }

    void Update()
    {
        
    }

    public bool[] ClassifyNum(int n)
    {
        bool[] res = new bool[rulesUsed.Count];
        for (int i = 0; i < rulesUsed.Count; i++)
        {
            res[i] = ClassifyNum(n, rulesUsed[i]);
        }
        return res;
    }

    public bool ClassifyNum(int n, string rule)
    {
        List<object> ruleMethodList = rules.Get(rule);
        MethodInfo ruleMethod = (MethodInfo)ruleMethodList[0];
        object[] ruleMethodParams = (object[])ruleMethodList[1];
        object[] newRuleMethodParams = new object[ruleMethodParams.Length + 1];
        newRuleMethodParams[0] = n;
        ruleMethodParams.CopyTo(newRuleMethodParams, 1);
        return (bool)ruleMethod.Invoke(GetComponent<BizzBuzzClassification>(), newRuleMethodParams);
    }

    public void SetUpRules()
    {
        rules = new MyHashTable<string, List<object>>();

        rules.Put("Bizz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 5));
        rules.Put("Buzz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 7));
        rules.Put("Fuzz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 8));
        rules.Put("Bazz", SetUpRuleMethodList("IsDivisbleByOrContainsDigit", 9));
    }

    public List<object> SetUpRuleMethodList(string ruleMethodName, params object[] ruleMethodParams)
    {
        Type classType = Type.GetType(GetType().Name);
        MethodInfo ruleMethod = classType.GetMethod(ruleMethodName);
        return new List<object>() {ruleMethod, ruleMethodParams};
    }

    public bool IsDivisbleByOrContainsDigit(int n, int digit)
    {
        return n % digit == 0 || ContainsDigit(n, digit);
    }

    public bool ContainsDigit(int n, int digit)
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
}