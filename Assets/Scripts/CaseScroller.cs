using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// The type of component (CPU, GPU...).
/// </summary>
public enum ComponentType
{
    CPU, GPU, RAM, Motherboard, All
}
/// <summary>
/// The type of case (Common, Major...).
/// </summary>
public enum CaseType
{
    Common, Major, VeryGood, Top
}

public class CaseScroller : MonoBehaviour
{
    /// <summary>
    /// Speed of scrolling case.
    /// </summary>
    private float speed;
    /// <summary>
    /// Rarity of current case.
    /// </summary>
    private CaseType caseRarity;
    /// <summary>
    /// Cells of case.
    /// </summary>
    private readonly Cell[] cells = new Cell[50];

    /// <summary>
    /// Dropped component.
    /// </summary>
    public PCComponent currentComponent;
    /// <summary>
    /// The parent of cells.
    /// </summary>
    public RectTransform cellsGroup;
    /// <summary>
    /// The cell example.
    /// </summary>
    public GameObject cellPrefab;
    /// <summary>
    /// Current case type.
    /// </summary>
    public ComponentType caseType;

    /// <summary>
    /// Cursor script. Used for getting current cell.
    /// </summary>
    public CursorScript cursor;
    /// <summary>
    /// Inventory script.
    /// </summary>
    public Inventory inventory;
    /// <summary>
    /// Navigation script.
    /// </summary>
    public NavigationScript navigation;
    /// <summary>
    /// MoneySystem script.
    /// </summary>
    public MoneySystem moneySystem;
    /// <summary>
    /// MessageBox manager.
    /// </summary>
    public MessageBoxManager messageBox;

    /// <summary>
    /// Animation of drop.
    /// </summary>
    public Animation dropAnim;
    /// <summary>
    /// Image of drop.
    /// </summary>
    public Image dropImage;
    /// <summary>
    /// Drop properties of dropped component.
    /// </summary>
    public Text dropProperties;
    /// <summary>
    /// Text of sell button of drop.
    /// </summary>
    public Text sellText;
    /// <summary>
    /// Drop script.
    /// </summary>
    public DropScript drop;

    /// <summary>
    /// Is turned on debug fast mode?
    /// </summary>
    public bool fastMode;

    /// <summary>
    /// Is player has enought money for case open.
    /// </summary>
    public bool EnoughMoney
    {
        get
        {
            return caseRarity == CaseType.Common ||
            (caseRarity == CaseType.Major && moneySystem.Money.Value >= 50) ||
            (caseRarity == CaseType.VeryGood && moneySystem.Money.Value >= 750) ||
            (caseRarity == CaseType.Top && moneySystem.Money.Value >= 1500);
        }
    }

    /// <summary>
    /// The method for case buttons.
    /// </summary>
    /// <param name="caseType">Type of case.</param>
    public void ChangeCaseComponentType(int caseType)
    {
        this.caseType = (ComponentType)caseType;
    }
    /// <summary>
    /// The method for case buttons.
    /// </summary>
    /// <param name="caseRarity">Rarity of case.</param>
    public void ChangeCaseRarity(int caseRarity)
    {
        this.caseRarity = (CaseType)caseRarity;
    }
    /// <summary>
    /// Full list of exciting components.
    /// </summary>
    public List<PCComponent>[] CPUs, GPUs, RAMs, motherboards;
    /// <summary>
    /// Rarity colors.
    /// </summary>
    public Color[] rarityColors;
    /// <summary>
    /// Chances of selected rarity of case.
    /// </summary>
    public float[] commonRarity, majorRarity, veryGoodRarity, topRarity;

    //Chances:
    //Top: 0.25%;
    //VeryGood: 0.75%;
    //Good: 4%;
    //Middle: 15%;
    //Bad: 80%;

    /// <summary>
    /// Initialize lists of components (CPUs, GPUs...).
    /// </summary>
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

            float[] currentRarity;
            switch (caseRarity)
            {
                case CaseType.Common:
                    currentRarity = commonRarity;
                    break;
                case CaseType.Major:
                    currentRarity = majorRarity;
                    break;
                case CaseType.VeryGood:
                    currentRarity = veryGoodRarity;
                    break;
                case CaseType.Top:
                    currentRarity = topRarity;
                    break;
                default:
                    throw new System.Exception("Error! Code 11.");
            }
            //Choose rarity.
            if (randRarity < currentRarity[0])
                rarity = Rarity.Bad;
            else if (randRarity < currentRarity[1])
                rarity = Rarity.Middle;
            else if (randRarity < currentRarity[2])
                rarity = Rarity.Good;
            else if (randRarity < currentRarity[3])
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
        //Check, player has enought money for this case type.
        if (!EnoughMoney)
        {
            //Message player about not enought money.
            messageBox.StartMessage(LangManager.GetString("not_enough_money"), 2);
        }
        else
        {
            //Stats.
            switch (caseType)
            {
                case ComponentType.CPU:
                    StatisticsScript.CPUsDroppedByCases[(int)caseRarity]++;
                    break;
                case ComponentType.GPU:
                    StatisticsScript.GPUsDroppedByCases[(int)caseRarity]++;
                    break;
                case ComponentType.RAM:
                    StatisticsScript.RAMsDroppedByCases[(int)caseRarity]++;
                    break;
                case ComponentType.Motherboard:
                    StatisticsScript.motherboardsDroppedByCases[(int)caseRarity]++;
                    break;
                case ComponentType.All:
                    StatisticsScript.generalDroppedByCases[(int)caseRarity]++;
                    break;
            }

            //Decrease number of money.
            if (caseRarity == CaseType.Major)
                moneySystem.Money -= 50;
            else if (caseRarity == CaseType.VeryGood)
                moneySystem.Money -= 750;
            else if (caseRarity == CaseType.Top)
                moneySystem.Money -= 1500;
            //Spawn 50 cells
            for (int i = 0; i < 50; i++)
            {
                SpawnCell(caseType, i);
            }
            float screenSpeedMultiplier = ((float)Screen.width / Screen.height) / (16F / 9F);
            if (fastMode)
                //0 speed.
                speed = 10F;
            else
                //Randomizing speed.
                speed = Random.Range(90F * screenSpeedMultiplier, 109.3F * screenSpeedMultiplier);
            navigation.MenuItemClicked(8);
            //Start coroutine of case scrolling.
            StartCoroutine(CaseScroll());
        }
    }
    /// <summary>
    /// Coroutine of case scrolling.
    /// </summary>
    private IEnumerator CaseScroll()
    {
        navigation.blocked = true;
        cellsGroup.localPosition = new Vector2(0, 0);
        while (speed > 0)
        {
            //Move cells with speed.
            cellsGroup.Translate(speed * Time.deltaTime * -50, 0, 0);
            //Speed decreasing.
            speed -= Time.deltaTime * 20;
            //Waiting 1 frame.
            yield return null;
        }
        CaseStop();
        navigation.blocked = false;
    }
    /// <summary>
    /// Case stop event.
    /// </summary>
    private void CaseStop()
    {
        //Get dropped component.
        currentComponent = cursor.currentComponent;

        //Stats.
        StatisticsScript.casesOpened++;
        if (currentComponent is CPU)
            StatisticsScript.CPUsDropped++;
        else if (currentComponent is GPU)
            StatisticsScript.GPUsDropped++;
        else if (currentComponent is RAM)
            StatisticsScript.RAMsDropped++;
        else if (currentComponent is Motherboard)
            StatisticsScript.motherboardsDropped++;
        StatisticsScript.droppedByRarities[(int)currentComponent.rarity]++;

            //Set time of getting of component.
            currentComponent.time = System.DateTime.Now;
        //Set sprite of drop image.
        dropImage.sprite = currentComponent.image;
        //Set text of drop.
        dropProperties.text = currentComponent.ShortProperties;
        //Set sell text.
        if (currentComponent.price / 20 > 0)
            sellText.text = $"{LangManager.GetString("sell")} ({currentComponent.price / 20}$)";
        else
            sellText.text = LangManager.GetString("remove");
        //Add component to inventory.
        inventory.components.Add(currentComponent);
        //Invoke OnDrop function.
        drop.OnDrop();
        //Set active true of drop component.
        dropAnim.gameObject.SetActive(true);
        //Play animation.
        dropAnim.Play("OpenDroppedComponent");
    }
}