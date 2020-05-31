using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;

public class ErrorManager : MonoBehaviour
{
    public MessageBoxManager messageBox;
    public NavigationScript nav;

    //Фіксація і відображення помилок та виключень
#if !UNITY_EDITOR
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private async void HandleLog(string logString, string stackTrace, LogType type)
    {
        //If log type == error or exception.
        if (type == LogType.Error || type == LogType.Exception)
        {
            //Start message to player.
            messageBox.StartMessage("An error occured! Sending email about it...", 5);
            //Preparing text for email.
            string body = "Device name: " + SystemInfo.deviceName + "\n" +
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
                "Stack trace: " + stackTrace;
            //Send email.
            bool result = await Task.Run(() => SendEmail("Error in ComputerCase", body));
            //Start message about success or fail.
            if (result)
                messageBox.StartMessage("An error occured! Sending email about it...\nSuccess!", 5);
            else
                messageBox.StartMessage("An error occured! Sending email about it...\nFailed!", 5);
        }
    }

    private async Task<bool> SendEmail(string subject, string body)
    {
        try
        {
            //Initialize key and initialization vector for AES.
            byte[] key = Convert.FromBase64String("AMalpiCC/umTuLIRjeGuZI6xQIIfqyaWMV3T6koSqWo=");
            byte[] IV = Convert.FromBase64String("bJL3nvsWLODhPEaRYDKUnA==");
            Aes aes = Aes.Create();
            //Встановлення розміру ключа.
            aes.KeySize = key.Length * 8;
            //Встановлення самого ключа.
            aes.Key = key;
            //Встановлення вектора ініціалізації.
            aes.IV = IV;
            //Встановлення заповнення, якщо розмір даних не ділиться на розмір блока.
            aes.Padding = PaddingMode.PKCS7;
            //Встановлення режиму.
            aes.Mode = CipherMode.ECB;
            //Створення екземпляра інтерфейсу ICryptoTransform, який зашифрує дані.
            ICryptoTransform dec = aes.CreateDecryptor();
            //Отримання розшифрованого паролю.
            string pass = Encoding.UTF8.GetString(dec.TransformFinalBlock(Convert.FromBase64String("/IcwTYcAxKIc/jiO528mTQ=="), 0, Convert.FromBase64String("/IcwTYcAxKIc/jiO528mTQ==").Length));
            string email = Encoding.UTF8.GetString(dec.TransformFinalBlock(Convert.FromBase64String("Y6pdr5Oqj9c5kuJtBAy2kLo7sgEFOIz3IP2l9072BOE="), 0, Convert.FromBase64String("Y6pdr5Oqj9c5kuJtBAy2kLo7sgEFOIz3IP2l9072BOE=").Length));

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
        catch
        {
            //Return false (failed operation).
            return false;
        }
    }
#endif
}
