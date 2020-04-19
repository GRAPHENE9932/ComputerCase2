using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneySystem : MonoBehaviour
{
    public static SecureLong money;

    public Text moneyText;

    public void UpdateMoney()
    {
        moneyText.text = money.Value + "$";
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
}