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
    public static decimal earned = 0;

    /// <summary>
    /// Performance of computer in BTC/day.
    /// </summary>
    public static decimal Performance
    {
        get
        {
            try
            {
                decimal performance = 0;

                //Add performance from GPUs.
                for (int i = 0; i < ComputerScript.GPUs.Count; i++)
                    if (ComputerScript.GPUs[i] != null)
                        performance += 0.00005m * ComputerScript.GPUs[i].power;

                //Add performance from CPU.
                performance += ComputerScript.mainCPU.power * 0.0002m;

                //Find minimum RAM frequency.
                int minRAMFreq = int.MaxValue;
                for (int i = 0; i < ComputerScript.RAMs.Count; i++)
                    if (ComputerScript.RAMs[i] != null && ComputerScript.RAMs[i].frequency < minRAMFreq)
                        minRAMFreq = ComputerScript.RAMs[i].frequency;

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
    public static decimal Capacity
    {
        get
        {
            //Total volume of RAM in computer.
            int totalVolume = 0;
            for (int i = 0; i < ComputerScript.RAMs.Count; i++)
                if (ComputerScript.RAMs[i] != null)
                    totalVolume += ComputerScript.RAMs[i].memory;

            return totalVolume * 0.000005m;
        }
    }

    private void Start()
    {
        //Set saves in start instead of awake because mining need begin with mining coroutine and saves of computer in awake.
        ApplySaves();
        StartCoroutine(MiningCoroutine());
    }

    public static void ApplySaves()
    {
        earned = GameSaver.savesPack.mined;
        if (GameSaver.savesPack.lastSession != DateTime.MinValue && Performance != -1)
            earned += (decimal)(DateTime.Now - GameSaver.savesPack.lastSession).TotalDays * Performance;
        if (earned > Capacity)
            earned = Capacity;
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
        CPUModel.text = ComputerScript.mainCPU.fullName;
        CPUFrequency.text = $"{ComputerScript.mainCPU.frequency} MHz";
        CPUBusWidth.text = ComputerScript.mainCPU._64bit ? "64 bit" : "32 bit";

        int minFreq = int.MaxValue;

        for (int i = 0; i < ComputerScript.RAMs.Count; i++)
            if (ComputerScript.RAMs[i] != null && ComputerScript.RAMs[i].frequency < minFreq)
                minFreq = ComputerScript.RAMs[i].frequency;

        RAMFrequency.text = $"{minFreq} MHz";
        RAMGeneration.text = "DDR" + (ComputerScript.mainMotherboard.RAMType == 1 ? null : ComputerScript.mainMotherboard.RAMType.ToString());

        int ramVol = 0;
        for (int i = 0; i < ComputerScript.RAMs.Count; i++)
            if (ComputerScript.RAMs[i] != null)
                ramVol += ComputerScript.RAMs[i].memory;

        RAMVolume.text = $"{ramVol} MB";

        socket.text = ComputerScript.mainMotherboard.socket;
        chipset.text = ComputerScript.mainMotherboard.chipset.ToString().RemoveChar('_');

        integratedGraphics.sprite = ComputerScript.mainCPU.integratedGraphics ? checkChecked : checkUnchecked;
        multiplierUnlocked.sprite = ComputerScript.mainCPU.unlocked ? checkChecked : checkUnchecked;
        SLI.sprite = ComputerScript.mainMotherboard.SLI ? checkChecked : checkUnchecked;
        crossfire.sprite = ComputerScript.mainMotherboard.crossfire ? checkChecked : checkUnchecked;

        //GPU buttons.
        //Destroy old buttons.
        for (int i = 0; i < GPUButtons.Count; i++)
            Destroy(GPUButtons[i]);
        GPUButtons.Clear();

        //Instantiate buttons.
        for (int i = 0; i < ComputerScript.mainMotherboard.busVersions.Length; i++)
        {
            GPUButtons.Add(Instantiate(gpuButtonPrefab, gpuParent));
            //Change text of label.
            GPUButtons[i].transform.Find("Label").GetComponent<Text>().text = $"GPU {i}:";
            //Change button text.
            GPUButtons[i].transform.Find("Button").GetComponentInChildren<Text>().text = LangManager.GetString("properties...");
            //If GPU with index i != null, button.interactable = true.
            if (ComputerScript.GPUs[i] != null)
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
        GPUModel.text = ComputerScript.GPUs[index].fullName;
        GPUBusInterface.text = $"PCIe {ComputerScript.GPUs[index].busVersion}.0 x{ComputerScript.GPUs[index].busMultiplier}";
        GPUMemoryVolume.text = $"{ComputerScript.GPUs[index].memory} MB";
    }

    public void UpdateRecommendations()
    {
        //Add recomendations.
        string recommendations = null;

        //Check for enabled multiple channel mode of RAM.
        if (CheckMultipleChannels() && ComputerScript.RAMs.Count(x => x != null) >= ComputerScript.mainMotherboard.RAMCount / 2)
            recommendations += string.Format(LangManager.GetString("ram_channel_recom"), ComputerScript.mainMotherboard.RAMCount / 2);

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
        for (int i = 0; i < ComputerScript.RAMs.Count; i++)
            if (ComputerScript.RAMs[i] != null)
                totalRAM += ComputerScript.RAMs[i].memory;
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
        MoneySystem.BTCMoney += earned;
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
        bool? usedDouble = ComputerScript.RAMs[0] != null, usedNotDouble = ComputerScript.RAMs[1] != null;
        for (int i = 2; i < ComputerScript.mainMotherboard.RAMCount; i += 2)
        {
            if (usedDouble != (ComputerScript.RAMs[i] != null))
                usedDouble = null;
        }
        for (int i = 3; i < ComputerScript.mainMotherboard.RAMCount; i += 2)
        {
            if (usedNotDouble != (ComputerScript.RAMs[i] != null))
                usedNotDouble = null;
        }
        Debug.Log(usedDouble == null && usedNotDouble == null);
        return usedDouble == null && usedNotDouble == null;
    }
}
