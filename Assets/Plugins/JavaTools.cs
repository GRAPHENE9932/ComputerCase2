using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KlimSoft
{
    public static class JavaTools
    {
        private static AndroidJavaObject vibrator;
        private static AndroidJavaObject context;

        /// <summary>
        /// Simple vibrate with selected time.
        /// </summary>
        /// <param name="milliseconds">Time to vibrate (milliseconds).</param>
        public static void Vibrate(long milliseconds)
        {
            try
            {
                if (vibrator == null)
                {
                    if (context == null)
                    {
                        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                        context = activity.Call<AndroidJavaObject>("getApplicationContext");
                    }
                    vibrator = context.Call<AndroidJavaObject>("getSystemService", new object[] { "vibrator" });
                }
                if (ApiLevel >= 26)
                {
                    AndroidJavaObject effect = new AndroidJavaClass("android.os.VibrationEffect").
                        CallStatic<AndroidJavaObject>("createOneShot", new object[] { milliseconds, -1 });
                    vibrator.Call("vibrate", new object[] { effect });
                }
                else
                {
                    vibrator.Call("vibrate", new object[] { milliseconds });
                }
            }
            catch { }
        }

        /// <summary>
        /// Vibrate with specific intervals.
        /// </summary>
        /// <param name="intervals">Intervals array.</param>
        public static void Vibrate(long[] intervals, int repeat = -1)
        {
            try
            {
                if (vibrator == null)
                {
                    if (context == null)
                    {
                        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                        context = activity.Call<AndroidJavaObject>("getApplicationContext");
                    }
                    vibrator = context.Call<AndroidJavaObject>("getSystemService", new object[] { "vibrator" });
                }
                if (ApiLevel >= 26)
                {
                    AndroidJavaObject effect = new AndroidJavaClass("android.os.VibrationEffect").
                        CallStatic<AndroidJavaObject>("createWaveform", new object[] { intervals, repeat });
                    vibrator.Call("vibrate", new object[] { effect });
                }
                else
                {
                    vibrator.Call("vibrate", new object[] { intervals, repeat });
                }
            }
            catch { }
        }

        public static void MakeToast(string text, int length = 0)
        {
            try
            {
                if (context == null)
                {
                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    context = activity.Call<AndroidJavaObject>("getApplicationContext");
                }
                new AndroidJavaClass("android.widget.Toast").CallStatic<AndroidJavaObject>("makeText", context, text, length).Call("show");
            }
            catch { }
        }

        /// <summary>
        /// Get current Android API level.
        /// </summary>
        private static int ApiLevel
        {
            get
            {
                try
                {
                    var version = new AndroidJavaClass("android.os.Build$VERSION");
                    return version.GetStatic<int>("SDK_INT");
                }
                catch { return 0; }
            }
        }
    }
}