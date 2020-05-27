using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OSPage
{
    SystemConfiguration, Recommendations, Mining
}

public class OSScript : MonoBehaviour
{
    OSPage currentPage;
    public GameObject[] pages;

    public void Navigate(int page)
    {
        pages[(int)currentPage].SetActive(false);
        currentPage = (OSPage)page;
        pages[page].SetActive(true);
    }

    public void CloseClicked()
    {
        pages[(int)currentPage].SetActive(true);
    }
}
