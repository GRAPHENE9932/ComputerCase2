using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Bad, Middle, Good, VeryGood, Top
}

[CreateAssetMenu(menuName = "Components", fileName = "Default component")]
public class PCComponent : ScriptableObject
{
    public string fullName;
    public string shortName;
    public int price;
    public System.DateTime time;
    public Rarity rarity;
    public Sprite image;
    /// <summary>
    /// All properties of component in one string.
    /// </summary>
    public virtual string Properties
    {
        get
        {
            return "Error! Code 0.";
        }
    }
}
