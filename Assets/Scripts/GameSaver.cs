using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using GooglePlayGames.BasicApi.SavedGame;

public class GameSaver : MonoBehaviour
{
	public Inventory inventory;
	public ComputerScript comp;
	public OSScript osScript;
	public Toggle soundToggle, vibroToggle, FPSToggle;
	public EnumerationSetting langSetting;
	public MoneySystem moneySys;

	private static SavesPack savesPack;
	public static string versionOfSaves;
	public static float loadProgress;
	public static string loadStatus;
	public static List<SavesPack.TimeLog> timeLogs = new List<SavesPack.TimeLog>();
	private static bool dataSetted = false;

	public static SavesPack Saves
	{
		get
		{
			if (savesPack == null)
			{
				//Debug.Log("savesPack == null.");
				return SavesPack.Default;
			}
			else
				return savesPack;
		}
		set
		{
			savesPack = value;
		}
	}

	public static long? NetworkTime
	{
		get
		{
			try
			{
				string server = "1.ua.pool.ntp.org";
				byte[] data = new byte[48];
				//                   |er|ver|mod|
				//Set first data byte 00 011 011 = 1B (hex).
				data[0] = 0x1B;
				IPAddress[] addresses = Dns.GetHostEntry(server).AddressList;
				IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
				{
					socket.Connect(ipEndPoint);
					socket.ReceiveTimeout = 3000;

					socket.Send(data);
					socket.Receive(data);
					socket.Close();
				}
				uint seconds = BitConverter.ToUInt32(data, 40);
				seconds = SwapEndian(seconds);

				return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds).Ticks / 10000000;
			}
			catch
			{
				return null;
			}
		}
	}

	private static uint SwapEndian(uint input)
	{
		return ((input & 0x000000ff) << 24) +  // First byte
			((input & 0x0000ff00) << 8) +   // Second byte
			((input & 0x00ff0000) >> 8) +   // Third byte
			((input & 0xff000000) >> 24);   // Fourth byte
	}

	private void Awake()
	{
		if (!dataSetted)
		{
			SetData(false);
			timeLogs.Add(new SavesPack.TimeLog(true));
		}
	}

	private void OnApplicationQuit()
	{
		timeLogs.Add(new SavesPack.TimeLog(false));
		if (timeLogs.Count > 100)
			timeLogs.RemoveAt(0);
		CollectData();
		Save();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			timeLogs.Add(new SavesPack.TimeLog(false));
			if (timeLogs.Count > 100)
				timeLogs.RemoveAt(0);
			CollectData();
			Save();
		}
		else
		{
			void OnRead(SavedGameRequestStatus status, byte[] data)
			{
				Load(data);
				SetData(true);
			}
			GPGSManager.ReadSaveData(GPGSManager.DEFAULT_SAVE_NAME, OnRead);
		}
	}

	private static void Save()
	{
		CollectDataThere();
		string packSerialized = KlimSoft.Serializer.Serialize(Saves);

		byte[] dataToSave = Encoding.UTF8.GetBytes(packSerialized);
		GPGSManager.WriteSaveData(dataToSave);
	}

	public static async void LoadAsync(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			Saves = SavesPack.Default;
			loadProgress = 1F;
			loadStatus = "No saves";
		}
		else
		{
			loadProgress = 0.33F;
			loadStatus = "Encoding...";

			string packSerialized = null;
			await Task.Run(() => packSerialized = Encoding.UTF8.GetString(data));

			loadProgress = 0.67F;
			loadStatus = "Deserializing...";

			Saves = (SavesPack)KlimSoft.Serializer.Deserialize(packSerialized, typeof(SavesPack));

			loadProgress = 1F;
			loadStatus = "Readed!";
		}
	}

	private static void Load(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			Saves = SavesPack.Default;
		}
		else
		{
			string packSerialized = Encoding.UTF8.GetString(data);
			Saves = (SavesPack)KlimSoft.Serializer.Deserialize(packSerialized, typeof(SavesPack));
		}
	}

	/// <summary>
	/// Collects static data from this script to saves pack.
	/// </summary>
	private static void CollectDataThere()
	{
		Saves.version = Application.version;
		Saves.timeLogs = timeLogs.ToArray();
		Saves.lastSession = DateTime.Now;
	}

	/// <summary>
	/// Set static data in this script from saves pack.
	/// </summary>
	private static void SetDataThere()
	{
		versionOfSaves = Saves.version;
		timeLogs = Saves.timeLogs.ToList();
	}

	/// <summary>
	/// Collects data from game to saves pack.
	/// </summary>
	private void CollectData()
	{
		//Regenerate image names in inventory and computer.
		Inventory.components.ForEach(x => x.RegenerateImage());

		if (ComputerScript.mainCPU != null)
			ComputerScript.mainCPU.RegenerateImage();

		if (ComputerScript.mainMotherboard != null)
			ComputerScript.mainMotherboard.RegenerateImage();

		foreach (GPU gpu in ComputerScript.GPUs)
			if (gpu != null)
				gpu.RegenerateImage();

		foreach (RAM ram in ComputerScript.RAMs)
			if (ram != null)
				ram.RegenerateImage();

		//Collect data from game.
		Saves = new SavesPack
		{
			invCPUs = Inventory.components.Where(x => x is CPU).Select(x => (CPU)x).ToArray(),
			invGPUs = Inventory.components.Where(x => x is GPU).Select(x => (GPU)x).ToArray(),
			invRAMs = Inventory.components.Where(x => x is RAM).Select(x => (RAM)x).ToArray(),
			invBoards = Inventory.components.Where(x => x is Motherboard).Select(x => (Motherboard)x).ToArray(),

			cpu = ComputerScript.mainCPU,
			motherboard = ComputerScript.mainMotherboard,
			gpus = ComputerScript.GPUs.ToArray(),
			rams = ComputerScript.RAMs.ToArray(),
			cpuCooler = ComputerScript.cpuCooler.level,
			cpuClock = OSScript.cpuClock,
			powerOn = OSScript.instance.powerToggler.Toggled,
			mined = OSScript.earned,

			lastSession = DateTime.Now,

			casesOpened = StatisticsScript.casesOpened,
			itemsScrolled = StatisticsScript.itemsScrolled,
			CPUsDropped = StatisticsScript.CPUsDropped,
			GPUsDropped = StatisticsScript.GPUsDropped,
			RAMsDropped = StatisticsScript.RAMsDropped,
			motherboardsDropped = StatisticsScript.motherboardsDropped,
			componentsSold = StatisticsScript.componentsSold,
			moneyEarnedBySale = StatisticsScript.moneyEarnedBySale,
			moneyWonInMinigames = StatisticsScript.moneyWonInMinigames,
			moneyLostInMinigames = StatisticsScript.moneyLostInMinigames,
			gameLaunches = StatisticsScript.gameLaunches,
			gameplayTime = StatisticsScript.gameplayTime,
			droppedByRarities = StatisticsScript.droppedByRarities,
			CPUsDroppedByCases = StatisticsScript.CPUsDroppedByCases,
			GPUsDroppedByCases = StatisticsScript.GPUsDroppedByCases,
			RAMsDroppedByCases = StatisticsScript.RAMsDroppedByCases,
			motherboardsDroppedByCases = StatisticsScript.motherboardsDroppedByCases,
			generalDroppedByCases = StatisticsScript.generalDroppedByCases,

			sound = soundToggle.toggled,
			vibro = vibroToggle.toggled,
			showFPS = FPSToggle.toggled,
			lang = langSetting.Index,

			money = MoneySystem.Money.Value,
			BTCMoney = MoneySystem.BTCMoney.Value,

			usedCodes = EnterCode.usedHashes.ToArray()
		};
		CollectDataThere();
	}

	/// <summary>
	/// Set game data from saves pack.
	/// </summary>
	private void SetData(bool includingScripts)
	{
		if (includingScripts)
		{
			Inventory.ApplySaves();
			ComputerScript.ApplySaves();
			OSScript.ApplySaves();
			StatisticsScript.ApplySaves();
			MoneySystem.ApplySaves();
			EnterCode.ApplySaves();
		}

		soundToggle.toggled = Saves.sound;
		vibroToggle.toggled = Saves.vibro;
		FPSToggle.toggled = Saves.showFPS;
		langSetting.Index = Saves.lang;

		SetDataThere();

		dataSetted = true;
	}
}

public class SavesPack
{
	//Inventory.
	public CPU[] invCPUs;
	public GPU[] invGPUs;
	public RAM[] invRAMs;
	public Motherboard[] invBoards;

	//Computer.
	public CPU cpu;
	public Motherboard motherboard;
	public GPU[] gpus;
	public RAM[] rams;
	public byte cpuCooler;
	public decimal mined;
	public uint cpuClock;
	public bool powerOn;
	public DateTime lastSession;

	//Statistics.
	public ulong casesOpened, itemsScrolled, CPUsDropped, GPUsDropped, RAMsDropped,
		motherboardsDropped, componentsSold, moneyEarnedBySale, moneyWonInMinigames,
		moneyLostInMinigames, gameLaunches;
	public long gameplayTime;
	public ulong[] droppedByRarities, CPUsDroppedByCases, GPUsDroppedByCases,
		RAMsDroppedByCases, motherboardsDroppedByCases, generalDroppedByCases;

	//Settings.
	public bool sound, vibro, showFPS;
	public int lang;

	//Balance.
	public long money;
	public decimal BTCMoney;

	//Codes
	public byte[][] usedCodes;

	//Version of game which saved this pack.
	public string version;

	public TimeLog[] timeLogs;

	//Anti cheat bans count.
	public int bans;
	public bool banned;
	public long? banTime;

	public static SavesPack Default
	{
		get
		{
			SavesPack res = new SavesPack
			{
				invCPUs = new CPU[0],
				invGPUs = new GPU[0],
				invBoards = new Motherboard[0],
				invRAMs = new RAM[0],
				gpus = new GPU[0],
				rams = new RAM[0],
				droppedByRarities = new ulong[5],
				CPUsDroppedByCases = new ulong[4],
				GPUsDroppedByCases = new ulong[4],
				RAMsDroppedByCases = new ulong[4],
				motherboardsDroppedByCases = new ulong[4],
				generalDroppedByCases = new ulong[4],
				sound = true,
				vibro = true,
				showFPS = false,
				version = Application.version,
				timeLogs = new TimeLog[0],
				lastSession = DateTime.MinValue
				//Other is default by default.
			};
			//Set language.
			switch (Application.systemLanguage)
			{
				case SystemLanguage.English:
					res.lang = 0;
					break;
				case SystemLanguage.Russian:
					res.lang = 1;
					break;
				case SystemLanguage.Ukrainian:
					res.lang = 2;
					break;
				default:
					res.lang = 0;
					break;
			}
			return res;
		}
	}

	public class TimeLog
	{
		public long sys;

		/// <summary>
		/// Is player entered (true) or leaved (false) from game?
		/// </summary>
		public bool entered;

		public TimeLog()
		{

		}

		public TimeLog(bool entered)
		{
			this.entered = entered;
			GetTime();
		}

		private void GetTime()
		{
			//Get system time.
			//Tick - 100 ns. In 1 s 1 000 ms -> 1 000 000 mcs -> 10 000 000 * 100 ns.
			sys = DateTime.UtcNow.Ticks / 10000000;
		}
	}
}