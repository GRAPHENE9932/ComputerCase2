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
    /// Background of money text.
    /// </summary>
    public Image moneyImage;

    /// <summary>
    /// Money value.
    /// </summary>
    private SecureLong money;
    public SecureLong Money
    {
        get
        {
            return money;
        }
        set
        {
            //At changing number of money, start color animation.
            if (value > money)
                StartCoroutine(MoneyColor(true));
            else if (value < money)
                StartCoroutine(MoneyColor(false));
            money = value;
        }
    }

    private void Start()
    {
        Money += 2000;
        UpdateMoney();
    }
    public void UpdateMoney()
    {
        moneyText.text = "$" + money.Value;
    }
    /// <summary>
    /// Color animation of money.
    /// </summary>
    /// <param name="green">True - green color, false - red color.</param>
    private IEnumerator MoneyColor(bool green)
    {
        //Total duration: 1 sec.
        float time = 0F;
        while (time < 1F)
        {
            if (green)
                moneyImage.color = new Color(0F, time, 0F);
            else
                moneyImage.color = new Color(time, 0F, 0F);
            time += Time.deltaTime * 2F;
            yield return null;
        }
        UpdateMoney();
        while (time > 0F)
        {
            if (green)
                moneyImage.color = new Color(0F, time, 0F);
            else
                moneyImage.color = new Color(time, 0F, 0F);
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