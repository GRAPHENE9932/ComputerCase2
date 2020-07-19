using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrashMinigame : MonoBehaviour
{
    public GameObject clickToStartImage;
    public Text clickToStartLabel;
    public RectTransform[] squares;
    public InputField betField;
    public Text mainText;

    public Text resultText;
    public CanvasGroup downPanelGroup;

    public MessageBoxManager messageBox;
    public SoundManager soundMgr;

    public AudioClip winSound, loseSound;

    private long bet = -1;
    /// <summary>
    /// Is value increasing now?
    /// </summary>
    private bool isPlaying = false;
    /// <summary>
    /// Is money already collected?
    /// </summary>
    private bool moneyCollected = false;
    /// <summary>
    /// Does player stopped game to collect money?
    /// </summary>
    private bool stoppedByPlayer = false;
    /// <summary>
    /// Does player stopped game to restart it?
    /// </summary>
    private bool totallyStopedByPlayer = false;
    /// <summary>
    /// Is game need to be restarted?
    /// </summary>
    private bool needBeRestarted = false;

    public void Restart()
    {
        if (isPlaying)
            totallyStopedByPlayer = true;
        else
        {
            needBeRestarted = false;
            StartCoroutine(ShowBottomPanel(false));
            mainText.text = "x1.00";
            clickToStartImage.SetActive(true);
            clickToStartLabel.gameObject.SetActive(true);
        }
    }

    public void Start_Clicked()
    {
        if (isPlaying)
            stoppedByPlayer = true;
        else if (!needBeRestarted)
        {
            if (bet <= 0 || bet > MoneySystem.Money.Value)
                messageBox.StartMessage(LangManager.GetString("invalid_bet"), 1);
            else
            {
                isPlaying = true;
                clickToStartLabel.text = LangManager.GetString("click_to_stop");
                StartCoroutine(MainCorut());
            }
        }
    }

    private IEnumerator MainCorut()
    {
        //Set text of clickToStartLabel.
        clickToStartLabel.text = LangManager.GetString("click_to_stop");
        //Block bet field.
        betField.interactable = false;
        //Block navigation.
        NavigationScript.blocked = true;
        //Decrease money by bet.
        MoneySystem.Money -= bet;
        float crashValue;
        float rand = Random.Range(0F, 1F);
        if (rand < 0.6F)
            crashValue = Random.Range(1F, 2F);
        else if (rand < 0.90F)
            crashValue = Random.Range(2F, 5F);
        else if (rand < 0.99F)
            crashValue = Random.Range(5F, 20F);
        else
            crashValue = Random.Range(20F, 200F);

        float finishValue = 1F;
        float increaseMultiplier = 0.01F;
        float increaseMultiplier2 = 0.0005F;
        moneyCollected = false;
        stoppedByPlayer = false;
        while (isPlaying)
        {
            finishValue += Time.deltaTime * increaseMultiplier;
            increaseMultiplier += increaseMultiplier2 * Time.deltaTime;
            increaseMultiplier2 += 0.00005F;
            yield return null;
            mainText.text = string.Format("x{0:0.00}", finishValue);

            //Animate squares.
            for (int i = 0; i < squares.Length; i++)
                squares[i].anchoredPosition = new Vector2(Random.Range(-finishValue, finishValue),
                    Random.Range(-finishValue, finishValue));

            if (finishValue >= crashValue)
            {
                isPlaying = false;

                if (!moneyCollected)
                    Lose();

                needBeRestarted = true;

                clickToStartImage.SetActive(false);
                clickToStartLabel.gameObject.SetActive(false);

                break;
            }
            if (!moneyCollected && stoppedByPlayer)
            {
                Win(finishValue);

                stoppedByPlayer = false;
                needBeRestarted = true;

                clickToStartImage.SetActive(false);
                clickToStartLabel.gameObject.SetActive(false);
            }
            if (totallyStopedByPlayer)
            {
                isPlaying = false;

                Restart();

                totallyStopedByPlayer = false;
                needBeRestarted = false;
            }
        }
        StartCoroutine(SquaresToNormal(finishValue));
        //Unblock navigation.
        NavigationScript.blocked = false;
        //Unblock bet field.
        betField.interactable = true;
        BetField_EndEdit();
        //Set text of clickToStartLabel.
        clickToStartLabel.text = LangManager.GetString("click_to_start");
    }

    /// <summary>
    /// Animates returning squares to normal position.
    /// </summary>
    /// <param name="current">Current vibrating value.</param>
    private IEnumerator SquaresToNormal(float current)
    {
        for (float time = 0F; time < 0.5F; time += Time.deltaTime)
        {
            for (int i = 0; i < squares.Length; i++)
                squares[i].anchoredPosition = new Vector2(
                    Random.Range(Mathf.Lerp(-current, 0, time * 2), 
                    Mathf.Lerp(current, 0, time * 2)),
                    Random.Range(Mathf.Lerp(-current, 0, time * 2),
                    Mathf.Lerp(current, 0, time * 2)));
            yield return null;
        }
        //Set squares to normal position.
        for (int i = 0; i < squares.Length; i++)
            squares[i].anchoredPosition = new Vector2(0, 0);
    }

    private void Lose()
    {
        resultText.color = Color.red;
        resultText.text = $"-{bet}$";
        //Statistics.
        StatisticsScript.moneyLostInMinigames += (ulong)bet;
        soundMgr.PlaySound(loseSound);
        StartCoroutine(ShowBottomPanel());
    }

    private void Win(float multiplier)
    {
        long won = (long)System.Math.Round(bet * multiplier - bet);

        resultText.color = Color.green;
        resultText.text = $"+{won}$";
        //Increase money.
        MoneySystem.Money += bet + won;
        //Statistics.
        StatisticsScript.moneyWonInMinigames += (ulong)won;
        soundMgr.PlaySound(winSound);
        StartCoroutine(ShowBottomPanel());
        moneyCollected = true;

        //If player won, give him ad with chance 30%.
        float adRand = Random.Range(0F, 1F);
        if (adRand <= 0.3F)
            AdManager.ShowInterstitial();
    }

    /// <summary>
    /// Shows or hides bottom panel by 500 ms.
    /// </summary>
    /// <param name="show">Show (true) or hide (false).</param>
    private IEnumerator ShowBottomPanel(bool show = true)
    {
        float time = downPanelGroup.alpha;
        downPanelGroup.interactable = show;
        if (show)
            while (time < 1F)
            {
                time += Time.deltaTime * 2;
                downPanelGroup.alpha = time;
                yield return null;
            }
        else
            while (time > 0F)
            {
                time -= Time.deltaTime * 2;
                downPanelGroup.alpha = time;
                yield return null;
            }
    }

    public void BetField_EndEdit()
    {
        if (long.TryParse(betField.text, out bet))
        {
            if (bet > MoneySystem.Money.Value)
                bet = MoneySystem.Money.Value;
            else if (bet < 0)
                bet = 0;

            betField.text = bet.ToString();
        }
        else
            bet = -1;
    }

    public void Max_Clicked()
    {
        bet = MoneySystem.Money.Value;
        betField.text = bet.ToString();
    }
}
