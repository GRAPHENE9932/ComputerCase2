﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeScript : MonoBehaviour
{
    /// <summary>
    /// Red image in update button. Used for indication of errors.
    /// </summary>
    public Image redUpdateImage;
    /// <summary>
    /// Rect transform of icon of update button.
    /// </summary>
    public RectTransform updateIcon;
    /// <summary>
    /// Text that indicates the price of BTC in USD or USD in BTC.
    /// </summary>
    public Text priceText;
    /// <summary>
    /// Text close to priceText, indicates the currency (for example, "$=");
    /// </summary>
    public Text currencyIconText;
    /// <summary>
    /// Field, where player type money to exchange.
    /// </summary>
    public InputField fromField;
    /// <summary>
    /// Text, that displays money, player will get after exchange.
    /// </summary>
    public Text toText;
    public Text fromFieldPlaceholder;
    public MoneySystem moneySystem;

    private decimal BTC = -1;
    /// <summary>
    /// Is bitcoin price updating now?
    /// </summary>
    private bool updating;
    private bool showCurrencyOfBTC = true;
    private bool exchangeBTCToUSD = true;
    private string previousFromField;

    private void Start()
    {
        StartCoroutine(UpdateBTC());
    }
    /// <summary>
    /// Event of clicked update button.
    /// </summary>
    public void UpdateClicked()
    {
        StartCoroutine(UpdateBTC());
    }

    /// <summary>
    /// Event of clicked currency switch button with text, for example, "$=".
    /// </summary>
    public void CurrencyIconClicked()
    {
        showCurrencyOfBTC = !showCurrencyOfBTC;
        CurrencyTextUpdate();
    }

    /// <summary>
    /// Event of clicked Max button.
    /// </summary>
    public void MaxClicked()
    {
        if (exchangeBTCToUSD)
        {
            fromField.text = moneySystem.BTCMoney.Value.ToString().TakeChars(20) + "₿";
        }
        else
        {
            fromField.text = $"{moneySystem.Money.Value}$";
        }
    }

    /// <summary>
    /// Event of clicked currency change button.
    /// </summary>
    public void CurrencyChangeClicked()
    {
        exchangeBTCToUSD = !exchangeBTCToUSD;

        if (exchangeBTCToUSD)
        {
            fromFieldPlaceholder.text = "₿";
            fromField.text = null;
            fromField.contentType = InputField.ContentType.DecimalNumber;
            toText.text = "$";
        }
        else
        {
            fromFieldPlaceholder.text = "$";
            fromField.text = null;
            fromField.contentType = InputField.ContentType.IntegerNumber;
            toText.text = "₿";
        }
    }

    /// <summary>
    /// Event of fromField text changed.
    /// </summary>
    public void FromFieldChanged()
    {
        if (fromField.text != null && fromField.text != "")
        {
            if (exchangeBTCToUSD)
            {
                if (decimal.TryParse(fromField.text, out decimal num) && num <= moneySystem.BTCMoney.Value)
                    //Floor the value of dollars.
                    toText.text = $"{Math.Floor(num * BTC)}$";
                else
                    fromField.text = previousFromField;
            }
            else
            {
                if (long.TryParse(fromField.text, out long num) && num <= moneySystem.Money.Value)
                    //Take only 20 first symbols in the number of bitcoins to fit in the text component.
                    toText.text = (num * (1m / BTC)).ToString().TakeChars(20) + "₿";
                else
                    fromField.text = previousFromField;
            }
        }
        else
        {
            toText.text = exchangeBTCToUSD ? "$" : "₿";
        }
        previousFromField = fromField.text;
    }

    /// <summary>
    /// Event of clicked exchange button.
    /// </summary>
    public void ExchangeClicked()
    {
        if (fromField.text != null && fromField.text != "")
        {
            if (exchangeBTCToUSD)
            {
                if (decimal.TryParse(fromField.text, out decimal num) && num <= moneySystem.BTCMoney.Value)
                {
                    moneySystem.BTCMoney -= num;
                    moneySystem.Money += Convert.ToInt64(Math.Floor(num * BTC));
                }
            }
            else
            {
                if (long.TryParse(fromField.text, out long num) && num <= moneySystem.Money.Value)
                {
                    moneySystem.Money -= num;
                    moneySystem.BTCMoney += num * (1m / BTC);
                }
            }
        }
    }

    /// <summary>
    /// Update currency icon text and price text.
    /// </summary>
    private void CurrencyTextUpdate()
    {
        if (showCurrencyOfBTC)
        {
            if (BTC == -1)
                priceText.text = "no data";
            else
                priceText.text = $"{Math.Round(BTC, 2)}$";
            currencyIconText.text = "₿=";
        }
        else
        {
            if (BTC == -1)
                priceText.text = "no data";
            else
                //Take only first 31 chars of this number to fit it in the text component.
                priceText.text = (1 / BTC).ToString().TakeChars(31) + "₿";
            currencyIconText.text = "$=";
        }
    }

    /// <summary>
    /// Coroutine of updating progress. Rotates the update icon.
    /// </summary>
    private IEnumerator UpdateBTC()
    {
        updating = true;
        try
        {
            //Create the web client.
            WebClient client = new WebClient
            {
                UseDefaultCredentials = true
            };
            //Add function to download event.
            client.DownloadStringCompleted += DownloadCompleted;
            //Download price of 1E12 USD in bitcoins. This method gives max precision.
            client.DownloadStringAsync(new Uri("https://blockchain.info/tobtc?currency=USD&value=1000000000000"));
        }
        catch
        {
            updating = false;
            //If error, start red indicator on update button.
            StartCoroutine(ErrorUpdateAnim());
        }
        while (updating)
        {
            //Rotate update icon while updating.
            updateIcon.Rotate(0, 0, Time.deltaTime * -1000);
            //1 frame waiting.
            yield return null;
        }
    }

    /// <summary>
    /// Function of event of downloading completed.
    /// </summary>
    private void DownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        try
        {
            //Set bitcoin price in USD.
            BTC = 1E12m / decimal.Parse(e.Result);
            CurrencyTextUpdate();
        }
        catch
        {
            //If error, start red indicator on update button.
            StartCoroutine(ErrorUpdateAnim());
        }
        updating = false;
    }

    /// <summary>
    /// Animation of red indicator in update button.
    /// </summary>
    private IEnumerator ErrorUpdateAnim()
    {
        //0 -> 1 during 0.1 sec.
        //1 -> 0 during 0.2 sec.
        //0 -> 1 during 0.1 sec.
        //1 -> 0 during 0.2 sec.
        float time;
        for (int i = 0; i < 2; i++)
        {
            time = 0;
            while (time < 0.1F)
            {
                redUpdateImage.color = new Color(1, 0, 0, time * 10);
                time += Time.deltaTime;
                yield return null;
            }
            time = 0.2F;
            while (time > 0)
            {
                redUpdateImage.color = new Color(1, 0, 0, time * 5);
                time -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
