using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;

public class AdManager : MonoBehaviour, IRewardedVideoAdListener
{
    private const string APP_KEY = "587b8177a98a7ae845c12a7beeefb5dfdbc535528bd5998a";
    private static readonly string[] DISABLED_NETWORKS = new string[] { AppodealNetworks.FACEBOOK, AppodealNetworks.YANDEX, 
    AppodealNetworks.FLURRY};

    private void Start()
    {
        Initialize(false);
    }

    private static void Initialize(bool testing)
    {
        Appodeal.setTesting(testing);
        Appodeal.disableLocationPermissionCheck();
        Appodeal.muteVideosIfCallsMuted(true);
        foreach (string network in DISABLED_NETWORKS)
            Appodeal.disableNetwork(network);

        Appodeal.initialize(APP_KEY, Appodeal.INTERSTITIAL/* | Appodeal.REWARDED_VIDEO*/);
    }

    public static void ShowInterstitial()
    {
         Appodeal.show(Appodeal.INTERSTITIAL);
    }

    public void onRewardedVideoLoaded(bool precache)
    {
        //Loaded.
    }

    public void onRewardedVideoFailedToLoad()
    {
        //Load failed.
    }

    public void onRewardedVideoShowFailed()
    {
        //Show failed.
    }

    public void onRewardedVideoShown()
    {
        //Shown.
    }

    public void onRewardedVideoFinished(double amount, string name)
    {
        //Finished. //Give reward.
    }

    public void onRewardedVideoClosed(bool finished)
    {
        //Closed.
    }

    public void onRewardedVideoExpired()
    {
        //Expired.
    }

    public void onRewardedVideoClicked()
    {
        //Clicked.
    }
}
