using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneySystem : MonoBehaviour
{
    /// <summary>
    /// Text shows money number.
    /// </summary>
    public Text moneyText;
    /// <summary>
    /// Text shows Bitcoin money number.
    /// </summary>
    public Text btcMoneyText;
    /// <summary>
    /// Background of money text.
    /// </summary>
    public Image moneyImage, btcMoneyImage;

    public static MoneySystem singletone;

    /// <summary>
    /// Money value.
    /// </summary>
    private static SecureLong money;
    /// <summary>
    /// Bitcoin money value.
    /// </summary>
    private static SecureDecimal btcMoney;
    public static SecureLong Money
    {
        get
        {
            return money;
        }
        set
        {
            //At changing number of money, start color animation.
            if (value > money)
                singletone.StartCoroutine(singletone.MoneyColor(true, true));
            else if (value < money)
                singletone.StartCoroutine(singletone.MoneyColor(false, true));
            money = value;
        }
    }
    public static SecureDecimal BTCMoney
    {
        get
        {
            return btcMoney;
        }
        set
        {
            //At changing number of money, start color animation.
            if (value > btcMoney)
                singletone.StartCoroutine(singletone.MoneyColor(true, false));
            else if (value < btcMoney)
                singletone.StartCoroutine(singletone.MoneyColor(false, false));
            btcMoney = value;
        }
    }

    private void Awake()
    {
        singletone = this;
        ApplySaves();
    }

    public static void ApplySaves()
    {
        Money = new SecureLong(GameSaver.Saves.money);
        BTCMoney = new SecureDecimal(GameSaver.Saves.BTCMoney);
    }

    private void Start()
    {
        Money += 200000;
        UpdateMoney();
    }

    public void UpdateMoney()
    {
        moneyText.text = $"${money.Value}";
        btcMoneyText.text = $"₿{System.Math.Round(btcMoney.Value, 15)}";
    }
    /// <summary>
    /// Color animation of money.
    /// </summary>
    /// <param name="green">True - green color, false - red color.</param>
    /// <param name="dollars">True - dollars, false - Bitcoins.</param>
    private IEnumerator MoneyColor(bool green, bool dollars)
    {
        Image targetImage = dollars ? moneyImage : btcMoneyImage;
        //Total duration: 1 sec.
        float time = 0F;
        while (time < 1F)
        {
            if (green)
                targetImage.color = new Color(0F, time, 0F);
            else
                targetImage.color = new Color(time, 0F, 0F);
            time += Time.deltaTime * 2F;
            yield return null;
        }
        UpdateMoney();
        while (time > 0F)
        {
            if (green)
                targetImage.color = new Color(0F, time, 0F);
            else
                targetImage.color = new Color(time, 0F, 0F);
            time -= Time.deltaTime * 2F;
            yield return null;
        }
    }
}

/// <summary>
/// Long type, secured from memory analysis.
/// </summary>
public struct SecureLong
{
    private long value;
    private int offset;

    public SecureLong(long value)
    {
        offset = Random.Range(int.MinValue, int.MaxValue);
        value += offset;
        this.value = value;
    }

    public long Value
    {
        get
        {
            //Just subtract offset from value.
            return value - offset;
        }
        set
        {
            //Temp is real value.
            long temp = this.value - offset;
            //Create new offset.
            offset = Random.Range(int.MinValue, int.MaxValue);
            //Set new value with new offset.
            this.value = temp + offset;
        }
    }

    public static SecureLong operator+(SecureLong s1, long u2)
    {
        return new SecureLong(s1.Value + u2);
    }
    public static SecureLong operator-(SecureLong s1, long u2)
    {
        return new SecureLong(s1.Value - u2);
    }
    public static bool operator==(SecureLong s1, SecureLong s2)
    {
        return (s1.value - s1.offset) == (s2.value - s2.offset);
    }
    public static bool operator !=(SecureLong s1, SecureLong s2)
    {
        return !(s1 == s2);
    }
    public static bool operator >(SecureLong s1, SecureLong s2)
    {
        return (s1.value - s1.offset) > (s2.value - s2.offset);
    }
    public static bool operator <(SecureLong s1, SecureLong s2)
    {
        return (s1.value - s1.offset) < (s2.value - s2.offset);
    }
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

/// <summary>
/// Decimal type, secured from memory analysis.
/// </summary>
public struct SecureDecimal
{
    private decimal value;
    private int offset;

    public SecureDecimal(decimal value)
    {
        offset = Random.Range(int.MinValue, int.MaxValue);
        value += offset;
        this.value = value;
    }

    public decimal Value
    {
        get
        {
            //Just subtract offset from value.
            return value - offset;
        }
        set
        {
            //Temp is real value.
            decimal temp = this.value - offset;
            //Create new offset.
            offset = Random.Range(int.MinValue, int.MaxValue);
            //Set new value with new offset.
            this.value = temp + offset;
        }
    }

    public static SecureDecimal operator +(SecureDecimal s1, decimal u2)
    {
        return new SecureDecimal(s1.Value + u2);
    }
    public static SecureDecimal operator -(SecureDecimal s1, decimal u2)
    {
        return new SecureDecimal(s1.Value - u2);
    }
    public static bool operator ==(SecureDecimal s1, SecureDecimal s2)
    {
        return (s1.value - s1.offset) == (s2.value - s2.offset);
    }
    public static bool operator !=(SecureDecimal s1, SecureDecimal s2)
    {
        return !(s1 == s2);
    }
    public static bool operator >(SecureDecimal s1, SecureDecimal s2)
    {
        return (s1.value - s1.offset) > (s2.value - s2.offset);
    }
    public static bool operator <(SecureDecimal s1, SecureDecimal s2)
    {
        return (s1.value - s1.offset) < (s2.value - s2.offset);
    }
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}