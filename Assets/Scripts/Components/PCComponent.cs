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
    public virtual string FullProperties
    {
        get
        {
            return "Error! Code 0.";
        }
    }
    /// <summary>
    /// Not all properties of component in one string.
    /// </summary>
    public virtual string ShortProperties
    {
        get
        {
            return "Error! Code 0.";
        }
    }
    /// <summary>
    /// Clones this object.
    /// </summary>
    /// <returns>
    /// Clonned object.
    /// </returns>
    public virtual PCComponent Clone()
    {
        PCComponent component = ScriptableObject.CreateInstance<PCComponent>();
        component.fullName = this.fullName;
        component.shortName = this.shortName;
        component.price = this.price;
        component.time = this.time;
        component.rarity = this.rarity;
        component.image = this.image;

        return component;
    }
}
