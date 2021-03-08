using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using MEC;
using TMPro;

public class AdManager : MonoBehaviour, IUnityAdsListener
{
    internal static AdManager instance;

    [SerializeField] GameObject adMenu;
    [SerializeField] GameObject adClock;
    [SerializeField] TMP_Text descriptionErrorText;
    private bool isTestMode = false;
    private bool isTimeScaleZeroDuringAd;
    private string placement = "rewardedVideo";

    internal bool isAdCompleted = false;
    internal int adsWatchedTotal = 0;
    internal int maxAdsWatchedPerGame = 1;  //{Optional: change value}

    void Awake()
    {
        if (instance == null)  // Creates a new instance on scene restart because the AdManager script is destroyed along with the GameObject?
        {
            instance = this;
        }
    }

    void Start()
    {
        isTimeScaleZeroDuringAd = isTestMode;
        Advertisement.AddListener(this);
        if (Constants.platform == Constants.Platform.iOS)
        {
            Advertisement.Initialize(Constants.appleGameId, isTestMode);  // iOS SPECIFIC
        }
        else
        {
            Advertisement.Initialize(Constants.androidGameId, isTestMode);  // Android SPECIFIC
        }

        if (GameManager.instance.areSymptomsDelayed && GameManager.instance.isResettingDelayedSymptoms)  //{Change if condition and description text}
        {
            descriptionErrorText.text = "Watch ad to reset infection timers and reveal all infected people";
        }
        else
        {
            descriptionErrorText.text = "Watch ad to reset infection timers";
        }
    }

    IEnumerator<float> ShowAd()
    {
        isAdCompleted = false;
        if (isTimeScaleZeroDuringAd)
        {
            Time.timeScale = 0;
        }
        if (!Advertisement.IsReady())
        {
            yield return Timing.WaitForOneFrame;
        }
        Advertisement.Show(placement);
        // Debug.Log("Ad shown");
    }

    public void ShowAdStartCoroutine()
    {
        Timing.RunCoroutine(ShowAd());
    }

    public void CloseAdMenu()
    {
        adMenu.SetActive(false);
    }

    public IEnumerator<float> InfiniteWaitToBreakFrom()
    {
        while (true)
        {
            yield return Timing.WaitForOneFrame;
        }
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (isTimeScaleZeroDuringAd)
        {
            Time.timeScale = 1;
        }
        if (showResult == ShowResult.Finished)
        {
            isAdCompleted = true;
            adsWatchedTotal++;
            CloseAdMenu();
            // Debug.Log("Ad finished");
        }
        else
        {
            descriptionErrorText.text = "<color=#FF4040>Error loading or finishing ad! Check your internet connection.</color>";
            adClock.GetComponent<AdProgressBar>().progressTimer = 0;
            Debug.Log("Ad error!");
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
        
    }

    public void OnUnityAdsDidError(string message)
    {
        descriptionErrorText.text = "<color=#FF4040>Error loading ad! Check your internet connection.</color>";
        Debug.Log("OnUnityAdsDidError()");
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        
    }

    void OnDestroy()
    {
        Advertisement.RemoveListener(this);
    }
}