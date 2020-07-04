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

public class ErrorManager : MonoBehaviour
{
    public MessageBoxManager messageBox;
    public NavigationScript nav;

    /*private static byte[] key, iv;
    private static byte[] pass, email;*/

    private static string path;

    //Фіксація і відображення помилок та виключень
//#if !UNITY_EDITOR

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        path = Path.Combine(Application.persistentDataPath, "Errors and warnings.log");
#else
        path = Path.Combine(Application.dataPath, "Errors and warnings.log");
#endif
        //byte[] bytes = Convert.FromBase64String(Resources.Load<TextAsset>("MailKey").text);

        /*key = bytes.Take(32).ToArray();
        iv = bytes.Skip(32).Take(16).ToArray();
        pass = bytes.Skip(48).Take(16).ToArray();
        email = bytes.Skip(64).Take(32).ToArray();*/
    }

    void OnEnable()
    {
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
            
            //Start message about error.
            JavaTools.MakeToast(string.Format(LangManager.GetString("error_occured"), path));
        }

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
        {
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
                "Navigation state: " + nav.currentState + "\n" +
                "Log string: " + logString + "\n" +
                "Stack trace: " + stackTrace + "\n";

            File.AppendAllText(path, body);
        }
    }

    /*private async Task<bool> SendEmail(string subject, string body)
    {
        try
        {
            //Initialize key and initialization vector for AES.
            Aes aes = Aes.Create();
            //Встановлення розміру ключа.
            aes.KeySize = key.Length * 8;
            //Встановлення самого ключа.
            aes.Key = key;
            //Встановлення вектора ініціалізації.
            aes.IV = iv;
            //Встановлення заповнення, якщо розмір даних не ділиться на розмір блока.
            aes.Padding = PaddingMode.PKCS7;
            //Встановлення режиму.
            aes.Mode = CipherMode.ECB;
            //Створення екземпляра інтерфейсу ICryptoTransform, який зашифрує дані.
            ICryptoTransform dec = aes.CreateDecryptor();
            //Отримання розшифрованого паролю.
            string pass = Encoding.UTF8.GetString(dec.TransformFinalBlock(ErrorManager.pass, 0, ErrorManager.pass.Length));
            string email = Encoding.UTF8.GetString(dec.TransformFinalBlock(ErrorManager.email, 0, ErrorManager.email.Length));

            Debug.Log(email);
            Debug.Log(pass);

            //Create mail message.
            MailMessage mail = new MailMessage(email, email, subject, body);
            //Create smtp client.
            SmtpClient client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(email, pass),
                EnableSsl = true,
                Timeout = 5000
            };
            //Send.
            await Task.Run(() => client.Send(mail));
            //Return true (access operation).
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            //Return false (failed operation).
            return false;
        }
    }*/
//#endif
}
