using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using KlimSoft;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
    public NavigationScript nav;
    public GameObject readError;
    public Text readErrorInfo;

    private static string path;

    private void Start()
    {
        if (LoadScript.errorInfo != null)
        {
            readError.SetActive(true);
            readErrorInfo.text = LoadScript.errorInfo;
        }
    }

    void OnEnable()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        path = Path.Combine(Application.persistentDataPath, "Errors.log");
#else
        path = Path.Combine(Application.dataPath, "Errors.log");
#endif
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        //If log type == error or exception.
        if (type == LogType.Error || type == LogType.Exception)
        {
            try
            {
                //Start message about error.
                JavaTools.MakeToast(string.Format(LangManager.GetString("error_occured"), path), 1);
            }
            catch { };

            //Preparing text for log.
            string body = "Log type: " + type + "\n" +
                "Time: " + DateTime.Now + "\n" +
                "Device name: " + SystemInfo.deviceName + "\n" +
                "Device model: " + SystemInfo.deviceModel + "\n" +
                "Operating system: " + SystemInfo.operatingSystem + "\n" +
                "System memory size: " + SystemInfo.systemMemorySize + "\n" +
                "Device unique identifier: " + SystemInfo.deviceUniqueIdentifier + "\n" +
                "Battery level: " + SystemInfo.batteryLevel + "\n" +
                "Battery status: " + SystemInfo.batteryStatus + "\n" +
                "Device type: " + SystemInfo.deviceType + "\n" +
                "Processor count: " + SystemInfo.processorCount + "\n" +
                "Processor frequency: " + SystemInfo.processorFrequency + "\n" +
                "Navigation state: " + (nav != null ? nav.currentState.ToString() : "Loading") + "\n" +
                "Game version: " + Application.version + "\n" +
                "Log string: " + logString + "\n" +
                "Stack trace: " + stackTrace + "\n";

            File.AppendAllText(path, body);
        }
    }
}
