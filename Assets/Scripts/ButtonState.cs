using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonState : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    internal enum ClickState
    {
        NotClicked,     // ✖
        PointerDown,    // ↓
        PointerHeld,    // ✔
        PointerUp,      // ↑
    }
    internal ClickState clickState;

    void LateUpdate()
    {
        // ✖|✖  =>  ↓|✔  =>  ✔|✔  =>  ↑|✖
        // ClickState beforeClickState = clickState;
        switch (clickState)
        {
            case ClickState.PointerDown:
                clickState = ClickState.PointerHeld;
                break;
            case ClickState.PointerUp:
                clickState = ClickState.NotClicked;
                break;
        }
        
        // ClickState afterClickState = clickState;
        /*
        if (beforeClickState != ClickState.NotClicked || afterClickState != ClickState.NotClicked)
        {
            Debug.Log("Before: " + beforeClickState);
            Debug.Log("After: " + afterClickState);
        }
        */
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        clickState = ClickState.PointerDown;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        clickState = ClickState.PointerUp;
    }
}
