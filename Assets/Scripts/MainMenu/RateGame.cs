using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class RateGame : MonoBehaviour
{
    [SerializeField] GameObject rateButton;
    [SerializeField] GameObject creditsButton;

    internal static bool isReadyToRequestStoreReview = false;

    void Start()
    {
        if (!Constants.isMobilePlatform)
        {
            rateButton.SetActive(false);
            float xPos = (rateButton.GetComponent<RectTransform>().anchoredPosition.x + creditsButton.GetComponent<RectTransform>().anchoredPosition.x) / 2;
            float yPos = creditsButton.GetComponent<RectTransform>().anchoredPosition.y;
            creditsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
        }
        else if (isReadyToRequestStoreReview)
        {
#if UNITY_IOS
            Device.RequestStoreReview();
#endif
            isReadyToRequestStoreReview = false;
            PlayerPrefs.SetInt("StoreReviewRequestTotal", 1);
            Debug.Log("Ready to request store review!");
        }
    }

    public void RequestStoreReview()
    {
#if UNITY_IOS
        Device.RequestStoreReview();  // iOS SPECIFIC, will change to link to store page
#endif
    }

    //{Rate Button should open a link to game's store page}
}