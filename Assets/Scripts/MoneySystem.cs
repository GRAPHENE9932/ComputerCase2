using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneySystem : MonoBehaviour
{
    public Text moneyText;
    public Image moneyImage;

    private SecureLong money;
    public SecureLong Money
    {
        get
        {
            return money;
        }
        set
        {
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
    private IEnumerator MoneyColor(bool green)
    {
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
            return value - offset;
        }
        set
        {
            long temp = this.value - offset;
            offset = Random.Range(int.MinValue, int.MaxValue);
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