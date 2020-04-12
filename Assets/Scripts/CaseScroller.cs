using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum ComponentType
{
    CPU, GPU, RAM, Motherboard, All
}

public class CaseScroller : MonoBehaviour
{
    public float speed;

    public RectTransform cellsGroup;
    public GameObject cellPrefab;
    public ComponentType caseType;
    public void Casetype(int caseType)
    {
        this.caseType = (ComponentType)caseType;
    }
    public List<PCComponent>[] CPUs, GPUs, RAMs, motherboards;
    public Color[] rarityColors;

    //Chances:
    //Top: 0.25%;
    //VeryGood: 0.75%;
    //Good: 4%;
    //Middle: 15%;
    //Bad: 80%;

    private void Awake()
    {
        //Loading arrays of components from resourceds.
        //Arrays initialisation.
        CPUs = new List<PCComponent>[5];
        GPUs = new List<PCComponent>[5];
        RAMs = new List<PCComponent>[5];
        motherboards = new List<PCComponent>[5];
        //List in arrays initialization.
        for (int i = 0; i < 5; i++)
        {
            CPUs[i] = new List<PCComponent>();
            GPUs[i] = new List<PCComponent>();
            RAMs[i] = new List<PCComponent>();
            motherboards[i] = new List<PCComponent>();
        }
        //CPUs loading
        PCComponent[] tmp = Resources.LoadAll<PCComponent>("CPU");
        //Add CPUs to array of lists
        for (int i = 0; i < tmp.Length; i++)
        {
            CPUs[(int)tmp[i].rarity].Add(tmp[i]);
        }
        //GPUs loading
        tmp = Resources.LoadAll<PCComponent>("GPU");
        //Add GPUs to array of lists
        for (int i = 0; i < tmp.Length; i++)
        {
            GPUs[(int)tmp[i].rarity].Add(tmp[i]);
        }
        //RAMs loading
        tmp = Resources.LoadAll<PCComponent>("RAM");
        //Add RAMs to array of lists
        for (int i = 0; i < tmp.Length; i++)
        {
            RAMs[(int)tmp[i].rarity].Add(tmp[i]);
        }
        //motherboards loading
        tmp = Resources.LoadAll<PCComponent>("Motherboard");
        //Add motherboards to array of lists
        for (int i = 0; i < tmp.Length; i++)
        {
            motherboards[(int)tmp[i].rarity].Add(tmp[i]);
        }
    }

    private void SpawnCell(ComponentType type)
    {
        if (type != ComponentType.All)
        {

            float randRarity = Random.Range(0F, 1F);
            Rarity rarity;
            PCComponent component = null;
            int randComponent;

            if (randRarity < .8F)
                rarity = Rarity.Bad;
            else if (randRarity < .95F)
                rarity = Rarity.Middle;
            else if (randRarity < .99F)
                rarity = Rarity.Good;
            else if (randRarity < .9975F)
                rarity = Rarity.VeryGood;
            else
                rarity = Rarity.Top;

            switch (type)
            {
                case ComponentType.CPU:
                    randComponent = Random.Range(0, CPUs[(int)rarity].Count - 1);
                    component = CPUs[(int)rarity][randComponent];
                    break;
                case ComponentType.GPU:
                    randComponent = Random.Range(0, GPUs[(int)rarity].Count - 1);
                    component = GPUs[(int)rarity][randComponent];
                    break;
                case ComponentType.RAM:
                    randComponent = Random.Range(0, RAMs[(int)rarity].Count - 1);
                    component = RAMs[(int)rarity][randComponent];
                    break;
                case ComponentType.Motherboard:
                    randComponent = Random.Range(0, motherboards[(int)rarity].Count - 1);
                    component = motherboards[(int)rarity][randComponent];
                    break;
            }

            GameObject cell = Instantiate(cellPrefab, cellsGroup);
            cell.GetComponent<Cell>().component = component;
            cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
            cell.GetComponent<Cell>().image.sprite = component.image;
        }
        else
        {
            SpawnCell((ComponentType)Random.Range(0, 3));
        }
    }

    /// <summary>
    /// Starts case.
    /// </summary>
    public void StartCase()
    {
        for (int i = 0; i < 50; i++)
        {
            SpawnCell(caseType);
        }
        speed = Random.Range(90F, 109.3F);
        StartCoroutine(CaseScroll());
    }

    private IEnumerator CaseScroll()
    {
        while (speed > 0)
        {
            cellsGroup.Translate(speed * Time.deltaTime * -50, 0, 0);
            speed -= Time.deltaTime * 20;
            yield return null;
        }
    }
}
