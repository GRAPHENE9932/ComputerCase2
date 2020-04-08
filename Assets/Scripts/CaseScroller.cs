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

    /// <summary>
    /// Starts case.
    /// </summary>
    public void StartCase()
    {
        switch (caseType)
        {
            case ComponentType.CPU:
                for (int i = 0; i < 50; i++)
                {
                    Rarity rarity;
                    PCComponent componentToInstantiate;
                    //Randomizing rarity.
                    float rand = Random.Range(0F, 1F);
                    //Bad.
                    if (rand < .8)
                    {
                        int rand2 = Random.Range(0, CPUs[0].Count - 1);
                        componentToInstantiate = CPUs[0][rand2];
                        rarity = Rarity.Bad;
                    }
                    //Middle.
                    else if (rand < .95)
                    {
                        int rand2 = Random.Range(0, CPUs[1].Count - 1);
                        componentToInstantiate = CPUs[1][rand2];
                        rarity = Rarity.Middle;
                    }
                    //Good.
                    else if (rand < .99)
                    {
                        int rand2 = Random.Range(0, CPUs[2].Count - 1);
                        componentToInstantiate = CPUs[2][rand2];
                        rarity = Rarity.Good;
                    }
                    //Very good.
                    else if (rand < .9975)
                    {
                        int rand2 = Random.Range(0, CPUs[3].Count - 1);
                        componentToInstantiate = CPUs[3][rand2];
                        rarity = Rarity.VeryGood;
                    }
                    //Top.
                    else
                    {
                        int rand2 = Random.Range(0, CPUs[4].Count - 1);
                        componentToInstantiate = CPUs[4][rand2];
                        rarity = Rarity.Top;
                    }
                    //Instantiating case cell.
                    GameObject cell = Instantiate(cellPrefab, cellsGroup);
                    //Settings for cell.
                    cell.GetComponent<Cell>().component = componentToInstantiate;
                    cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
                    cell.GetComponent<Cell>().image.sprite = componentToInstantiate.image;
                }
                break;
                //Analogic to CPU.
            case ComponentType.GPU:
                for (int i = 0; i < 50; i++)
                {
                    Rarity rarity;
                    PCComponent componentToInstantiate;
                    float rand = Random.Range(0F, 1F);
                    if (rand < .8)
                    {
                        int rand2 = Random.Range(0, GPUs[0].Count);
                        componentToInstantiate = GPUs[0][rand2];
                        rarity = Rarity.Bad;
                    }
                    else if (rand < .95)
                    {
                        int rand2 = Random.Range(0, GPUs[1].Count - 1);
                        componentToInstantiate = GPUs[1][rand2];
                        rarity = Rarity.Middle;
                    }
                    else if (rand < .99)
                    {
                        int rand2 = Random.Range(0, GPUs[2].Count - 1);
                        componentToInstantiate = GPUs[2][rand2];
                        rarity = Rarity.Good;
                    }
                    else if (rand < .9975)
                    {
                        int rand2 = Random.Range(0, GPUs[3].Count - 1);
                        componentToInstantiate = GPUs[3][rand2];
                        rarity = Rarity.VeryGood;
                    }
                    else
                    {
                        int rand2 = Random.Range(0, GPUs[4].Count - 1);
                        componentToInstantiate = GPUs[4][rand2];
                        rarity = Rarity.Top;
                    }
                    GameObject cell = Instantiate(cellPrefab, cellsGroup);
                    cell.GetComponent<Cell>().component = componentToInstantiate;
                    cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
                    cell.GetComponent<Cell>().image.sprite = componentToInstantiate.image;
                }
                break;
            case ComponentType.RAM:
                for (int i = 0; i < 50; i++)
                {
                    Rarity rarity;
                    PCComponent componentToInstantiate;
                    float rand = Random.Range(0F, 1F);
                    if (rand < .8)
                    {
                        int rand2 = Random.Range(0, RAMs[0].Count - 1);
                        componentToInstantiate = RAMs[0][rand2];
                        rarity = Rarity.Bad;
                    }
                    else if (rand < .95)
                    {
                        int rand2 = Random.Range(0, RAMs[1].Count - 1);
                        componentToInstantiate = RAMs[1][rand2];
                        rarity = Rarity.Middle;
                    }
                    else if (rand < .99)
                    {
                        int rand2 = Random.Range(0, RAMs[2].Count - 1);
                        componentToInstantiate = RAMs[2][rand2];
                        rarity = Rarity.Good;
                    }
                    else if (rand < .9975)
                    {
                        int rand2 = Random.Range(0, RAMs[3].Count - 1);
                        componentToInstantiate = RAMs[3][rand2];
                        rarity = Rarity.VeryGood;
                    }
                    else
                    {
                        int rand2 = Random.Range(0, RAMs[4].Count - 1);
                        componentToInstantiate = RAMs[4][rand2];
                        rarity = Rarity.Top;
                    }
                    GameObject cell = Instantiate(cellPrefab, cellsGroup);
                    cell.GetComponent<Cell>().component = componentToInstantiate;
                    cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
                    cell.GetComponent<Cell>().image.sprite = componentToInstantiate.image;
                }
                break;
            case ComponentType.Motherboard:
                for (int i = 0; i < 50; i++)
                {
                    Rarity rarity;
                    PCComponent componentToInstantiate;
                    float rand = Random.Range(0F, 1F);
                    if (rand < .8)
                    {
                        int rand2 = Random.Range(0, motherboards[0].Count - 1);
                        componentToInstantiate = motherboards[0][rand2];
                        rarity = Rarity.Bad;
                    }
                    else if (rand < .95)
                    {
                        int rand2 = Random.Range(0, motherboards[1].Count - 1);
                        componentToInstantiate = motherboards[1][rand2];
                        rarity = Rarity.Middle;
                    }
                    else if (rand < .99)
                    {
                        int rand2 = Random.Range(0, motherboards[2].Count - 1);
                        componentToInstantiate = motherboards[2][rand2];
                        rarity = Rarity.Good;
                    }
                    else if (rand < .9975)
                    {
                        int rand2 = Random.Range(0, motherboards[3].Count - 1);
                        componentToInstantiate = motherboards[3][rand2];
                        rarity = Rarity.VeryGood;
                    }
                    else
                    {
                        int rand2 = Random.Range(0, motherboards[4].Count - 1);
                        componentToInstantiate = motherboards[4][rand2];
                        rarity = Rarity.Top;
                    }
                    GameObject cell = Instantiate(cellPrefab, cellsGroup);
                    cell.GetComponent<Cell>().component = componentToInstantiate;
                    cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
                    cell.GetComponent<Cell>().image.sprite = componentToInstantiate.image;
                }
                break;
            case ComponentType.All:
                for (int i = 0; i < 50; i++)
                {
                    Rarity rarity;
                    PCComponent componentToInstantiate;
                    float rand = Random.Range(0F, 1F);
                    if (rand < .8)
                    {
                        int rand2 = Random.Range(0, CPUs[0].Count - 1);
                        componentToInstantiate = CPUs[0][rand2];
                        rarity = Rarity.Bad;
                    }
                    else if (rand < .95)
                    {
                        int rand2 = Random.Range(0, CPUs[1].Count - 1);
                        componentToInstantiate = CPUs[1][rand2];
                        rarity = Rarity.Middle;
                    }
                    else if (rand < .99)
                    {
                        int rand2 = Random.Range(0, CPUs[2].Count - 1);
                        componentToInstantiate = CPUs[2][rand2];
                        rarity = Rarity.Good;
                    }
                    else if (rand < .9975)
                    {
                        int rand2 = Random.Range(0, CPUs[3].Count - 1);
                        componentToInstantiate = CPUs[3][rand2];
                        rarity = Rarity.VeryGood;
                    }
                    else
                    {
                        int rand2 = Random.Range(0, CPUs[4].Count - 1);
                        componentToInstantiate = CPUs[4][rand2];
                        rarity = Rarity.Top;
                    }
                    GameObject cell = Instantiate(cellPrefab, cellsGroup);
                    cell.GetComponent<Cell>().component = componentToInstantiate;
                    cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
                    cell.GetComponent<Cell>().image.sprite = componentToInstantiate.image;
                }
                break;
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
