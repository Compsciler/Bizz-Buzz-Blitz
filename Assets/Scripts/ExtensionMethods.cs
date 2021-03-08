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
            int randomIndex = Random.Range(0, listClone.Count);
            randomItemList.Add(listClone[randomIndex]);
            listClone.RemoveAt(randomIndex);
        }
        randomItemList.Sort();
        // Debug.Log(ListToString(randomItemList));
        return randomItemList;
    }
}