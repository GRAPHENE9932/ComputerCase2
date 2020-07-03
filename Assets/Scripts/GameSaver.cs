using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class GameSaver : MonoBehaviour
{
    public Inventory inventory;
    public ComputerScript comp;
    public OSScript osScript;
    public Toggle soundToggle, vibroToggle, FPSToggle;
    public EnumerationSetting langSetting;
    public MoneySystem moneySys;

    public static SavesPack savesPack;
    public static string versionOfSaves;
    public static float loadProgress;
    public static string loadStatus;
    public static List<SavesPack.TimeLog> timeLogs = new List<SavesPack.TimeLog>();
    private static string path;

    private void Awake()
    {
        SetData();
    }

    private void OnApplicationQuit()
    {
        timeLogs.Add(new SavesPack.TimeLog(false));
        if (timeLogs.Count > 500)
            timeLogs.RemoveAt(0);
        CollectData();
        Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            timeLogs.Add(new SavesPack.TimeLog(false));
            if (timeLogs.Count > 500)
                timeLogs.RemoveAt(0);
            CollectData();
            Save();
        }
        else
        {
            Load();
            timeLogs.Add(new SavesPack.TimeLog(true));
            if (timeLogs.Count > 500)
                timeLogs.RemoveAt(0);
            SetData();
        }
    }

    private static void Save()
    {
        //Save file in dataPath if this game in editor, else save file in persistentDataPath.
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Saves.pack");
#else
        path = Path.Combine(Application.dataPath, "Saves.pack");
#endif

        CollectDataThere();
        string packSerialized = KlimSoft.Serializer.Serialize(savesPack);
        Debug.Log(packSerialized);
        byte[] dataToSave = Encrypt(Encoding.UTF8.GetBytes(packSerialized));
        File.WriteAllBytes(path, dataToSave);
    }

    public static async void LoadAsync()
    {
        //Load file from dataPath if this game in editor, else read file from persistentDataPath.
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Saves.pack");
#else
        path = Path.Combine(Application.dataPath, "Saves.pack");
#endif
        if (File.Exists(path))
        {
            loadProgress = 0.01F;
            loadStatus = "Reading...";

            byte[] encryptedData = null;
            await Task.Run(() => encryptedData = File.ReadAllBytes(path));

            loadProgress = 0.2F;
            loadStatus = "Decrypting...";

            byte[] decryptedData = await DecryptAsync(encryptedData);

            loadProgress = 0.5F;
            loadStatus = "Encoding...";

            string packSerialized = null;
            await Task.Run(() => packSerialized = Encoding.UTF8.GetString(decryptedData));

            loadProgress = 0.75F;
            loadStatus = "Deserializing...";

            savesPack = (SavesPack)KlimSoft.Serializer.Deserialize(packSerialized, typeof(SavesPack));
        }
        else
        {
            savesPack = SavesPack.Default;
            loadProgress = 1F;
            loadStatus = "No saves";
        }

        SetDataThere();
    }

    private static void Load()
    {
        //Load file from dataPath if this game in editor, else read file from persistentDataPath.
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Saves.pack");
#else
        path = Path.Combine(Application.dataPath, "Saves.pack");
#endif
        SavesPack pack;
        if (File.Exists(path))
        {
            byte[] encryptedData = File.ReadAllBytes(path);
            byte[] decryptedData = Decrypt(encryptedData);
            string packSerialized = Encoding.UTF8.GetString(decryptedData);
            pack = (SavesPack)KlimSoft.Serializer.Deserialize(packSerialized, typeof(SavesPack));
        }
        else
            pack = SavesPack.Default;

        SetDataThere();
    }

    private static byte[] Encrypt(byte[] data)
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

    private static byte[] Decrypt(byte[] data)
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

    private static async Task<byte[]> DecryptAsync(byte[] data)
    {
        //This on main thread, else => EXCEPTION D:
        string id = SystemInfo.deviceUniqueIdentifier;
        byte[] result = null;
        //Async part.
        await Task.Run(() => {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            SHA384 hash = SHA384.Create();
            //Unique hash - is hash of device unique identifier.
            byte[] uniqueHash = hash.ComputeHash(HexToBytes(id));

            //  |          256-bit key          |  128-bit IV  |
            //  00112233445566778899AABBCCDDEEFF0011223344556677
            //  |              384-bit uniqueHash              |
            aes.Key = uniqueHash.Take(32).ToArray();
            aes.IV = uniqueHash.Skip(32).ToArray();

            result = aes.CreateDecryptor()
                .TransformFinalBlock(data, 0, data.Length);
        });
        return result;
    }

    /// <summary>
    /// Collects static data from this script to saves pack.
    /// </summary>
    private static void CollectDataThere()
    {
        savesPack.version = Application.version;
        savesPack.timeLogs = timeLogs.ToArray();
        savesPack.lastSession = DateTime.Now;
    }

    /// <summary>
    /// Set static data in this script from saves pack.
    /// </summary>
    private static void SetDataThere()
    {
        versionOfSaves = savesPack.version;
        timeLogs = savesPack.timeLogs.ToList();
    }

    /// <summary>
    /// Collects data from game to saves pack.
    /// </summary>
    private void CollectData()
    {
        //Regenerate image names in inventory.
        Inventory.components.ForEach(x => x.RegenerateImage());
        //Collect data from game.
        savesPack = new SavesPack
        {
            inventoryCPUs = Inventory.components.Where(x => x is CPU).Select(x => (CPU)x).ToArray(),
            inventoryGPUs = Inventory.components.Where(x => x is GPU).Select(x => (GPU)x).ToArray(),
            inventoryRAMs = Inventory.components.Where(x => x is RAM).Select(x => (RAM)x).ToArray(),
            inventoryMotherboards = Inventory.components.Where(x => x is Motherboard).Select(x => (Motherboard)x).ToArray(),

            cpu = ComputerScript.mainCPU,
            motherboard = ComputerScript.mainMotherboard,
            gpus = ComputerScript.GPUs.ToArray(),
            rams = ComputerScript.RAMs.ToArray(),
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
            lang = langSetting.Index,

            money = MoneySystem.Money.Value,
            BTCMoney = MoneySystem.BTCMoney.Value
        };
        CollectDataThere();
    }

    /// <summary>
    /// Set game data from saves pack.
    /// </summary>
    private void SetData()
    {
        Inventory.ApplySaves();

        ComputerScript.ApplySaves();

        OSScript.ApplySaves();

        StatisticsScript.ApplySaves();

        soundToggle.toggled = savesPack.sound;
        vibroToggle.toggled = savesPack.vibration;
        FPSToggle.toggled = savesPack.showFPS;
        langSetting.Index = savesPack.lang;

        MoneySystem.ApplySaves();

        SetDataThere();
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
        moneyWonInCasino, moneyLostInCasino, gameLaunches;
    public long gameplayTime;
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

    public TimeLog[] timeLogs;

    public static SavesPack Default
    {
        get
        {
            SavesPack res = new SavesPack
            {
                inventoryCPUs = new CPU[0],
                inventoryGPUs = new GPU[0],
                inventoryMotherboards = new Motherboard[0],
                inventoryRAMs = new RAM[0],
                gpus = new GPU[0],
                rams = new RAM[0],
                droppedByRarities = new ulong[5],
                CPUsDroppedByCases = new ulong[4],
                GPUsDroppedByCases = new ulong[4],
                RAMsDroppedByCases = new ulong[4],
                motherboardsDroppedByCases = new ulong[4],
                generalDroppedByCases = new ulong[4],
                sound = true,
                vibration = true,
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
        private const string server = "1.ua.pool.ntp.org";

        public long sys;
        public long? net;

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

            //Get system time.
            //Tick - 100 ns. In 1 s 1 000 ms -> 1 000 000 mcs -> 10 000 000 * 100 ns.
            sys = DateTime.UtcNow.Ticks / 10000000;

            //Get network time.
            try
            {
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

                net = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds).Ticks / 10000000;
            }
            catch
            {
                net = null;
            }
        }

        private uint SwapEndian(uint input)
        {
            return ((input & 0x000000ff) << 24) +  // First byte
                ((input & 0x0000ff00) << 8) +   // Second byte
                ((input & 0x00ff0000) >> 8) +   // Third byte
                ((input & 0xff000000) >> 24);   // Fourth byte
        }
    }
}