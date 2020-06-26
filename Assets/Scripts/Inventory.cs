using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    /// <summary>
    /// The list of components in the inventory.
    /// </summary>
    public List<PCComponent> components = new List<PCComponent>();

    /// <summary>
    /// The prefab of inventory cell.
    /// </summary>
    public GameObject cellPrefab;
    /// <summary>
    /// The group and parent of inventory cells.
    /// </summary>
    public RectTransform cellsGroup;
    /// <summary>
    /// Grid layout group of cells.
    /// </summary>
    public GridLayoutGroup cellsGrid;
    /// <summary>
    /// Text of main statistics of inventory on bottom panel.
    /// </summary>
    public Text inventoryInfoText;

    /// <summary>
    /// Case scroller script.
    /// </summary>
    public CaseScroller scroller;
    /// <summary>
    /// Money system script.
    /// </summary>
    public MoneySystem moneySystem;

    /// <summary>
    /// Game object of info window.
    /// </summary>
    public GameObject infoWindowObj;
    /// <summary>
    /// Main text of info window.
    /// </summary>
    public Text infoText;
    /// <summary>
    /// Text of sell button in info window.
    /// </summary>
    public Text sellInfoText;
    /// <summary>
    /// Button of info window.
    /// </summary>
    public Button sellInfoButton, removeInfoButton;
    /// <summary>
    /// Button animation of button of info window.
    /// </summary>
    public ButtonAnimation sellInfoButtonAnim, removeInfoButtonAnim;
    /// <summary>
    /// Script of info window.
    /// </summary>
    public InventoryInfoWindow infoWindow;
    /// <summary>
    /// Text of sorting. It is on bottom panel.
    /// </summary>
    public Text sortText;
    public SoundManager soundManager;

    private int page;
    private int selectedCell = -1;
    private Sort sortType;
    readonly private List<InventoryCell> cells = new List<InventoryCell>();

    /// <summary>
    /// Update inventory cells.
    /// </summary>
    public void UpdateInventory()
    {
        switch (sortType)
        {
            case Sort.Price:
                components.Sort(new SortByPrice());
                break;
            case Sort.Time:
                components.Sort(new SortByTime());
                break;
            case Sort.Rarity:
                components.Sort(new SortByRarity());
                break;
            case Sort.Type:
                components.Sort(new SortByType());
                break;
        }

        CalculateProperties(out int cellsInRow, out int cellsInColumn, out int cellsMax, out int cellsInPage);
        //Instantiate cells if they are not enough.
        while (cells.Count < cellsInPage)
            cells.Add(Instantiate(cellPrefab, cellsGroup).GetComponent<InventoryCell>());
        //Set active true to all needed cells in page. And false to unneeded.
        for (int i = 0; i < cells.Count; i++)
        {
            if (i < cellsInPage)
                cells[i].gameObject.SetActive(true);
            else
                cells[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < cellsInPage; i++)
        {
            cells[i].component = components[cellsMax * page + i];
            cells[i].image.sprite = components[cellsMax * page + i].image;
            cells[i].rarityLine.color = scroller.rarityColors[(int)components[cellsMax * page + i].rarity];
            int eventIndex = i;
            cells[i].GetComponent<Button>().onClick.AddListener(delegate { CellClicked(eventIndex); });
        }

        long totalPrice = 0;
        for (int i = 0; i < components.Count; i++)
        {
            totalPrice += components[i].price;
        }
        int totalPages = components.Count / cellsMax;
        if (components.Count > cellsMax * totalPages)
            totalPages++;
        if (totalPages == 0)
            totalPages++;
        inventoryInfoText.text = $"{LangManager.GetString("page")} {page + 1}/{totalPages}, {LangManager.GetString("items:")} {components.Count}, {LangManager.GetString("total_price:")} {totalPrice}$";
    }
    /// <summary>
    /// Event of next page button.
    /// </summary>
    public void PageNext()
    {
        CalculateProperties(out int _, out int _, out int cellsMax, out int _);

        if (cellsMax * (page + 1) < components.Count)
        {
            page++;
            UpdateInventory();
        }
    }
    /// <summary>
    /// Event of previous button.
    /// </summary>
    public void PagePrevious()
    {
        if (page > 0)
            page--;
        UpdateInventory();
    }
    /// <summary>
    /// Event of clicked sell.
    /// </summary>
    /// <param name="index">Index of cell.</param>
    private void CellClicked(int index)
    {
        selectedCell = index;

        CalculateProperties(out int _, out int _, out int _, out int cellsInPage);
        //Properties text.
        infoText.text = cells[index].component.FullProperties + $"\n{LangManager.GetString("time:")} " + components[cellsInPage * page + index].time.ToString("dd.MM.yyyy hh:mm:ss");
        
        if (components[cellsInPage * page + index].price / 20 > 0)
            sellInfoText.text = $"{LangManager.GetString("sell")} (+" + components[cellsInPage * page + index].price / 20 + "$)";
        else
            sellInfoText.text = LangManager.GetString("remove");
        //Opening window.
        StartCoroutine(infoWindow.WindowInAnimation());
        //Enable "Sell", "Remove" and "Equip" buttons.
        sellInfoButton.interactable = true;
        removeInfoButton.interactable = true;

        sellInfoButtonAnim.disabled = false;
        removeInfoButtonAnim.disabled = false;

        infoWindowObj.SetActive(true);
    }

    public void Sell()
    {
        //Stats.
        StatisticsScript.componentsSold++;
        StatisticsScript.moneyEarnedBySale += (uint)(cells[selectedCell].component.price / 20);

        //Play random sound.
        if (cells[selectedCell].component.price / 20 > 0)
            soundManager.PlayRandomSell();
        //Add money.
        moneySystem.Money += cells[selectedCell].component.price / 20;
        //Remove component.
        components.Remove(cells[selectedCell].component);

        //Disable "Sell", "Remove" and "Equip" buttons.
        sellInfoButton.interactable = false;
        removeInfoButton.interactable = false;
        
        sellInfoButtonAnim.disabled = true;
        removeInfoButtonAnim.disabled = true;

        //Update inventory.
        UpdateInventory();
        //Close window.
        StartCoroutine(infoWindow.WindowOutAnimation());
    }
    public void Remove()
    {
        //Remove component.
        components.Remove(cells[selectedCell].component);

        //Disable "Sell", "Remove" and "Equip" buttons.
        sellInfoButton.interactable = false;
        removeInfoButton.interactable = false;

        sellInfoButtonAnim.disabled = true;
        removeInfoButtonAnim.disabled = true;

        //Update inventory.
        UpdateInventory();
        //Close window.
        StartCoroutine(infoWindow.WindowOutAnimation());
    }

    public void SortingChange()
    {
        if ((int)sortType < 3)
            sortType++;
        else
            sortType = 0;
        UpdateInventory();
        switch (sortType)
        {
            case Sort.Price:
                sortText.text = $"{LangManager.GetString("sort_by")} {LangManager.GetString("by_price")}";
                break;
            case Sort.Rarity:
                sortText.text = $"{LangManager.GetString("sort_by")} {LangManager.GetString("rarity")}";
                break;
            case Sort.Time:
                sortText.text = $"{LangManager.GetString("sort_by")} {LangManager.GetString("time")}";
                break;
            case Sort.Type:
                sortText.text = $"{LangManager.GetString("sort_by")} {LangManager.GetString("type")}";
                break;
        }
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
        cellsInPage = components.Count - cellsMax * page;
        if (cellsInPage > cellsMax)
            cellsInPage = cellsMax;
    }
}

public enum Sort
{
    Time, Price, Rarity, Type
}

class SortByTime : IComparer<PCComponent>
{
    public int Compare(PCComponent p1, PCComponent p2)
    {
        if (p1.time > p2.time)
            return -1;
        else if (p1.time < p2.time)
            return 1;
        else
            return 0;
    }
}
class SortByPrice : IComparer<PCComponent>
{
    public int Compare(PCComponent p1, PCComponent p2)
    {
        if (p1.price > p2.price)
            return -1;
        else if (p1.price < p2.price)
            return 1;
        else
            return 0;
    }
}
class SortByRarity : IComparer<PCComponent>
{
    public int Compare(PCComponent p1, PCComponent p2)
    {
        if (p1.rarity > p2.rarity)
            return -1;
        else if (p1.rarity < p2.rarity)
            return 1;
        else
        {
            ComponentType type1, type2;

            if (p1 is CPU)
                type1 = ComponentType.CPU;
            else if (p1 is GPU)
                type1 = ComponentType.GPU;
            else if (p1 is RAM)
                type1 = ComponentType.RAM;
            else
                type1 = ComponentType.Motherboard;

            if (p2 is CPU)
                type2 = ComponentType.CPU;
            else if (p2 is GPU)
                type2 = ComponentType.GPU;
            else if (p2 is RAM)
                type2 = ComponentType.RAM;
            else
                type2 = ComponentType.Motherboard;

            if (type1 > type2)
                return 1;
            else if (type1 < type2)
                return -1;
            else
            {
                if (p1.price > p2.price)
                    return -1;
                else if (p1.price > p2.price)
                    return 1;
                else
                    return 0;
            }
        }
    }
}
class SortByType : IComparer<PCComponent>
{
    public int Compare(PCComponent p1, PCComponent p2)
    {
        ComponentType type1, type2;

        if (p1 is CPU)
            type1 = ComponentType.CPU;
        else if (p1 is GPU)
            type1 = ComponentType.GPU;
        else if (p1 is RAM)
            type1 = ComponentType.RAM;
        else
            type1 = ComponentType.Motherboard;

        if (p2 is CPU)
            type2 = ComponentType.CPU;
        else if (p2 is GPU)
            type2 = ComponentType.GPU;
        else if (p2 is RAM)
            type2 = ComponentType.RAM;
        else
            type2 = ComponentType.Motherboard;

        if (type1 > type2)
            return 1;
        if (type1 < type2)
            return -1;
        else
        {
            if (p1.rarity > p2.rarity)
                return -1;
            else if (p1.rarity < p2.rarity)
                return 1;
            else
            {
                if (p1.price > p2.price)
                    return -1;
                else if (p1.price < p2.price)
                    return 1;
                else
                    return 0;
            }
        }
    }
}