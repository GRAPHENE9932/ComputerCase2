using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour
{
    /// <summary>
    /// Prefabs of shop sells.
    /// </summary>
    public GameObject cellPrefab;
    /// <summary>
    /// Parent of shop sells.
    /// </summary>
    public RectTransform cellsGroup;
    /// <summary>
    /// Grid layout group of parent of shop sells.
    /// </summary>
    public GridLayoutGroup cellsGrid;
    public CaseScroller caseScroller;
    /// <summary>
    /// The main text of info window.
    /// </summary>
    public Text infoText;
    /// <summary>
    /// Text of buy button.
    /// </summary>
    public Text buyText;
    /// <summary>
    /// Canvas group of info window.
    /// </summary>
    public CanvasGroup infoGroup;
    /// <summary>
    /// The object of info window.
    /// </summary>
    public GameObject infoObject;
    public InputField searchField;
    public MoneySystem moneySystem;
    public Inventory inventory;
    public MessageBoxManager messageBox;


    private int page;
    private ComponentType shopType;
    private readonly List<GameObject> cells = new List<GameObject>();
    private List<CPU> CPUs = new List<CPU>();
    private List<GPU> GPUs = new List<GPU>();
    private List<RAM> RAMs = new List<RAM>();
    private List<Motherboard> motherboards = new List<Motherboard>();
    private List<PCComponent> currentList;
    /// <summary>
    /// The component cell clicked.
    /// </summary>
    private PCComponent selectedComponent;

    private void Awake()
    {
        //Load and sort components.
        CPUs = Resources.LoadAll<PCComponent>("CPU").Select(x => (CPU)x).ToList();
        CPUs.Sort(new SortByRarity());
        GPUs = Resources.LoadAll<PCComponent>("GPU").Select(x => (GPU)x).ToList();
        GPUs.Sort(new SortByRarity());
        RAMs = Resources.LoadAll<PCComponent>("RAM").Select(x => (RAM)x).ToList();
        RAMs.Sort(new SortByRarity());
        motherboards = Resources.LoadAll<PCComponent>("Motherboard").Select(x => (Motherboard)x).ToList();
        motherboards.Sort(new SortByRarity());
    }

    public void SetShopType(int type)
    {
        //Set shop type.
        shopType = (ComponentType)type;
        //Set currentList.
        switch (shopType)
        {
            case ComponentType.CPU:
                currentList = CPUs.Select(x => (PCComponent)x).ToList();
                break;
            case ComponentType.GPU:
                currentList = GPUs.Select(x => (PCComponent)x).ToList();
                break;
            case ComponentType.RAM:
                currentList = RAMs.Select(x => (PCComponent)x).ToList();
                break;
            case ComponentType.Motherboard:
                currentList = motherboards.Select(x => (PCComponent)x).ToList();
                break;
        }
    }

    public void UpdateShop()
    {
        CalculateProperties(out int cellsInRow, out int cellsInColumn, out int cellsMax, out int cellsInPage);
        while (cells.Count < cellsInPage)
            cells.Add(Instantiate(cellPrefab, cellsGroup));

        for (int i = 0; i < cells.Count; i++)
        {
            if (i < cellsInPage)
                cells[i].SetActive(true);
            else
                cells[i].SetActive(false);
        }
        for (int i = 0; i < cellsInPage; i++)
        {
            cells[i].transform.Find("Image").GetComponent<Image>().sprite = currentList[cellsMax * page + i].image;
            cells[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)currentList[cellsMax * page + i].rarity];
            cells[i].transform.Find("PriceText").GetComponent<Text>().text = (currentList[cellsMax * page + i].price).ToString() + "$";
            int eventIndex = i;
            cells[i].GetComponent<Button>().onClick.AddListener(delegate { CellClicked(eventIndex); });
        }
    }

    /// <summary>
    /// Event of next page button.
    /// </summary>
    public void PageNext()
    {
        CalculateProperties(out int _, out int _, out int cellsMax, out int _);

        int count = 0;
        switch (shopType)
        {
            case ComponentType.CPU:
                count = CPUs.Count;
                break;
            case ComponentType.GPU:
                count = GPUs.Count;
                break;
            case ComponentType.RAM:
                count = RAMs.Count;
                break;
            case ComponentType.Motherboard:
                count = motherboards.Count;
                break;
        }

        if (cellsMax * (page + 1) < count)
        {
            page++;
            UpdateShop();
        }
    }
    /// <summary>
    /// Event of previous button.
    /// </summary>
    public void PagePrevious()
    {
        if (page > 0)
            page--;
        UpdateShop();
    }

    /// <summary>
    /// Event of cell with component clicked
    /// </summary>
    /// <param name="index">Index of cell.</param>
    private void CellClicked(int index)
    {
        selectedComponent = currentList[index];

        CalculateProperties(out int _, out int _, out int _, out int cellsInPage);

        //Properties text.
        infoText.text = currentList[cellsInPage * page + index].FullProperties;
        //Set text of buy button.
        buyText.text = $"Buy (-{currentList[cellsInPage * page + index].price}$)";

        StartCoroutine(InfoAnimation(true));
    }

    /// <summary>
    /// Event of back button in the info window.
    /// </summary>
    public void Back()
    {
        StartCoroutine(InfoAnimation(false));
    }

    /// <summary>
    /// Event of buy button in the info window.
    /// </summary>
    public void Buy()
    {
        //If not enought money, start message about it and stop function.
        if (selectedComponent.price > moneySystem.Money.Value)
        {
            messageBox.StartMessage("Not enought money!", 1);
            return;
        }

        PCComponent compToBuy = selectedComponent;
        compToBuy.time = DateTime.Now;
        moneySystem.Money -= compToBuy.price;
        inventory.components.Add(compToBuy);
    }

    /// <summary>
    /// Animation of info window.
    /// </summary>
    /// <param name="open">True - info show, false - info hide.</param>
    private IEnumerator InfoAnimation(bool open)
    {
        //If open, enable info window.
        if (open)
            infoObject.SetActive(true);
        //Duration 0.25 s.
        float time = 0;
        while (time < 0.25F)
        {
            time += Time.deltaTime;
            if (open)
                infoGroup.alpha = time * 4;
            else
                infoGroup.alpha = (0.25F - time) * 4;
            yield return null;
        }
        //If close, disable info window.
        if (!open)
            infoObject.SetActive(false);
    }

    /// <summary>
    /// Event of changed text in search input field.
    /// </summary>
    public void SearchChanged()
    {
        currentList.Clear();
        PCComponent[] listOriginal = null;
        switch (shopType)
        {
            case ComponentType.CPU:
                listOriginal = CPUs.Select(x => (PCComponent)x).ToArray();
                break;
            case ComponentType.GPU:
                listOriginal = GPUs.Select(x => (PCComponent)x).ToArray();
                break;
            case ComponentType.RAM:
                listOriginal = RAMs.Select(x => (PCComponent)x).ToArray();
                break;
            case ComponentType.Motherboard:
                listOriginal = motherboards.Select(x => (PCComponent)x).ToArray();
                break;
        }
        //Filter the currentList with text in search field.
        for (int i = 0; i < listOriginal.Length; i++)
        {
            if (listOriginal[i].fullName.ToLower().Contains(searchField.text.ToLower()))
                currentList.Add(listOriginal[i]);
        }
        //Then, update shop.
        UpdateShop();
    }

    private void CalculateProperties(out int cellsInRow, out int cellsInColumn, out int cellsMax, out int cellsInPage)
    {
        //Cells in one row.
        cellsInRow = ((int)cellsGroup.rect.width + (int)cellsGrid.spacing.x) / ((int)cellsGrid.cellSize.x + (int)cellsGrid.spacing.x);
        //Cells in one column.
        cellsInColumn = ((int)cellsGroup.rect.height + (int)cellsGrid.spacing.y) / ((int)cellsGrid.cellSize.y + (int)cellsGrid.spacing.y);
        //Max cells in one page.
        cellsMax = cellsInRow * cellsInColumn;

        //Sells in page
        cellsInPage = currentList.Count - cellsMax * page;
        if (cellsInPage > cellsMax)
            cellsInPage = cellsMax;
    }
}
