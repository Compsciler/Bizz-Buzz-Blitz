using UnityEngine;

public class AllClearDisabler : MonoBehaviour
{
    private bool isAllClear;

    public GameObject[] enableIfNotAllClear;
    public GameObject[] disableIfNotAllClear;

    void Start()
    {
        isAllClear = (PlayerPrefs.GetInt("IsAllClear", 1) == 1);
        if (!isAllClear)
        {
            foreach (GameObject go in enableIfNotAllClear)
            {
                go.SetActive(true);
            }
            foreach (GameObject go in disableIfNotAllClear)
            {
                go.SetActive(false);
            }
        }
    }
}