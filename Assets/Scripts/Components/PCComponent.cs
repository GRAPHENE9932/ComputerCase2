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
    public int price;
    public System.DateTime time;
    public Rarity rarity;
    public Sprite image;
}
