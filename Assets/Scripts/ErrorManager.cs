using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class ErrorManager : MonoBehaviour
{
    public MessageBoxManager messageBox;


    //Фіксація і відображення помилок та виключень
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            SendEmail("Error in ComputerCase", logString + "\n" + stackTrace);
        }
        messageBox.StartMessage("An error occured! The info about it has been sent to developer", 5);
    }

    private void SendEmail(string subject, string body)
    {
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

        MailMessage mail = new MailMessage(email, email, subject, body);
        SmtpClient client = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new System.Net.NetworkCredential(email, pass),
            EnableSsl = true
        };
        client.Send(mail);
    }
}
