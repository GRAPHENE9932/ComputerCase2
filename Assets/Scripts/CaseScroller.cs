using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum ComponentType
{
    CPU, GPU, RAM, Motherboard, All
}

public class CaseScroller : MonoBehaviour
{
    public RectTransform cellsGroup;
    public GameObject cellPrefab;
    public ComponentType caseType;
    public List<PCComponent>[] CPUs, GPUs, RAMs, motherboards;
    public Color[] rarityColors;

    //Top: 0.25575%;
    //VeryGood: 0.63939%;
    //Good: 3.19693%;
    //Middle: 15.98465%;
    //Bad: 79.92327%;

    private void Start()
    {
        PCComponent[] tmp = Resources.LoadAll<PCComponent>("CPU");
        for (int i = 0; i < tmp.Length; i++)
            CPUs[(int)tmp[i].rarity].Add(tmp[i]);
        tmp = Resources.LoadAll<PCComponent>("GPU");
        for (int i = 0; i < tmp.Length; i++)
            GPUs[(int)tmp[i].rarity].Add(tmp[i]);
        tmp = Resources.LoadAll<PCComponent>("RAM");
        for (int i = 0; i < tmp.Length; i++)
            RAMs[(int)tmp[i].rarity].Add(tmp[i]);
        tmp = Resources.LoadAll<PCComponent>("Motherboard");
        for (int i = 0; i < tmp.Length; i++)
            motherboards[(int)tmp[i].rarity].Add(tmp[i]);
    }

    public void StartCase()
    {
        switch (caseType)
        {
            case ComponentType.CPU:
                for (int i = 0; i < 50; i++)
                {
                    PCComponent componentToInstantiate;
                    float rand = Random.Range(0F, 1F);
                    if (rand < .7992327)
                    {
                        int rand2 = Random.Range(0, CPUs[0].Count);
                        componentToInstantiate = CPUs[0][rand2];
                    }
                    else if (rand < .9590792)
                    {
                        int rand2 = Random.Range(0, CPUs[1].Count);
                        componentToInstantiate = CPUs[1][rand2];
                    }
                    else if (rand < .9910485)
                    {
                        int rand2 = Random.Range(0, CPUs[2].Count);
                        componentToInstantiate = CPUs[2][rand2];
                    }
                    else if (rand < .9974424)
                    {
                        int rand2 = Random.Range(0, CPUs[3].Count);
                        componentToInstantiate = CPUs[3][rand2];
                    }
                    else
                    {
                        int rand2 = Random.Range(0, CPUs[4].Count);
                        componentToInstantiate = CPUs[4][rand2];
                    }
                    GameObject cell = Instantiate(cellPrefab);
                    cell.GetComponent<Cell>().component = componentToInstantiate;
                    cell.GetComponent<Cell>().rarityLine.color = rarityColors[0];
                    cell.GetComponent<Cell>().image.sprite = componentToInstantiate.image;
                }
                break;
            case ComponentType.GPU:
                for (int i = 0; i < 50; i++)
                {

                }
                break;
            case ComponentType.RAM:
                for (int i = 0; i < 50; i++)
                {

                }
                break;
            case ComponentType.Motherboard:
                for (int i = 0; i < 50; i++)
                {

                }
                break;
            case ComponentType.All:
                for (int i = 0; i < 50; i++)
                {

                }
                break;
        }
    }

    private IEnumerator CaseScrolling()
    {
        
    }
}
