using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public List<PCComponent> components = new List<PCComponent>();
    public GameObject cellPrefab;
    public RectTransform cellsGroup;
    public GridLayoutGroup cellsGrid;
    public CaseScroller scroller;

    public GameObject infoWindowObj;
    public Text infoText;
    public InventoryInfoWindow infoWindow;

    private int page;
    readonly private List<InventoryCell> cells = new List<InventoryCell>();

    /// <summary>
    /// Update inventory cells.
    /// </summary>
    public void UpdateInventory()
    {
        CalculateProperties(out int cellsInRow, out int cellsInColumn, out int cellsMax, out int cellsInPage);
        //Instantiate cells if they are not enough.
        while (cells.Count < cellsInPage)
        {
            cells.Add(Instantiate(cellPrefab, cellsGroup).GetComponent<InventoryCell>());
        }
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
            cells[i].image.sprite = components[cellsMax * page + i].image;
            cells[i].rarityLine.color = scroller.rarityColors[(int)components[cellsMax * page + i].rarity];
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
        CalculateProperties(out int _, out int _, out int _, out int cellsInPage);
        //Properties text.
        infoText.text = components[cellsInPage * page + index].Properties + "\nTime: " + components[cellsInPage * page + index].time.ToString("dd.MM.yyyy hh.mm.ss");
        //Opening window.
        StartCoroutine(infoWindow.WindowInAnimation());
        infoWindowObj.SetActive(true);
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
