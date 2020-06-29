using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum OSPage
{
    SystemConfiguration, Recommendations, Mining, None
}

public class OSScript : MonoBehaviour
{
    OSPage currentPage;
    /// <summary>
    /// Parents of page objects.
    /// </summary>
    public GameObject[] pages;
    /// <summary>
    /// Text in field of configuration page.
    /// </summary>
    public Text CPUModel, CPUFrequency, CPUBusWidth, RAMFrequency, RAMGeneration, RAMVolume, socket, chipset;
    /// <summary>
    /// Checks of configuration page.
    /// </summary>
    public Image integratedGraphics, multiplierUnlocked, SLI, crossfire;
    /// <summary>
    /// The buttons of GPUs configuration.
    /// </summary>
    private readonly List<GameObject> GPUButtons = new List<GameObject>();

    /// <summary>
    /// GPU configuration window.
    /// </summary>
    public GameObject GPUWindow;
    /// <summary>
    /// Text in field of GPU configuration window.
    /// </summary>
    public Text GPUModel, GPUBusInterface, GPUMemoryVolume, GPUHeader;

    public Sprite checkChecked, checkUnchecked;
    public ComputerScript comp;
    public MoneySystem moneySystem;
    public List<EventTrigger.Entry> triggersForButton;
    /// <summary>
    /// Prefab of GPU configuration label and button.
    /// </summary>
    public GameObject gpuButtonPrefab;
    /// <summary>
    /// Parent of GPU buttons array.
    /// </summary>
    public Transform gpuParent;

    /// <summary>
    /// Main text component in recommendations page.
    /// </summary>
    public Text recommendationsText;

    /// <summary>
    /// Text in field of mining page.
    /// </summary>
    public Text earnedText, capacityText, performanceText0, performanceText1, performanceText2, memoryUsedText;
    /// <summary>
    /// Text in progressbar of mining page.
    /// </summary>
    public Text miningProgressbarText0, miningProgressbarText1;
    /// <summary>
    /// Mining progressbar bar. Used FillAmount.
    /// </summary>
    public Image miningProgressbar;

    /// <summary>
    /// Mined money in Bitcoins
    /// </summary>
    private decimal earned = 0;

    /// <summary>
    /// Performance of computer in BTC/day.
    /// </summary>
    private decimal Performance
    {
        get
        {
            try
            {
                decimal performance = 0;

                //Add performance from GPUs.
                for (int i = 0; i < comp.GPUs.Count; i++)
                    if (comp.GPUs[i] != null)
                        performance += 0.00005m * comp.GPUs[i].power;

                //Add performance from CPU.
                performance += comp.mainCPU.power * 0.0002m;

                //Find minimum RAM frequency.
                int minRAMFreq = int.MaxValue;
                for (int i = 0; i < comp.RAMs.Count; i++)
                    if (comp.RAMs[i] != null && comp.RAMs[i].frequency < minRAMFreq)
                        minRAMFreq = comp.RAMs[i].frequency;

                //Add performance from RAM.
                performance += minRAMFreq * 0.0000125m;

                return performance;
            }
            catch
            {
                return -1;
            }
        }
    }

    /// <summary>
    /// Capacity of RAM in Bitcoins.
    /// </summary>
    private decimal Capacity
    {
        get
        {
            //Total volume of RAM in computer.
            int totalVolume = 0;
            for (int i = 0; i < comp.RAMs.Count; i++)
                if (comp.RAMs[i] != null)
                    totalVolume += comp.RAMs[i].memory;

            return totalVolume * 0.000005m;
        }
    }
    private void Start()
    {
        StartCoroutine(MiningCoroutine());
    }
    /// <summary>
    /// Start OS, when monitor clicked.
    /// </summary>
    public void StartOS()
    {
        //Disable all pages.
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(false);

        currentPage = OSPage.None;
    }
    /// <summary>
    /// Change page.
    /// </summary>
    /// <param name="page">Index of page in OSPage enum.</param>
    public void Navigate(int page)
    {
        //Disable previous page.
        if (currentPage != OSPage.None)
            pages[(int)currentPage].SetActive(false);
        //Change page variable.
        currentPage = (OSPage)page;
        //Enable new page.
        pages[page].SetActive(true);
        //If current page == system configuration, update system configuration.
        if (currentPage == OSPage.SystemConfiguration)
            UpdateSystemConfiguration();
        //If current page == recommendation, update recommendations.
        else if (currentPage == OSPage.Recommendations)
            UpdateRecommendations();
    }

    /// <summary>
    /// Close button clicked.
    /// </summary>
    public void CloseClicked()
    {
        //Disable current page.
        pages[(int)currentPage].SetActive(false);
    }

    /// <summary>
    /// Update configuration.
    /// </summary>
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
        chipset.text = comp.mainMotherboard.chipset.ToString().RemoveChar('_');

        integratedGraphics.sprite = comp.mainCPU.integratedGraphics ? checkChecked : checkUnchecked;
        multiplierUnlocked.sprite = comp.mainCPU.unlocked ? checkChecked : checkUnchecked;
        SLI.sprite = comp.mainMotherboard.SLI ? checkChecked : checkUnchecked;
        crossfire.sprite = comp.mainMotherboard.crossfire ? checkChecked : checkUnchecked;

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
                GPUButtons[i].GetComponentInChildren<EventTrigger>().triggers = triggersForButton;
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
    /// <summary>
    /// Clicked button of GPU properties.
    /// </summary>
    /// <param name="index">Index of GPU.</param>
    private void GPUPropertiesButton(int index)
    {
        //Enable GPU window.
        GPUWindow.SetActive(true);
        //Set text of properties in this window.
        GPUHeader.text = string.Format(LangManager.GetString("gpu_properties"), index);
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
            recommendations += string.Format(LangManager.GetString("ram_channel_recom"), comp.mainMotherboard.RAMCount / 2);

        recommendationsText.text = recommendations;
    }

    public void UpdateMining()
    {
        earnedText.text = $"{Math.Round(earned, 15)} BTC";
        capacityText.text = $"{Math.Round(Capacity, 15)} BTC";
        performanceText0.text = $"{Math.Round(Performance / 1440, 15)} BTC/{LangManager.GetString("min")}";
        performanceText1.text = $"{Math.Round(Performance / 24, 15)} BTC/{LangManager.GetString("hour")}";
        performanceText2.text = $"{Math.Round(Performance, 15)} BTC/{LangManager.GetString("day")}";
        int totalRAM = 0;
        for (int i = 0; i < comp.RAMs.Count; i++)
            if (comp.RAMs[i] != null)
                totalRAM += comp.RAMs[i].memory;
        memoryUsedText.text = $"{Math.Round(earned / 0.00001m, 2)}/{totalRAM} MB";

        float progress = (float)earned / 0.00001F / totalRAM;
        miningProgressbar.fillAmount = progress;
        miningProgressbarText0.text = $"{Math.Round(progress, 4) * 100}%";
        miningProgressbarText1.text = $"{Math.Round(progress, 4) * 100}%";
    }

    public IEnumerator MiningCoroutine()
    {
        while (true)
        {
            decimal perf = Performance;
            if (perf != -1)
            {
                //Add performance per second.
                earned += perf / 86400;
                if (earned > Capacity)
                    earned = Capacity;
                if (currentPage == OSPage.Mining)
                    UpdateMining();
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void CollectClicked()
    {
        moneySystem.BTCMoney += earned;
        earned = 0;
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
        Debug.Log(usedDouble == null && usedNotDouble == null);
        return usedDouble == null && usedNotDouble == null;
    }
}
