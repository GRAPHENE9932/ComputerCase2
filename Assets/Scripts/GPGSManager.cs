using System;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

public static class GPGSManager
{
    public const string DEFAULT_SAVE_NAME = "MainSave";

    private static ISavedGameClient savedGameClient;
    private static ISavedGameMetadata currentMetadata;

    private static DateTime startDateTime;

    public static bool IsAuthenticated
    {
        get
        {
            if (PlayGamesPlatform.Instance != null)
                return PlayGamesPlatform.Instance.IsAuthenticated();
            return false;
        }
    }

    public static void Initialize(bool debug)
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = debug;
        PlayGamesPlatform.Activate();
    }

    public static void Auth(Action<bool, string> onAuth)
    {
        Social.localUser.Authenticate((success, callback) =>
        {
            if (success)
                savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            onAuth(success, callback);
        });
    }
    public static void Auth(Action<bool> onAuth)
    {
        Social.localUser.Authenticate((success) =>
        {
            if (success)
                savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            onAuth(success);
        });
    }

    private static void OpenSaveData(string fileName, Action<SavedGameRequestStatus, ISavedGameMetadata> onDataOpen)
    {
        if (!IsAuthenticated)
        {
            onDataOpen(SavedGameRequestStatus.AuthenticationError, null);
            return;
        }
        savedGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            onDataOpen);
    }

    public static void ReadSaveData(string fileName, Action<SavedGameRequestStatus, byte[]> onDataRead)
    {
        if (!IsAuthenticated)
        {
            onDataRead(SavedGameRequestStatus.AuthenticationError, null);
            return;
        }
        OpenSaveData(fileName, (status, metadata) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                savedGameClient.ReadBinaryData(metadata, onDataRead);
                currentMetadata = metadata;
            }
            startDateTime = DateTime.Now;
        });
    }

    public static void WriteSaveData(byte[] data)
    {
        TimeSpan currentSpan = DateTime.Now - startDateTime;
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
        if (currentMetadata == null)
        {
            OpenSaveData(DEFAULT_SAVE_NAME, (status, metadata) =>
            {
                Debug.Log("Cloud data write status: " + status.ToString());
                if (status == SavedGameRequestStatus.Success)
                {
                    currentMetadata = metadata;
                    OnDataWrite();
                }
            });
        }
        else
        {
            OnDataWrite();
        }
    }
}
