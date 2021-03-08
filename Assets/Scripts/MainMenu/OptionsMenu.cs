using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] GameObject[] titleLetters;
    private Vector3[] titleLetterStartPoses;
    private Vector3 lowercaseT_EndPos;
    private Vector3 lowercaseO_EndPos;
    private Vector3 lowercaseS_EndPos;

    [SerializeField] GameObject scrollbarText;
    [SerializeField] GameObject dateDropdown;
    [SerializeField] GameObject yearText;

    void Start()
    {
        titleLetterStartPoses = new Vector3[titleLetters.Length];
        for (int i = 0; i < titleLetters.Length; i++)
        {
            titleLetterStartPoses[i] = titleLetters[i].GetComponent<RectTransform>().anchoredPosition3D;
        }
        lowercaseT_EndPos = new Vector3(23f, titleLetterStartPoses[2].y, titleLetterStartPoses[2].z);
        lowercaseO_EndPos = new Vector3(-8f, titleLetterStartPoses[2].y, titleLetterStartPoses[2].z);
        lowercaseS_EndPos = new Vector3(-70f, titleLetterStartPoses[2].y, titleLetterStartPoses[2].z);
    }

    public void UpdateScrollbarText()
    {
        Scrollbar scrollbarComponent = EventSystem.current.currentSelectedGameObject.GetComponent<Scrollbar>();
        GameObject[] transparentLetters = {titleLetters[0], titleLetters[3], titleLetters[5]};
        foreach (GameObject letter in transparentLetters)
        {
            Color textColor = letter.GetComponent<TMP_Text>().color;
            letter.GetComponent<TMP_Text>().color = new Color(textColor.r, textColor.g, textColor.b, Mathf.Lerp(1, 0, scrollbarComponent.value));
        }
        titleLetters[2].GetComponent<RectTransform>().anchoredPosition3D = Vector3.Lerp(titleLetterStartPoses[2], lowercaseT_EndPos, scrollbarComponent.value);
        titleLetters[4].GetComponent<RectTransform>().anchoredPosition3D = Vector3.Lerp(titleLetterStartPoses[4], lowercaseO_EndPos, scrollbarComponent.value);
        titleLetters[6].GetComponent<RectTransform>().anchoredPosition3D = Vector3.Lerp(titleLetterStartPoses[6], lowercaseS_EndPos, scrollbarComponent.value);
    }

    public void UpdateSliderText()
    {
        Slider sliderComponent = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
        if (sliderComponent.value == 4)
        {
            scrollbarText.GetComponent<TMP_Text>().text = "for";
        }
        else
        {
            scrollbarText.GetComponent<TMP_Text>().text = sliderComponent.value.ToString();
        }
    }

    public void checkMonth(){
        TMP_Dropdown dropdownComponent = EventSystem.current.currentSelectedGameObject.GetComponentInParent<TMP_Dropdown>();
        if (dropdownComponent.value == 4)
        {
            dropdownComponent.gameObject.SetActive(false);
            dateDropdown.SetActive(true);
        }
    }

    public void checkDate()
    {
        TMP_Dropdown dropdownComponent = EventSystem.current.currentSelectedGameObject.GetComponentInParent<TMP_Dropdown>();
        if (dropdownComponent.value == 12)
        {
            dropdownComponent.gameObject.SetActive(false);
            yearText.GetComponent<TMP_Text>().text = "Easter";
        }
    }
}