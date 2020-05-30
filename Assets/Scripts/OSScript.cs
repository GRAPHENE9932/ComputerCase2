using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public enum OSPage
{
    SystemConfiguration, Recommendations, Mining, None
}

public class OSScript : MonoBehaviour
{
    OSPage currentPage;
    public GameObject[] pages;
    public Text CPUModel, CPUFrequency, CPUBusWidth, RAMFrequency, RAMGeneration, RAMVolume, socket, chipset;
    public Image integratedGraphics, multiplierUnlocked;
    private readonly List<GameObject> GPUButtons = new List<GameObject>();

    public GameObject GPUWindow;
    public Text GPUModel, GPUBusInterface, GPUMemoryVolume;

    public Sprite checkChecked, checkUnchecked;
    public ComputerScript comp;
    public GameObject gpuButtonPrefab;
    public Transform gpuParent;

    public Text recommendationsText;

    public void StartOS()
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(false);
        currentPage = OSPage.None;
    }

    public void Navigate(int page)
    {
        pages[(int)currentPage].SetActive(false);
        currentPage = (OSPage)page;
        pages[page].SetActive(true);
        if (currentPage == OSPage.SystemConfiguration)
            UpdateSystemConfiguration();
    }

    public void CloseClicked()
    {
        pages[(int)currentPage].SetActive(false);
    }

    public void UpdateSystemConfiguration()
    {
        //Text of page of SystemConfiguration.
        CPUModel.text = comp.mainCPU.fullName;
        CPUFrequency.text = $"{comp.mainCPU.frequency} MHz";
        CPUBusWidth.text = comp.mainCPU._64bit ? "64 bit" : "32 bit";

        int minFreq = int.MaxValue;

        for (int i = 0; i < comp.RAMs.Count; i++)
            if (comp.RAMs[i] != null && comp.RAMs[i].frequency < minFreq)
                minFreq = comp.RAMs[i].frequency;

        RAMFrequency.text = $"{minFreq} MHz";
        RAMGeneration.text = "DDR" + (comp.mainMotherboard.RAMType == 1 ? null : comp.mainMotherboard.RAMType.ToString());

        int ramVol = 0;
        for (int i = 0; i < comp.RAMs.Count; i++)
            if (comp.RAMs[i] != null)
                ramVol += comp.RAMs[i].memory;

        RAMVolume.text = $"{ramVol} MB";

        socket.text = comp.mainMotherboard.socket;
        chipset.text = comp.mainMotherboard.chipset.ToString().Replace("_", "");

        integratedGraphics.sprite = comp.mainCPU.integratedGraphics ? checkChecked : checkUnchecked;
        multiplierUnlocked.sprite = comp.mainCPU.unlocked ? checkChecked : checkUnchecked;

        //GPU buttons.
        //Destroy old buttons.
        for (int i = 0; i < GPUButtons.Count; i++)
            Destroy(GPUButtons[i]);
        GPUButtons.Clear();

        //Instantiate buttons.
        for (int i = 0; i < comp.mainMotherboard.busVersions.Length; i++)
        {
            GPUButtons.Add(Instantiate(gpuButtonPrefab, gpuParent));
            //Change text of label.
            GPUButtons[i].transform.Find("Label").GetComponent<Text>().text = $"GPU {i}:";
            //If GPU with index i != null, button.interactable = true.
            if (comp.GPUs[i] != null)
            {
                GPUButtons[i].GetComponentInChildren<Button>().interactable = true;
                //Add event of clicked button.
                int index = i;
                GPUButtons[i].GetComponentInChildren<Button>().onClick.AddListener(delegate { GPUPropertiesButton(index); });
            }
            else
            {
                //Disabled image is image in button with color #FFFFFF80. If turn on this image, button and text of button getting gray.
                GPUButtons[i].transform.Find("Button").Find("Disabled image").gameObject.SetActive(true);
                //Disable button.
                GPUButtons[i].GetComponentInChildren<Button>().interactable = false;
            }
        }
    }

    private void GPUPropertiesButton(int index)
    {
        GPUWindow.SetActive(true);
        GPUModel.text = comp.GPUs[index].fullName;
        GPUBusInterface.text = $"PCIe {comp.GPUs[index].busVersion}.0 x{comp.GPUs[index].busMultiplier}";
        GPUMemoryVolume.text = $"{comp.GPUs[index].memory} MB";
    }

    public void UpdateRecommendations()
    {
        //Add recomendations.
        string recommendations = null;

        //Check for enabled multiple channel mode of RAM.
        if (CheckMultipleChannels() && comp.RAMs.Count(x => x != null) >= comp.mainMotherboard.RAMCount / 2)
            recommendations += $"You using single-channel mode of RAM, but can use {comp.mainMotherboard.RAMCount / 2}-channel mode, just replace your RAM planks with alternate. It contributes to improvement of RAM performance.\n";

        recommendationsText.text = recommendations;
    }
    /// <summary>
    /// Check the using of multiple RAM channels.
    /// </summary>
    /// <returns>
    /// True - using multiple channels, false - not using.
    /// </returns>
    private bool CheckMultipleChannels()
    {
        //usedDouble - is slots with indexes 0, 2, 4, 6, ... used? True - used, false - unused, null - different.
        //usedNotDouble - is slots with indexes 1, 3, 5, 7, ... used? True - used, false - unused, null - different.
        bool? usedDouble = comp.RAMs[0] != null, usedNotDouble = comp.RAMs[1] != null;
        for (int i = 2; i < comp.mainMotherboard.RAMCount; i += 2)
        {
            if (usedDouble != (comp.RAMs[i] != null))
                usedDouble = null;
        }
        for (int i = 3; i < comp.mainMotherboard.RAMCount; i += 2)
        {
            if (usedNotDouble != (comp.RAMs[i] != null))
                usedNotDouble = null;
        }
        return usedDouble == null && usedNotDouble == null;
    }
}
