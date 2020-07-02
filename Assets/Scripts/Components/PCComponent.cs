using System;
using System.Reflection;
using UnityEngine;

public enum Rarity
{
    Bad, Common, Good, VeryGood, Top
}

[CreateAssetMenu(menuName = "Components", fileName = "Default component")]
[Serializable]
public class PCComponent : ScriptableObject, ICloneable
{
    public string fullName;
    public string shortName;
    public int price;
    public DateTime time;
    public Rarity rarity;
    [KlimSoft.Serializer.Ignore]
    public Sprite image;
    public string imageName;

    public PCComponent()
    {
        RegenerateImage();
    }

    public void RegenerateImage() 
    {
        //If no image, but has name, load image by name.
        if (image == null && !string.IsNullOrEmpty(imageName))
        {
            string resPath;
            if (this is CPU)
                resPath = "Component textures/CPU/" + imageName;
            else if (this is GPU)
                resPath = "Component textures/GPU/" + imageName;
            else if (this is RAM)
                resPath = "Component textures/RAM/" + imageName;
            else if (this is Motherboard)
                resPath = "Component textures/Motherboard/" + imageName;
            else
                throw new Exception("Error! Code 12. Tried to regenerate clear PCComponent.");

            image = Resources.Load<Sprite>(resPath);
        }
        //If has image, update the name.
        if (image != null)
            imageName = image.name;
    }

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
    public object Clone()
    {
        this.RegenerateImage();
        Type type = this.GetType();
        object res = ScriptableObject.CreateInstance(type);
        FieldInfo[] fields = res.GetType().GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            fields[i].SetValue(res, fields[i].GetValue(this));
        }

        return res;

        /*
        PCComponent component = ScriptableObject.CreateInstance<PCComponent>();
        component.fullName = this.fullName;
        component.shortName = this.shortName;
        component.price = this.price;
        component.time = this.time;
        component.rarity = this.rarity;
        component.image = this.image;
        component.imageName = this.imageName;

        return component;
        */
    }
}
