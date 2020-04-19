using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum ComponentType
{
    CPU, GPU, RAM, Motherboard, All
}

public class CaseScroller : MonoBehaviour
{
    private float speed;
    private readonly Cell[] cells = new Cell[50];

    public PCComponent currentComponent;
    public RectTransform cellsGroup;
    public GameObject cellPrefab;
    public ComponentType caseType;

    public CursorScript cursor;
    public Invertory invertory;

    public Animation dropAnim;
    public Image dropImage;
    public Text dropProperties, sellText;
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
    /// Spawn cell.
    /// </summary>
    /// <param name="type">
    /// The type of component to spawn.
    /// </param>
    private void SpawnCell(ComponentType type, int index)
    {
        if (type != ComponentType.All)
        {
            //Floating number, used for choossing rarity of spawning component.
            float randRarity = Random.Range(0F, 1F);
            Rarity rarity;
            //Component to spawn.
            PCComponent component = null;
            //Index of component in component array.
            int randComponent;

            //Choose rarity.
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

            //Switch type.
            switch (type)
            {
                case ComponentType.CPU:
                    //Randomizing index.
                    randComponent = Random.Range(0, CPUs[(int)rarity].Count - 1);
                    //Choosing component from array.
                    component = CPUs[(int)rarity][randComponent];
                    break;
                case ComponentType.GPU:
                    //Analogic to CPU.
                    randComponent = Random.Range(0, GPUs[(int)rarity].Count - 1);
                    component = GPUs[(int)rarity][randComponent];
                    break;
                case ComponentType.RAM:
                    //Analogic to CPU.
                    randComponent = Random.Range(0, RAMs[(int)rarity].Count - 1);
                    component = RAMs[(int)rarity][randComponent];
                    break;
                case ComponentType.Motherboard:
                    //Analogic to CPU.
                    randComponent = Random.Range(0, motherboards[(int)rarity].Count - 1);
                    component = motherboards[(int)rarity][randComponent];
                    break;
            }

            GameObject cell;
            if (cells[index] == null)
            {
                //Cell instantiate.
                cell = Instantiate(cellPrefab, cellsGroup);
                cell.GetComponent<Cell>().component = component;
                cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
                cell.GetComponent<Cell>().image.sprite = component.image;
                cells[index] = cell.GetComponent<Cell>();
            }
            else
            {
                //Use instantiated cell.
                cell = cells[index].gameObject;
                cell.GetComponent<Cell>().component = component;
                cell.GetComponent<Cell>().rarityLine.color = rarityColors[(int)rarity];
                cell.GetComponent<Cell>().image.sprite = component.image;
            }
        }
        else
        {
            //If component type == all, spawn random component.
            SpawnCell((ComponentType)Random.Range(0, 4), index);
        }
    }

    /// <summary>
    /// Starts case.
    /// </summary>
    public void StartCase()
    {
        //Spawn 50 cells
        for (int i = 0; i < 50; i++)
        {
            SpawnCell(caseType, i);
        }
        //Randomizing speed.
        speed = Random.Range(90F, 109.3F);
        //Start coroutine of case scrolling.
        StartCoroutine(CaseScroll());
    }

    private IEnumerator CaseScroll()
    {
        cellsGroup.localPosition = new Vector2(8375, 0);
        while (speed > 0)
        {
            //Move cells with speed.
            cellsGroup.Translate(speed * Time.deltaTime * -50, 0, 0);
            //Speed decreasing.
            speed -= Time.deltaTime * 20;
            //Waiting 1 frame.
            yield return null;
        }
        CaseStopped();
    }

    private void CaseStopped()
    {
        currentComponent = cursor.currentComponent;
        dropImage.sprite = currentComponent.image;
        dropProperties.text = currentComponent.Properties;
        sellText.text = "Sell (" + currentComponent.price / 20 + "$)";
        invertory.components.Add(currentComponent);
        dropAnim.gameObject.SetActive(true);
        dropAnim.Play("OpenDroppedComponent");
    }
}
