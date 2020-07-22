using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using KlimSoft;
using System.IO;
using GooglePlayGames.BasicApi.SavedGame;

public class LoadScript : MonoBehaviour
{
    public Image bar;
    public Text headerText, statusText;
    public CanvasGroup group;

    public static string errorInfo = null;

    private float progress;

    private void Start()
    {
        Application.targetFrameRate = 60;
        headerText.text = "ComputerCase " + Application.version;
        GPGSManager.Initialize(true);
        StartCoroutine(MainCorut());
    }

    private void FixedUpdate()
    {
        if (bar.fillAmount < progress)
            bar.fillAmount += 0.05F;
    }

    private IEnumerator MainCorut()
    {
        //Wait a bit.
        yield return new WaitForSeconds(0.25F);
        statusText.text = "Authenticating...";

        //Auth.
        bool authFinished = false, authSuccess = false;
        void OnAuth(bool success)
        {
            authFinished = true;
            authSuccess = success;
        }
        GPGSManager.Auth(OnAuth);
        //Wait until auth finished.
        while (!authFinished)
            yield return null;

        if (!authSuccess)
        {
            AndroidFeatures.MakeToast("Authentication failed! Try restart the game.", 1);
            try
            {
                GameSaver.LoadAsync(null);
            }
            catch (Exception e)
            {
                errorInfo = $"Message: {e.Message}\nStack trace: {e.StackTrace}";
            }
        }
        else
        {
            progress = 0.1F;
            statusText.text = "Reading saves...";

            SavedGameRequestStatus readStatus = SavedGameRequestStatus.TimeoutError;
            void OnRead(SavedGameRequestStatus status, byte[] data)
            {
                switch (readStatus)
                {
                    case SavedGameRequestStatus.AuthenticationError:
                        AndroidFeatures.MakeToast("Authentication error!", 1);
                        break;
                    case SavedGameRequestStatus.BadInputError:
                        AndroidFeatures.MakeToast("Bad input error!", 1);
                        break;
                    case SavedGameRequestStatus.InternalError:
                        AndroidFeatures.MakeToast("Internal error!", 1);
                        break;
                    default:
                        //Ignore TimeoutError because it keep working with it.
                        try
                        {
                            GameSaver.LoadAsync(data);
                        }
                        catch (Exception e)
                        {
                            errorInfo = $"Message: {e.Message}\nStack trace: {e.StackTrace}";
                        }
                        break;
                }
                readStatus = status;
                progress = 0.25F;
            }
            GPGSManager.ReadSaveData(GPGSManager.DEFAULT_SAVE_NAME, OnRead);
        }


        while (GameSaver.loadProgress < 1F)
        {
            progress = GameSaver.loadProgress / 4F + 0.25F;
            yield return null;
        }

        //Load scene.
        statusText.text = "Loading game...";
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while (!sceneLoad.isDone)
        {
            progress = 0.5F + sceneLoad.progress / 2F;
            yield return null;
        }
        progress = 1F;
        //Animation of fade.
        for (float t = 0F; t < 1F; t += Time.deltaTime)
        {
            group.alpha = 1F - t;
            yield return null;
        }
        //Unload this scene.
        SceneManager.UnloadSceneAsync(0);

        /*
        while (GameSaver.loadProgress < 0.75F)
        {
            progress = GameSaver.loadProgress / 2F;
            statusText.text = GameSaver.loadStatus;
            yield return null;
        }
        //Load scene.
        statusText.text = "Loading game...";
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while (!sceneLoad.isDone)
        {
            progress = 0.5F + sceneLoad.progress / 2F;
            yield return null;
        }
        progress = 1F;
        //Animation of fade.
        for (float t = 0F; t < 1F; t += Time.deltaTime)
        {
            group.alpha = 1F - t;
            yield return null;
        }
        //Unload this scene.
        SceneManager.UnloadSceneAsync(0);
        */
    }
}