using System;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using KlimSoft;
using System.Text;

public static class GPGSManager
{
    public const string LEADERBOARD = "CgkIwoSOgYodEAIQAg";
    public const string DEFAULT_SAVE_NAME = "MainSave";
    public const string CACHE_SAVE_NAME = "CachedSaves.enc";
    public static string cachePath = Path.Combine(Application.persistentDataPath, CACHE_SAVE_NAME);

    private static ISavedGameClient savedGameClient;
    private static ISavedGameMetadata currentMetadata;

    private static DateTime startDateTime;

    public static bool IsAuthenticated
    {
        get
        {
#if UNITY_ANDROID
            if (PlayGamesPlatform.Instance != null)
                return PlayGamesPlatform.Instance.IsAuthenticated();
            else
                return false;
#else
            return false;
#endif
        }
    }

    public static void Initialize(bool debug)
    {
#if UNITY_ANDROID
        //Initialize GPG.
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = debug;
        PlayGamesPlatform.Activate();
#endif
    }

    public static void Auth(Action<bool, string> onAuth)
    {
#if UNITY_ANDROID
        Social.localUser.Authenticate((success, callback) =>
        {
            if (success)
                savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            onAuth(success, callback);
        });
#endif
    }
    public static void Auth(Action<bool> onAuth)
    {
#if UNITY_ANDROID
        Social.localUser.Authenticate((success) =>
        {
            if (success)
                savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            onAuth(success);
        });
#endif
    }

    private static void OpenSaveData(string fileName, Action<SavedGameRequestStatus, ISavedGameMetadata> onDataOpen)
    {
        if (!IsAuthenticated)
        {
            onDataOpen(SavedGameRequestStatus.AuthenticationError, null);
        }
        else
        {
            savedGameClient.OpenWithAutomaticConflictResolution(fileName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                onDataOpen);
        }
    }

    public static void ShowLeaderboard()
    {
        try
        {
            Social.Active.ShowLeaderboardUI();
            AndroidFeatures.MakeToast("Auth: " + IsAuthenticated);
        }
        catch (Exception e)
        {
            AndroidFeatures.MakeToast(LangManager.GetString("show_leadboard_error") 
                + ": " + e.Message, 1);
        }
    }

    public static void AddValueToLeaderboard(long value)
    {
        if (IsAuthenticated)
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.ReportScore(value, LEADERBOARD, (success) => 
            {
                if (success)
                    AndroidFeatures.MakeToast("Leaderboard success!");
                else
                    AndroidFeatures.MakeToast("Leaderboard failure!");
            });
            //Social.ReportScore(value, LEADERBOARD, (bool success) =>
            //{
            //    if (success)
            //        AndroidFeatures.MakeToast("Leaderboard success!");
            //    else
            //        AndroidFeatures.MakeToast("Leaderboard failure!");
            //});
#endif
        }
    }

    public static void ReadSaveData(string fileName, Action<SavedGameRequestStatus, byte[]> onDataRead)
    {
        if (!IsAuthenticated)
        {
            if (File.Exists(cachePath))
            {
                byte[] encrypted = File.ReadAllBytes(cachePath);
                onDataRead(SavedGameRequestStatus.AuthenticationError, Decrypt(encrypted));
            }
            else
            {
                onDataRead(SavedGameRequestStatus.AuthenticationError, null);
            }
        }
        else
        {
            OpenSaveData(fileName, (status, metadata) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    currentMetadata = metadata;

                    //If cache exists.
                    if (File.Exists(cachePath))
                    {
                        void DataRead(SavedGameRequestStatus stat, byte[] data)
                        {
                            //Get cached.
                            byte[] encrypted = File.ReadAllBytes(cachePath);
                            byte[] cachedData = Decrypt(encrypted);
                            SavesPack cached = (SavesPack)Serializer
                            .Deserialize(Encoding.UTF8.GetString(cachedData), typeof(SavesPack));

                            //Get common.
                            SavesPack common = (SavesPack)Serializer
                            .Deserialize(Encoding.UTF8.GetString(data), typeof(SavesPack));

                            //If cached has more bigger gameplay time, use cached saves.
                            if (cached.gameplayTime > common.gameplayTime)
                            {
                                WriteSaveData(cachedData);
                                onDataRead(SavedGameRequestStatus.Success, cachedData);
                            }
                            //Else, use common data.
                            else
                            {
                                onDataRead(stat, data);
                            }
                        }
                        //Read common data.
                        savedGameClient.ReadBinaryData(metadata, DataRead);
                    }
                    //If cache not exists.
                    else
                    {
                        savedGameClient.ReadBinaryData(metadata, onDataRead);
                    }
                }
                startDateTime = DateTime.Now;
            });
        }
    }

    public static void WriteSaveData(byte[] data)
    {
        TimeSpan currentSpan = DateTime.Now - startDateTime;
#if UNITY_ANDROID
        void OnDataWrite()
        {
            TimeSpan totalPlayTime = currentMetadata.TotalTimePlayed + currentSpan;

            SavedGameMetadataUpdate updatedMetadata = new SavedGameMetadataUpdate.Builder().Build();

            ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(currentMetadata,
                updatedMetadata,
                data,
                (status, metadata) => currentMetadata = metadata);

            startDateTime = DateTime.Now;
        }
#endif
        if (currentMetadata == null)
        {
            OpenSaveData(DEFAULT_SAVE_NAME, (status, metadata) =>
            {
                currentMetadata = metadata;
                //If can write normally.
                if (status == SavedGameRequestStatus.Success)
                {
#if UNITY_ANDROID
                    OnDataWrite();
#endif
                }
                //Else write to cache.
                else
                {
                    File.WriteAllBytes(cachePath, Encrypt(data));
                }
            });
        }
        else
        {
#if UNITY_ANDROID
            OnDataWrite();
#endif
        }
    }

    private static byte[] Encrypt(byte[] data)
    {
        Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        SHA384 hash = SHA384.Create();
        //Unique hash - is hash of device unique identifier.
        byte[] uniqueHash = hash.ComputeHash(SystemInfo.deviceUniqueIdentifier.HexToBytes());

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
        aes.Padding = PaddingMode.PKCS7;
        SHA384 hash = SHA384.Create();

        //Unique hash - is hash of device unique identifier.
        byte[] uniqueHash = hash.ComputeHash(SystemInfo.deviceUniqueIdentifier.HexToBytes());

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
            aes.Padding = PaddingMode.PKCS7;
            SHA384 hash = SHA384.Create();
            //Unique hash - is hash of device unique identifier.
            byte[] uniqueHash = hash.ComputeHash(id.HexToBytes());

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
}
