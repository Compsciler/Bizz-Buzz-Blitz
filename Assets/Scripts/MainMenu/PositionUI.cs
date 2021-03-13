using UnityEngine;

public class PositionUI : MonoBehaviour
{
    [SerializeField] RectTransform referencePosition;
    [SerializeField] Vector2 offset;

    private enum ActiveState
    {
        Default,
        Active,
        Inactive
    }
    [SerializeField] ActiveState activeState;

    void Start()
    {
        GetComponent<RectTransform>().anchoredPosition = referencePosition.anchoredPosition + offset;
        switch (activeState)
        {
            case ActiveState.Active:
                gameObject.SetActive(true);
                break;
            case ActiveState.Inactive:
                gameObject.SetActive(false);
                break;
        }
    }
}