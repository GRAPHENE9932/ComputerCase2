using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadScript : MonoBehaviour
{
    public Image bar;
    public Text headerText, statusText;
    public CanvasGroup group;

    private float progress;

    private void Start()
    {
        headerText.text = "ComputerCase " + Application.version;
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
        //Load saves.
        GameSaver.LoadAsync();
        //Log.
        GameSaver.timeLogs.Add(new SavesPack.TimeLog(true));
        if (GameSaver.timeLogs.Count > 500)
            GameSaver.timeLogs.RemoveAt(0);

        
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
    }
}
