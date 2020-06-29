using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsScript : MonoBehaviour
{
    [HideInInspector]
    public static ulong casesOpened, itemsScrolled, CPUsDropped, GPUsDropped, RAMsDropped, motherboardsDropped, componentsSold, moneyEarnedBySale,
        moneyWonInCasino, moneyLostInCasino, gameLaunches;
    //[HideInInspector]
    public static ulong gameplayTime;
    [HideInInspector]
    public static ulong[] droppedByRarities;
    [HideInInspector]
    public static ulong[] CPUsDroppedByCases, GPUsDroppedByCases, RAMsDroppedByCases, motherboardsDroppedByCases, generalDroppedByCases;

    public Text casesOpenedText, itemsScrolledText, CPUsDroppedText, GPUsDroppedText, RAMsDroppedText, motherboardsDroppedText, componentsSoldText,
        moneyEarnedBySaleText, moneyWonInCasinoText, moneyLostInCasinoText, gameLaunchesText, gameplayTimeText;
    public Text[] itemsRarityText;
    public Image[] itemsRarityColumns, itemsRarityLegend;

    public Text casesOpenedText1;
    public Text[] CPUsDroppedByCasesText, GPUsDroppedByCasesText, RAMsDroppedByCasesText, motherboardsDroppedByCasesText, generalDroppedByCasesText;

    public CaseScroller caseScroller;

    private void Start()
    {
        //TODO: remove it when saves script will finished
        CPUsDroppedByCases = new ulong[4];
        GPUsDroppedByCases = new ulong[4];
        RAMsDroppedByCases = new ulong[4];
        motherboardsDroppedByCases = new ulong[4];
        generalDroppedByCases = new ulong[4];
        droppedByRarities = new ulong[5];
        //

        //Set graph colors.
        for (int i = 0; i < 5; i++)
        {
            itemsRarityColumns[i].color = caseScroller.rarityColors[i];
            itemsRarityLegend[i].color = caseScroller.rarityColors[i];
        }

        gameLaunches++;

        StartCoroutine(Timer());
        UpdateStatistics();
    }

    private IEnumerator Timer()
    {
        while (true)
        {
            gameplayTime++;
            UpdateGameplayTime();
            yield return new WaitForSeconds(1);
        }
    }

    /// <summary>
    /// Updates only gameplay time text.
    /// </summary>
    private void UpdateGameplayTime()
    {
        gameplayTimeText.text = $"{LangManager.GetString("gameplay_time:")}\n{FormatSeconds(gameplayTime)}";
    }

    /// <summary>
    /// Updates texts of statistics.
    /// </summary>
    public void UpdateStatistics()
    {
        casesOpenedText.text = $"{LangManager.GetString("cases_opened:")}\n{casesOpened}";
        itemsScrolledText.text = $"{LangManager.GetString("items_scrolled:")}\n{itemsScrolled}";

        //CPUs dropped:
        //228 (66,6%)
        float percent = (float)Math.Round((float)CPUsDropped / casesOpened * 100F, 1);
        string percentStr = float.IsNaN(percent) ? "N/A" : percent + "%";
        CPUsDroppedText.text = $"{LangManager.GetString("cpus_dropped:")}\n{CPUsDropped} ({percentStr})";

        //GPUs dropped:
        //228 (66,6%)
        percent = (float)Math.Round((float)GPUsDropped / casesOpened * 100F, 1);
        percentStr = float.IsNaN(percent) ? "N/A" : percent + "%";
        GPUsDroppedText.text = $"{LangManager.GetString("gpus_dropped:")}\n{GPUsDropped} ({percentStr})";

        //RAMs dropped:
        //228 (66,6%)
        percent = (float)Math.Round((float)RAMsDropped / casesOpened * 100F, 1);
        percentStr = float.IsNaN(percent) ? "N/A" : percent + "%";
        RAMsDroppedText.text = $"{LangManager.GetString("rams_dropped:")}\n{RAMsDropped} ({percentStr})";

        //Motherboards dropped:
        //228 (66,6%)
        percent = (float)Math.Round((float)motherboardsDropped / casesOpened * 100F, 1);
        percentStr = float.IsNaN(percent) ? "N/A" : percent + "%";
        motherboardsDroppedText.text = $"{LangManager.GetString("motherboards_dropped:")}\n{motherboardsDropped} ({percentStr})";

        componentsSoldText.text = $"{LangManager.GetString("components_sold:")}\n{componentsSold}";
        moneyEarnedBySaleText.text = $"{LangManager.GetString("money_earned_by_sale:")}\n{moneyEarnedBySale}$";
        moneyWonInCasinoText.text = $"{LangManager.GetString("money_won_in_casino:")}\n{moneyWonInCasino}$";
        moneyLostInCasinoText.text = $"{LangManager.GetString("money_lost_in_casino:")}\n{moneyLostInCasino}$";
        gameLaunchesText.text = $"{LangManager.GetString("game_launches:")}\n{gameLaunches}";
        gameplayTimeText.text = $"{LangManager.GetString("gameplay_time:")}\n{FormatSeconds(gameplayTime)}";

        //Second page.
        casesOpenedText1.text = $"{LangManager.GetString("cases_opened:")}\n{casesOpened}";

        for (int i = 0; i < 4; i++)
        {
            CPUsDroppedByCasesText[i].text = $"{LangManager.GetString("cpu_cases:")}\n{CPUsDroppedByCases[i]}";
            GPUsDroppedByCasesText[i].text = $"{LangManager.GetString("gpu_cases:")}\n{GPUsDroppedByCases[i]}";
            RAMsDroppedByCasesText[i].text = $"{LangManager.GetString("ram_cases:")}\n{RAMsDroppedByCases[i]}";
            motherboardsDroppedByCasesText[i].text = $"{LangManager.GetString("motherboard_cases:")}\n{motherboardsDroppedByCases[i]}";
            generalDroppedByCasesText[i].text = $"{LangManager.GetString("general_cases:")}\n{generalDroppedByCases[i]}";
        }

        //Graph.
        ulong maxDropped = droppedByRarities.Max();
        for (int i = 0; i < 5; i++)
        {
            float percent1 = (float)droppedByRarities[i] / casesOpened * 100F;
            float diagramPercent = (float)droppedByRarities[i] / maxDropped * 100F;
            string percentStr1;
            if (float.IsNaN(percent1))
                percentStr1 = "N/A";
            else
                percentStr1 = Math.Round(percent1, 1) + "%";
            itemsRarityText[i].text = percentStr1;
            itemsRarityColumns[i].fillAmount = diagramPercent / 100F;
        }
    }

    /// <summary>
    /// Converts seconds to days, hours, minutes and seconds.
    /// </summary>
    /// <param name="total">Total seconds - input.</param>
    public string FormatSeconds(ulong total)
    {
        byte seconds = (byte)(total % 60);
        byte minutes = (byte)(total % 3600 / 60);
        byte hours = (byte)(total % 86400 / 3600);
        short days = (short)(total / 86400);
        return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);
    }
}
