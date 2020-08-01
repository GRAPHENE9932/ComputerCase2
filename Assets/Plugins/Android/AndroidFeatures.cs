using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KlimSoft
{
    public static class AndroidFeatures
    {
        private static AndroidJavaClass featuresClass;
        private static bool initialized = false;

        public static int DialogButtonId
        {
            get
            {
                if (!initialized)
                    Initialize();

                int id = featuresClass.GetStatic<int>("dialogButtonId");
                featuresClass.SetStatic<int>("dialogButtonId", -1);
                return id;
            }
        }

        private static void Initialize()
        {
            featuresClass = new AndroidJavaClass("KlimSoft.AndroidFeatures");

            //Get context and activity.
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            //Set context and activity.
            featuresClass.CallStatic("setContext", context);
            featuresClass.CallStatic("setActivity", activity);

            initialized = true;
        }

        public static void Vibrate(long milliseconds)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("vibrate", milliseconds);
#endif
        }

        public static void Vibrate(long[] intervals, int repeat = -1)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("vibrate", intervals, repeat);
#endif
        }

        public static void MakeToast(string text, int length = 0)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("makeToast", text, length);
#endif
            Debug.Log("Computer case 2 toast: " + text);
        }

        public static void KillProcess()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("killProcess");
#endif
        }

        public static string GetSignature()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            return featuresClass.CallStatic<string>("getSignature");
#else
            return null;
#endif
        }

        public static void CheckSignature(string trueSignature)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("checkSignature", trueSignature);
#endif
        }

        public static void ShowDialogWithOneButton(string message, string title, string buttonText = "Ok", bool cancelable = true)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("showDialogWithOneButton", message, title, buttonText, cancelable);
#endif
        }

        public static string[] GetPackageNames()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            return featuresClass.CallStatic<string[]>("getPackageNames");
#else
            return null;
#endif
        }

        public static bool IsAppInstalled(string[] triggers)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!initialized)
                Initialize();

            bool res = false;
            try
            {
                res = featuresClass.CallStatic<bool>("isAppInstalled", new object[] { triggers });
            }
            catch (Exception e)
            {
                e.ToString();
            }
            return res;
#else
            return false;
#endif
        }
    }
}