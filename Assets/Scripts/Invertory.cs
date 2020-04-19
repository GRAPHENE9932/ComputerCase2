using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Invertory : MonoBehaviour
{
    public List<PCComponent> components = new List<PCComponent>();
    public GameObject cellPrefab;
    public RectTransform cellsGroup;
    public GridLayoutGroup cellsGrid;
    public CaseScroller scroller;

    public GameObject infoWindows;
    public Text infoText;

    private int page;
    readonly private List<InvertoryCell> cells = new List<InvertoryCell>();

    /// <summary>
    /// Update invertory cells.
    /// </summary>
    public void UpdateInvertory()
    {
        //Cells in one row.
        int cellsInRow = ((int)cellsGroup.rect.width + (int)cellsGrid.spacing.x) / ((int)cellsGrid.cellSize.x + (int)cellsGrid.spacing.x);
        //Cells in one column.
        int cellsInColumn = ((int)cellsGroup.rect.height + (int)cellsGrid.spacing.y) / ((int)cellsGrid.cellSize.y + (int)cellsGrid.spacing.y);
        //Max cells in one page.
        int cellsMax = cellsInRow * cellsInColumn;
        //Sells in page
        int cellsInPage = components.Count - cellsMax * page;
        if (cellsInPage > cellsMax)
            cellsInPage = cellsMax;
        //Instantiate cells if they are not enough.
        while (cells.Count < cellsInPage)
        {
            cells.Add(Instantiate(cellPrefab, cellsGroup).GetComponent<InvertoryCell>());
        }
        //Set active true to all needed cells in page. And false to unneeded.
        for (int i = 0; i < cellsInPage; i++)
        {
            if (i < cellsInPage)
                cells[i].gameObject.SetActive(true);
            else
                cells[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < cellsInPage; i++)
        {
            cells[i].image.sprite = components[cellsInPage * page + i].image;
            cells[i].rarityLine.color = scroller.rarityColors[(int)components[cellsMax * page + i].rarity];
            int eventIndex = i;
            cells[i].GetComponent<Button>().onClick.AddListener(delegate { CellClicked(eventIndex); });
        }
    }

    public void PageNext()
    {
        //Cells in one row.
        int cellsInRow = ((int)cellsGroup.localPosition.x + (int)cellsGrid.spacing.x) / ((int)cellsGroup.sizeDelta.x + (int)cellsGrid.spacing.x);
        //Cells in one column.
        int cellsInColumn = ((int)cellsGroup.localPosition.y + (int)cellsGrid.spacing.y) / ((int)cellsGroup.sizeDelta.y + (int)cellsGrid.spacing.y);
        //Max cells in one page.
        int cellsMax = cellsInRow * cellsInColumn;

        if (cellsMax * (page + 1) > components.Count)
        {
            page++;
            UpdateInvertory();
        }
    }

    public void PagePrevious()
    {
        if (page > 0)
            page--;
    }

    private void CellClicked(int index)
    {
        Debug.Log("Cell" + index);
    }
}
