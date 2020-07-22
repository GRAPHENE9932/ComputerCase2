using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KlimSoft
{
    public static class AndroidFeatures
    {
        private static AndroidJavaClass featuresClass;
        private static bool initialized = false;

        private static void Initialize()
        {
            featuresClass = new AndroidJavaClass("KlimSoft.AndroidFeatures");

            //Get context.
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            //Set context.
            featuresClass.CallStatic("setContext", context);

            initialized = true;
        }

        public static void Vibrate(long milliseconds)
        {
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("vibrate", milliseconds);
        }

        public static void Vibrate(long[] intervals, int repeat = -1)
        {
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("vibrate", intervals, repeat);
        }

        public static void MakeToast(string text, int length = 0)
        {
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("makeToast", text, length);
        }
    }
}