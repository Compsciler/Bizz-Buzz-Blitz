using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExtensionMethods
{
    // Source: https://forum.unity.com/threads/hiow-to-get-children-gameobjects-array.142617/
    public static GameObject[] GetChildren(this GameObject go)
    {
        GameObject[] children = new GameObject[go.transform.childCount];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = go.transform.GetChild(i).gameObject;
        }
        return children;
    }

    public static string ListToString<T>(List<T> list)
    {
        return string.Join(", ", list.Select(p => p.ToString()).ToArray());
    }

    public static List<T> SelectRandomItems<T>(List<T> list, int numItemsToSelect)
    {
        List<T> listClone = new List<T>(list);  // Works for primitives
        List<T> randomItemList = new List<T>();
        for (int i = 0; i < numItemsToSelect; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, listClone.Count);
            randomItemList.Add(listClone[randomIndex]);
            listClone.RemoveAt(randomIndex);
        }
        randomItemList.Sort();
        // Debug.Log(ListToString(randomItemList));
        return randomItemList;
    }

    public static T[] CloneSubarray<T>(this T[] arr, int startIndex, int endIndex)  // startIndex: inclusive; endIndex: exclusive
    {
        int subarrayLength = endIndex - startIndex;
        T[] result = new T[subarrayLength];
        Array.Copy(arr, startIndex, result, 0, subarrayLength);
        return result;
    }

    /*
    public static T[] CloneSubarray<T>(this T[] arr, int startIndex, int length)
    {
        T[] result = new T[length];
        Array.Copy(arr, startIndex, result, 0, length);
        return result;
    }
    */

    public static float RoundToDecimalPlaces(float n, int decimalPlaces)
    {
        return Mathf.Round(n * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
    }

    public static string RoundWithTrailingDecimalZeroes(float n, int decimalPlaces)
    {
        if (decimalPlaces <= 0)
        {
            return (Mathf.RoundToInt(Mathf.Round(n * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces))).ToString();  // Outer round to remove floating-point innacuracies
        }
        return string.Format("{0:F" + decimalPlaces + "}", Mathf.Round(n * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces));
    }

    public static string GetColoredRichText(string s, Color color)
    {
        return GetColoredRichText(s, ColorUtility.ToHtmlStringRGB(color));
    }

    public static string GetColoredRichText(string s, string colorHex)
    {
        return "<color=#" + colorHex + ">" + s + "</color>";
    }
}