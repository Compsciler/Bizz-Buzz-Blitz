using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerMatch : MonoBehaviour
{
    [SerializeField] float match;
    private float defaultAspectRatio = 16f / 9f;
    private float aspectRatio;

    void Start()
    {
        aspectRatio = (float)Screen.width / Screen.height;
        if (aspectRatio < defaultAspectRatio)
        {
            GetComponent<CanvasScaler>().matchWidthOrHeight = match;
        }
    }
}