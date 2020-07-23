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

        public static void KillProcess()
        {
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("killProcess");
        }

        public static string GetSignature()
        {
            if (!initialized)
                Initialize();

            return featuresClass.CallStatic<string>("getSignature");
        }

        public static void CheckSignature(string trueSignature)
        {
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("checkSignature", trueSignature);
        }

        public static void ShowDialogWithOneButton(string message, string title, string buttonText = "Ok", bool cancelable = true)
        {
            if (!initialized)
                Initialize();

            featuresClass.CallStatic("showDialogWithOneButton", message, title, buttonText, cancelable);
        }

        public static string[] GetPackageNames()
        {
            if (!initialized)
                Initialize();

            return featuresClass.CallStatic<string[]>("getPackageNames");
        }

        public static bool IsAppInstalled(string[] triggers)
        {
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
        }
    }
}