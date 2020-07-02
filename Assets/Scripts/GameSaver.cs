using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;

public class GameSaver : MonoBehaviour
{
    public Inventory inventory;
    public ComputerScript comp;
    public OSScript osScript;
    public Toggle soundToggle, vibroToggle, FPSToggle;
    public EnumerationSetting langSetting;
    public MoneySystem moneySys;

    public static string versionOfSaves;

    private string path;

    private bool gameLoaded;

    private void Awake()
    {
        if (!gameLoaded)
        Load();
        gameLoaded = true;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Save();
            gameLoaded = false;
        }
        else
        {
            if (!gameLoaded)
                Load();
            gameLoaded = true;
        }
    }

    private void OnApplicationQuit()
    {
        Save();
        gameLoaded = false;
    }

    private void Save()
    {
        //Save file in dataPath if this game in editor, else save file in persistentDataPath.
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Saves.pack");
#else
        path = Path.Combine(Application.dataPath, "Saves.pack");
#endif

        SavesPack pack = CollectData();
        string packSerialized = KlimSoft.Serializer.Serialize(pack);
        Debug.Log(packSerialized);
        byte[] dataToSave = Encrypt(Encoding.UTF8.GetBytes(packSerialized));
        File.WriteAllBytes(path, dataToSave);
    }

    private void Load()
    {
        //Load file from dataPath if this game in editor, else read file from persistentDataPath.
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Saves.pack");
#else
        path = Path.Combine(Application.dataPath, "Saves.pack");
#endif

        byte[] decryptedData = Decrypt(File.ReadAllBytes(path));
        string packSerialized = Encoding.UTF8.GetString(decryptedData);
        SavesPack pack = (SavesPack)KlimSoft.Serializer.Deserialize(packSerialized, typeof(SavesPack));
        SetData(pack);
    }

    private byte[] Encrypt(byte[] data)
    {
        Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        SHA384 hash = SHA384.Create();
        //Unique hash - is hash of device unique identifier.
        byte[] uniqueHash = hash.ComputeHash(HexToBytes(SystemInfo.deviceUniqueIdentifier));

        //  |          256-bit key          |  128-bit IV  |
        //  00112233445566778899AABBCCDDEEFF0011223344556677
        //  |              384-bit uniqueHash              |
        aes.Key = uniqueHash.Take(32).ToArray();
        aes.IV = uniqueHash.Skip(32).ToArray();

        return aes.CreateEncryptor()
            .TransformFinalBlock(data, 0, data.Length);
    }

    private byte[] Decrypt(byte[] data)
    {
        Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        SHA384 hash = SHA384.Create();

        //Unique hash - is hash of device unique identifier.
        byte[] uniqueHash = hash.ComputeHash(HexToBytes(SystemInfo.deviceUniqueIdentifier));

        //  |          256-bit key          |  128-bit IV  |
        //  00112233445566778899AABBCCDDEEFF0011223344556677
        //  |              384-bit uniqueHash              |
        aes.Key = uniqueHash.Take(32).ToArray();
        aes.IV = uniqueHash.Skip(32).ToArray();

        return aes.CreateDecryptor()
            .TransformFinalBlock(data, 0, data.Length);
    }

    /// <summary>
    /// Collects data from game to saves pack.
    /// </summary>
    private SavesPack CollectData()
    {
        //Regenerate image names in inventory.
        inventory.components.ForEach(x => x.RegenerateImage());
        
        //Regenerate image names in computer.
        if (comp.mainCPU != null)
            comp.mainCPU.RegenerateImage();

        if (comp.mainMotherboard != null)
            comp.mainMotherboard.RegenerateImage();
        
        comp.GPUs.Where(x => x != null).ToList().ForEach(x => x.RegenerateImage());

        comp.RAMs.Where(x => x != null).ToList().ForEach(x => x.RegenerateImage());

        //Collect data from game.
        SavesPack result = new SavesPack
        {
            inventoryCPUs = inventory.components.Where(x => x is CPU).Select(x => (CPU)x).ToArray(),
            inventoryGPUs = inventory.components.Where(x => x is GPU).Select(x => (GPU)x).ToArray(),
            inventoryRAMs = inventory.components.Where(x => x is RAM).Select(x => (RAM)x).ToArray(),
            inventoryMotherboards = inventory.components.Where(x => x is Motherboard).Select(x => (Motherboard)x).ToArray(),

            cpu = comp.mainCPU,
            motherboard = comp.mainMotherboard,
            gpus = comp.GPUs.ToArray(),
            rams = comp.RAMs.ToArray(),
            mined = osScript.earned,
            lastSession = DateTime.Now,

            casesOpened = StatisticsScript.casesOpened,
            itemsScrolled = StatisticsScript.itemsScrolled,
            CPUsDropped = StatisticsScript.CPUsDropped,
            GPUsDropped = StatisticsScript.GPUsDropped,
            RAMsDropped = StatisticsScript.RAMsDropped,
            motherboardsDropped = StatisticsScript.motherboardsDropped,
            componentsSold = StatisticsScript.componentsSold,
            moneyEarnedBySale = StatisticsScript.moneyEarnedBySale,
            moneyWonInCasino = StatisticsScript.moneyWonInCasino,
            moneyLostInCasino = StatisticsScript.moneyLostInCasino,
            gameLaunches = StatisticsScript.gameLaunches,
            gameplayTime = StatisticsScript.gameplayTime,
            droppedByRarities = StatisticsScript.droppedByRarities,
            CPUsDroppedByCases = StatisticsScript.CPUsDroppedByCases,
            GPUsDroppedByCases = StatisticsScript.GPUsDroppedByCases,
            RAMsDroppedByCases = StatisticsScript.RAMsDroppedByCases,
            motherboardsDroppedByCases = StatisticsScript.motherboardsDroppedByCases,
            generalDroppedByCases = StatisticsScript.generalDroppedByCases,

            sound = soundToggle.toggled,
            vibration = vibroToggle.toggled,
            showFPS = FPSToggle.toggled,
            lang = langSetting.index,

            money = moneySys.Money.Value,
            BTCMoney = moneySys.BTCMoney.Value,

            version = Application.version
        };
        return result;
    }

    /// <summary>
    /// Set data in game from saves pack.
    /// </summary>
    private void SetData(SavesPack pack)
    {
        inventory.components.Clear();
        inventory.components.AddRange(pack.inventoryCPUs);
        inventory.components.AddRange(pack.inventoryGPUs);
        inventory.components.AddRange(pack.inventoryRAMs);
        inventory.components.AddRange(pack.inventoryMotherboards);

        inventory.components.ForEach(x => x.RegenerateImage());

        comp.mainCPU = pack.cpu;
        if (comp.mainCPU != null)
            comp.mainCPU.RegenerateImage();

        comp.mainMotherboard = pack.motherboard;
        if (comp.mainMotherboard != null)
            comp.mainMotherboard.RegenerateImage();

        comp.GPUs = pack.gpus.ToList();
        comp.GPUs.Where(x => x != null).ToList().ForEach(x => x.RegenerateImage());

        comp.RAMs = pack.rams.ToList();
        comp.RAMs.Where(x => x != null).ToList().ForEach(x => x.RegenerateImage());

        osScript.earned = pack.mined;

        //Mine while game turned OFF.
        decimal performance = osScript.Performance;
        if (performance == -1)
            osScript.earned = pack.mined;
        else
            osScript.earned = pack.mined + osScript.Performance * (decimal)(DateTime.Now - pack.lastSession).TotalDays;
        if (osScript.earned > osScript.Capacity)
            osScript.earned = osScript.Capacity;

        StatisticsScript.casesOpened = pack.casesOpened;
        StatisticsScript.itemsScrolled = pack.itemsScrolled;
        StatisticsScript.CPUsDropped = pack.CPUsDropped;
        StatisticsScript.GPUsDropped = pack.GPUsDropped;
        StatisticsScript.RAMsDropped = pack.RAMsDropped;
        StatisticsScript.motherboardsDropped = pack.motherboardsDropped;
        StatisticsScript.componentsSold = pack.componentsSold;
        StatisticsScript.moneyEarnedBySale = pack.moneyEarnedBySale;
        StatisticsScript.moneyWonInCasino = pack.moneyWonInCasino;
        StatisticsScript.moneyLostInCasino = pack.moneyLostInCasino;
        StatisticsScript.gameLaunches = pack.gameLaunches;
        StatisticsScript.gameplayTime = pack.gameplayTime;
        StatisticsScript.droppedByRarities = pack.droppedByRarities;
        StatisticsScript.CPUsDroppedByCases = pack.CPUsDroppedByCases;
        StatisticsScript.GPUsDroppedByCases = pack.GPUsDroppedByCases;
        StatisticsScript.RAMsDroppedByCases = pack.RAMsDroppedByCases;
        StatisticsScript.motherboardsDroppedByCases = pack.motherboardsDroppedByCases;
        StatisticsScript.generalDroppedByCases = pack.generalDroppedByCases;

        soundToggle.toggled = pack.sound;
        vibroToggle.toggled = pack.vibration;
        FPSToggle.toggled = pack.showFPS;
        langSetting.index = pack.lang;

        moneySys.Money = new SecureLong(pack.money);
        moneySys.BTCMoney = new SecureDecimal(pack.BTCMoney);

        versionOfSaves = pack.version;
    }

    private static byte[] HexToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hexadecimal string has invalid length.");

        byte[] result = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length / 2; i++)
            result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

        return result;
    }
}

[Serializable]
public class SavesPack
{
    //Inventory.
    public CPU[] inventoryCPUs;
    public GPU[] inventoryGPUs;
    public RAM[] inventoryRAMs;
    public Motherboard[] inventoryMotherboards;

    //Computer.
    public CPU cpu;
    public Motherboard motherboard;
    public GPU[] gpus;
    public RAM[] rams;
    public decimal mined;
    public DateTime lastSession;

    //Statistics.
    public ulong casesOpened, itemsScrolled, CPUsDropped, GPUsDropped, RAMsDropped, motherboardsDropped, componentsSold, moneyEarnedBySale,
        moneyWonInCasino, moneyLostInCasino, gameLaunches, gameplayTime;
    public ulong[] droppedByRarities, CPUsDroppedByCases, GPUsDroppedByCases, RAMsDroppedByCases, motherboardsDroppedByCases,
        generalDroppedByCases;

    //Settings.
    public bool sound, vibration, showFPS;
    public int lang;

    //Balance.
    public long money;
    public decimal BTCMoney;

    //Version of game which saved this pack.
    public string version;
}