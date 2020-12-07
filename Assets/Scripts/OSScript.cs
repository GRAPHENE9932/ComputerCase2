using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using KlimSoft;

public enum OSPage
{
	SystemConfiguration, Recommendations, Mining, Overclocking, None
}

public class OSScript : MonoBehaviour
{
	OSPage currentPage;
	/// <summary>
	/// Parents of page objects.
	/// </summary>
	public GameObject[] pages;
	public GameObject pagesObj;
	public GameObject startObj;
	/// <summary>
	/// Text in field of configuration page.
	/// </summary>
	public Text CPUModel, CPUFrequency, CPUBusWidth, RAMFrequency, RAMGeneration,
		RAMVolume, socket, chipset;
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
	public Text earnedText, capacityText, performanceText0, performanceText1,
		performanceText2, memoryUsedText;
	/// <summary>
	/// Text in progressbar of mining page.
	/// </summary>
	public Text miningProgressbarText0, miningProgressbarText1;
	/// <summary>
	/// Mining progressbar bar. Used FillAmount.
	/// </summary>
	public Image miningProgressbar;

	public Text cpuNormalClockText;
	public Text cpuCurrentClockText;
	public Text cpuNewClockText;
	public Text tempText;
	public Slider cpuOverclockSlider;
	public ButtonColorToggler powerToggler;

	public static uint cpuClock;
	public Animation powerScreenAnim;
	public Text errorsText;
	public GameObject errorsObj;

	public static float temperature, deltaTemperature;

	public static OSScript instance;

	public uint CpuClock
	{
		get
		{
			if (cpuClock == 0)
				return ComputerScript.mainCPU.frequency;
			else
				return cpuClock;
		}
	}

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
				performance += ComputerScript.mainCPU.IPC *
					(ComputerScript.mainCPU.frequency / 1000m)
					* ComputerScript.mainCPU.cores * 0.0002m;

				//Find minimum RAM frequency.
				int minRAMFreq = int.MaxValue;
				for (int i = 0; i < ComputerScript.RAMs.Count; i++)
					if (ComputerScript.RAMs[i] != null &&
						ComputerScript.RAMs[i].frequency < minRAMFreq)
						minRAMFreq = ComputerScript.RAMs[i].frequency;

				//Add performance from RAM.
				performance += minRAMFreq * 0.0000125m;

				if (ComputerScript.RAMs.All(x => x == null))
					return -1;

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

	public uint NewCPUClock
	{
		get
		{
			uint minClock = ComputerScript.mainCPU.frequency / 2;
			uint maxClock = ComputerScript.mainCPU.frequency * 2;

			return (uint)Mathf.Lerp(minClock, maxClock, cpuOverclockSlider.value);
		}
		set
		{
			uint minClock = ComputerScript.mainCPU.frequency / 2;
			uint maxClock = ComputerScript.mainCPU.frequency * 2;

			cpuOverclockSlider.value = Mathf.InverseLerp(minClock, maxClock, value);
		}
	}

    private void Awake()
    {
		instance = this;
    }

    private void Start()
	{
		//Set saves in start instead of awake because mining need begin with
		//mining coroutine and saves of computer in awake.
		ApplySaves();
		StartCoroutine(MiningCoroutine());
		StartCoroutine(UpdateTemperature());
	}

	public static void ApplySaves()
	{
		earned = GameSaver.Saves.mined;
		cpuClock = GameSaver.Saves.cpuClock;
		instance.powerToggler.Toggled = GameSaver.Saves.powerOn;
		//Calculate money, mined during game is closed
		if (GameSaver.Saves.lastSession != DateTime.MinValue && Performance != -1
			&& instance.powerToggler.Toggled)
			earned += (decimal)(DateTime.Now - GameSaver.Saves.lastSession)
				.TotalDays * Performance;
		if (earned > Capacity)
			earned = Capacity;
	}
	/// <summary>
	/// Start OS, when monitor clicked.
	/// </summary>
	public void StartOS()
	{
		//Show black screen if computer off
		if (powerToggler.Toggled)
		{
			pagesObj.SetActive(true);

			//Disable all pages.
			for (int i = 0; i < pages.Length; i++)
				pages[i].SetActive(false);

			currentPage = OSPage.None;
		}
        else
        {
			pagesObj.SetActive(false);
        }
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
		RAMGeneration.text = "DDR" + (ComputerScript.mainMotherboard.RAMType == 1 ?
			null :
			ComputerScript.mainMotherboard.RAMType.ToString());

		int ramVol = 0;
		for (int i = 0; i < ComputerScript.RAMs.Count; i++)
			if (ComputerScript.RAMs[i] != null)
				ramVol += ComputerScript.RAMs[i].memory;

		RAMVolume.text = $"{ramVol} MB";

		socket.text = ComputerScript.mainMotherboard.socket;
		chipset.text = ComputerScript.mainMotherboard.chipset.ToString().RemoveChar('_');

		integratedGraphics.sprite = ComputerScript.mainCPU.integratedGraphics ?
			checkChecked :
			checkUnchecked;
		multiplierUnlocked.sprite = ComputerScript.mainCPU.unlocked ?
			checkChecked :
			checkUnchecked;
		SLI.sprite = ComputerScript.mainMotherboard.SLI ?
			checkChecked :
			checkUnchecked;
		crossfire.sprite = ComputerScript.mainMotherboard.crossfire ?
			checkChecked : 
			checkUnchecked;

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
			//If GPU with index i != null, button.interactable = true.
			if (ComputerScript.GPUs[i] != null)
			{
				GPUButtons[i].GetComponentInChildren<Button>().interactable = true;
				//Add event of clicked button.
				int index = i;
				GPUButtons[i].GetComponentInChildren<Button>().onClick.AddListener(
					delegate { GPUPropertiesButton(index); });
				GPUButtons[i].GetComponentInChildren<EventTrigger>().triggers =
					triggersForButton;
			}
			else
			{
				//Disabled image is image in button with color #FFFFFF80.
				//If turn on this image, button and text of button getting gray.
				GPUButtons[i].transform.Find("Button").Find("Disabled image").gameObject
					.SetActive(true);
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
		GPUBusInterface.text = $"PCIe {ComputerScript.GPUs[index].busVersion}.0 " +
			$"x{ComputerScript.GPUs[index].busMultiplier}";
		GPUMemoryVolume.text = $"{ComputerScript.GPUs[index].memory} MB";
	}

	public void UpdateRecommendations()
	{
		//Add recomendations.
		string recommendations = null;

		//Check for enabled multiple channel mode of RAM.
		if (CheckMultipleChannels() && ComputerScript.RAMs.Count(x => x != null) >=
			ComputerScript.mainMotherboard.RAMCount / 2)
			recommendations += string.Format(LangManager.GetString("ram_channel_recom"),
				ComputerScript.mainMotherboard.RAMCount / 2);

		recommendationsText.text = recommendations;
	}

	public void UpdateMining()
	{
		earnedText.text = $"{Math.Round(earned, 15)} BTC";
		capacityText.text = $"{Math.Round(Capacity, 15)} BTC";
		performanceText0.text = $"{Math.Round(Performance / 1440, 15)}" +
			$" BTC/{LangManager.GetString("min")}";
		performanceText1.text = $"{Math.Round(Performance / 24, 15)} " +
			$"BTC/{LangManager.GetString("hour")}";
		performanceText2.text = $"{Math.Round(Performance, 15)}" +
			$" BTC/{LangManager.GetString("day")}";
		int totalRAM = 0;
		for (int i = 0; i < ComputerScript.RAMs.Count; i++)
			if (ComputerScript.RAMs[i] != null)
				totalRAM += ComputerScript.RAMs[i].memory;
		memoryUsedText.text = $"{Math.Round(earned / Capacity * totalRAM, 2)}/{totalRAM} MB";

		float progress = (float)earned / (float)Capacity;
		miningProgressbar.fillAmount = progress;
		miningProgressbarText0.text = $"{Math.Round(progress, 4) * 100}%";
		miningProgressbarText1.text = $"{Math.Round(progress, 4) * 100}%";
	}

    #region OVERCLOCKING

    public void UpdateOverclocking()
	{
		//Update texts
		cpuNormalClockText.text = $"{ComputerScript.mainCPU.frequency} MHz";
		cpuCurrentClockText.text = $"{CpuClock} MHz";

		//Adjust slider
		uint minClock = ComputerScript.mainCPU.frequency / 2;
		uint maxClock = ComputerScript.mainCPU.frequency * 2;
		cpuOverclockSlider.value = Mathf.InverseLerp(minClock, maxClock, CpuClock);

		//Update text
		cpuNewClockText.text = $"{NewCPUClock} MHz";
	}

	const float CLOCK_FULL = 1000F;

	private float CpuTDP
    {
        get
        {
			float deltaClock = (float)CpuClock - (float)ComputerScript.mainCPU.frequency;
			float multiplier;
			if (deltaClock >= 0)
				multiplier = (deltaClock / CLOCK_FULL) * (deltaClock / CLOCK_FULL) + 1F;
			else
				multiplier = -(deltaClock / CLOCK_FULL) * (deltaClock / CLOCK_FULL) + 1F;

			if (multiplier < 0.25F)
				multiplier = 0.25F;

			return ComputerScript.mainCPU.TDP * multiplier;
        }
    }

	//1 extra watt of TDP equals?
	const float WATT_TO_DEG = 1F;
	//Temperature when CPU.TDP == cooler.power
	const float MAIN_TEMP = 100;
	const float TEMP_SMOOTHNESS = 5;
	const float TEMP_OFFSET = 30;
	const float CRITICAL_TEMP = 75;

	private IEnumerator UpdateTemperature()
    {
		WaitForSeconds wait = new WaitForSeconds(1);
		while (true)
        {
			if (comp.ComputerOK)
			{
				//Calculate delta temperature
				float delta = CpuTDP - (float)ComputerScript.cpuCooler.power;

				float targetTemp = powerToggler.Toggled ? delta * WATT_TO_DEG + MAIN_TEMP : 0;
				if (targetTemp < 0)
					targetTemp = 0;

				deltaTemperature = (targetTemp - temperature) /
					(TEMP_SMOOTHNESS * (powerToggler.Toggled ? 1F : 4F))
					+ UnityEngine.Random.Range(-1F, 1F);


				//Adjust temperature
				temperature += deltaTemperature;

				//Update text
				tempText.text = $"{LangManager.GetString("cpu:")} {temperature + TEMP_OFFSET:0}°C";
				//Adjust text color
				float colorH = (180F - 3F * temperature) / 360F;
				if (colorH > 0.5F)
					colorH = 0.5F;
				else if (colorH < 0F)
					colorH = 0F;
				tempText.color = Color.HSVToRGB(colorH, 1, 1);

				//Check for critical temperature
				if (temperature >= CRITICAL_TEMP)
				{
					TurnOff();

					errorsObj.SetActive(true);
					errorsText.text = string.Format(LangManager.GetString("critical_temp"),
						temperature + TEMP_OFFSET);

					OverclockReset();
				}
				else if (errorsObj.activeSelf)
				{
					errorsObj.SetActive(false);
					errorsText.text = null;
				}
			}
            else
            {
				temperature = 0;
            }

			yield return wait;
        }
    }

	public void OverclockSlider_ValueChanged()
	{
		//Update text
		cpuNewClockText.text = $"{NewCPUClock} MHz";
	}

	public void OverclockCancel()
	{
		NewCPUClock = CpuClock;
	}

	public void OverclockApply()
	{
		cpuClock = NewCPUClock;
		cpuCurrentClockText.text = $"{CpuClock} MHz";
	}

	public void OverclockReset()
	{
		cpuClock = ComputerScript.mainCPU.frequency;
		NewCPUClock = cpuClock;
		cpuCurrentClockText.text = $"{CpuClock} MHz";
	}

	#endregion

	Coroutine powerCoroutine;
    public void PowerButton_Clicked()
    {
		if (temperature < CRITICAL_TEMP)
		{
			if (powerCoroutine != null)
				StopCoroutine(powerCoroutine);
			powerCoroutine = StartCoroutine(PowerOnCoroutine());
		}
    }

	public void TurnOff()
    {
		pagesObj.SetActive(false);
		startObj.SetActive(false);
		powerToggler.Toggled = false;

		powerScreenAnim.Stop();
	}

	private const float transition = 0.5F;
	private IEnumerator PowerOnCoroutine()
    {
		if (powerToggler.Toggled)
		{
			startObj.SetActive(true);
			powerScreenAnim.Play();

			yield return new WaitForSeconds(powerScreenAnim.clip.length - transition);

			pagesObj.SetActive(true);
			yield return new WaitForSeconds(transition);
			startObj.SetActive(false);
		}
        else
        {
			TurnOff();
        }
	}

    public IEnumerator MiningCoroutine()
	{
		while (true)
		{
			if (powerToggler.Toggled)
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
		//usedDouble - is slots with indexes 0, 2, 4, 6, ... used?
		//True - used, false - unused, null - different.
		//usedNotDouble - is slots with indexes 1, 3, 5, 7, ... used?
		//True - used, false - unused, null - different.
		bool? usedDouble = ComputerScript.RAMs[0] != null,
			usedNotDouble = ComputerScript.RAMs[1] != null;
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
		return usedDouble == null && usedNotDouble == null;
	}
}
