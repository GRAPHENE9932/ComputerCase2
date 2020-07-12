using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using KlimSoft;

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
    public HorizontalLayoutGroup columnsGroup;
    public GameObject columnPrefab;
    public Button exchangeButton;
    public ButtonAnimation exchangeButtonAnimation;
    public SoundManager soundManager;

    private decimal BTC = -1;
    /// <summary>
    /// Is bitcoin price updating now?
    /// </summary>
    private bool updating;
    private bool showCurrencyOfBTC = true;
    private bool exchangeBTCToUSD = true;
    private bool updateClickedByPlayer = false;
    private string previousFromField;

    private Image[] graphColumns;
    private decimal[] BTCHistory;
    private WebClient client;

    private void Start()
    {
        //Calculate columns count by formula: count = (W+s/w+s), where W is width of culumns group, w is width of one column and s is spacing.
        int columnCount = Mathf.FloorToInt((((RectTransform)columnsGroup.transform).rect.width + columnsGroup.spacing) / (100 + columnsGroup.spacing));
        //Initialize columns collection.
        graphColumns = new Image[columnCount];
        BTCHistory = new decimal[columnCount];
        //Instantiate columns.
        for (int i = 0; i < columnCount; i++)
            graphColumns[i] = Instantiate(columnPrefab, columnsGroup.transform).GetComponent<Image>();

        //Create the web client.
        client = new WebClient
        {
            UseDefaultCredentials = true
        };
        //Add function to download event.
        client.DownloadStringCompleted += DownloadCompleted;

        CurrencyTextUpdate();
        StartCoroutine(AutoUpdate());
    }
    private void OnDisable()
    {
        //Because exception can occur at quit.
        client.DownloadStringCompleted -= DownloadCompleted;
    }
    /// <summary>
    /// Event of clicked update button.
    /// </summary>
    public void UpdateClicked()
    {
        updateClickedByPlayer = true;
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
            fromField.text = MoneySystem.BTCMoney.Value.ToString().TakeChars(20) + "₿";
        }
        else
        {
            fromField.text = $"{MoneySystem.Money.Value}$";
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
        if (!string.IsNullOrEmpty(fromField.text) && BTC != -1)
        {
            if (exchangeBTCToUSD)
            {
                if (decimal.TryParse(fromField.text, out decimal num) && num <= MoneySystem.BTCMoney.Value)
                    //Floor the value of dollars.
                    toText.text = $"{Math.Floor(num * BTC)}$";
                else
                    fromField.text = previousFromField;
            }
            else
            {
                if (long.TryParse(fromField.text, out long num) && num <= MoneySystem.Money.Value)
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
        if (!string.IsNullOrEmpty(fromField.text) && BTC != -1)
        {
            if (exchangeBTCToUSD)
            {
                if (decimal.TryParse(fromField.text, out decimal num) && num <= MoneySystem.BTCMoney.Value)
                {
                    MoneySystem.BTCMoney -= num;
                    MoneySystem.Money += Convert.ToInt64(Math.Floor(num * BTC));

                    //Set text in field in bounds of balance.
                    if (num > MoneySystem.BTCMoney.Value)
                        fromField.text = MoneySystem.BTCMoney.ToString();

                    //Play sound if exchange > 0.
                    if (num > 0)
                        soundManager.PlayRandomSell();
                }
            }
            else
            {
                if (long.TryParse(fromField.text, out long num) && num <= MoneySystem.Money.Value)
                {
                    MoneySystem.Money -= num;
                    MoneySystem.BTCMoney += num * (1m / BTC);

                    //Set text in field in bounds of balance.
                    if (num > MoneySystem.Money.Value)
                        fromField.text = MoneySystem.Money.Value.ToString();

                    //Play sound if exchange > 0.
                    if (num > 0)
                        soundManager.PlayRandomSell();
                }
            }
        }
    }

    private void UpdateGraph()
    {
        //Offset the BTCHistory.
        for (int i = BTCHistory.Length - 1; i > 0; i--)
            BTCHistory[i] = BTCHistory[i - 1];
        BTCHistory[0] = BTC;

        //Find max and min.
        float maxInHistory = (float)MaxExcept(ref BTCHistory, 0);
        float minInHistory = (float)MinExcept(ref BTCHistory, 0);

        for (int i = 0; i < graphColumns.Length; i++)
            graphColumns[i].fillAmount = ((float)BTCHistory[i] - minInHistory) / (maxInHistory - minInHistory) * 0.98F + 0.02F;
    }

    /// <summary>
    /// Update currency icon text and price text.
    /// </summary>
    private void CurrencyTextUpdate()
    {
        if (showCurrencyOfBTC)
        {
            if (BTC == -1)
                priceText.text = LangManager.GetString("no_data");
            else
                priceText.text = $"{Math.Round(BTC, 2)}$";
            currencyIconText.text = "₿=";
        }
        else
        {
            if (BTC == -1)
                priceText.text = LangManager.GetString("no_data");
            else
                //Take only first 31 chars of this number to fit it in the text component.
                priceText.text = (1 / BTC).ToString().TakeChars(31) + "₿";
            currencyIconText.text = "$=";
        }
    }

    private IEnumerator AutoUpdate()
    {
        while (true)
        {
            updateClickedByPlayer = false;
            StartCoroutine(UpdateBTC());
            yield return new WaitForSeconds(15);
        }
    }

    /// <summary>
    /// Coroutine of updating progress. Rotates the update icon.
    /// </summary>
    private IEnumerator UpdateBTC()
    {
        try { 
            updating = true;
            //Download price of 1E12 USD in bitcoins. This method gives max precision.
            client.DownloadStringAsync(new Uri("https://blockchain.info/tobtc?currency=USD&value=1000000000000"));
        }
        catch (Exception exc)
        {
            //If error, start red indicator on update button.
            StartCoroutine(ErrorUpdateAnim());
            Debug.Log("Exc: " + exc.Message);
        }
        FromFieldChanged();
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
#if !UNITY_ENGINE && UNITY_ANDROID
            BTC = 1m / (decimal.Parse(e.Result.RemoveChar(',').Replace('.', ',')) / 1E12m);
#else
            BTC = 1m / (decimal.Parse(e.Result.RemoveChar(',')) / 1E12m);
#endif
            CurrencyTextUpdate();
            if (BTC != BTCHistory[0])
                UpdateGraph();
        }
        catch (Exception exc)
        {
            //If error, start red indicator on update button.
            StartCoroutine(ErrorUpdateAnim());
            if (updateClickedByPlayer)
                JavaTools.MakeToast("No internet!");
            Debug.Log("Exc: " + exc.Message);
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

    public static decimal MaxExcept(ref decimal[] array, decimal exception)
    {
        decimal max = decimal.MinValue;
        for (int i = 0; i < array.Length; i++)
            if (array[i] != exception && array[i] > max)
                max = array[i];
        return max;
    }

    public static decimal MinExcept(ref decimal[] array, decimal exception)
    {
        decimal min = decimal.MaxValue;
        for (int i = 0; i < array.Length; i++)
            if (array[i] != exception && array[i] < min)
                min = array[i];
        return min;
    }
}
