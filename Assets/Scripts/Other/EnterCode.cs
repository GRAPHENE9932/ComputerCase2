using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class EnterCode : MonoBehaviour
{
    [Header("UI elements")]
    public Text successActText;
    public Image successActBG;
    public CanvasGroup successActGroup;
    public InputField field;

    public static List<byte[]> usedHashes = new List<byte[]>();

    private Coroutine textEnable;

    public static void ApplySaves()
    {
        if (GameSaver.Saves.usedCodes != null)
            usedHashes = GameSaver.Saves.usedCodes.ToList();
    }

    public void Activate()
    {
        string resource = Resources.Load<TextAsset>("Codes").text.Replace("\r", "");
        string[] codes = resource.Split(new string[] { "\n\n" },
            StringSplitOptions.None);
        byte[][] hashes = codes.Select(
            x => Convert.FromBase64String(x.Split('\n')[0])
            ).ToArray();

        string code = field.text;
        SHA256 hasher = SHA256.Create();
        byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(code));

        bool found = false;
        for (int i = 0; i < hashes.Length; i++)
        {
            if (Enumerable.SequenceEqual(hashes[i], hash) &&
                !Contains(ref usedHashes, ref hash))
            {
                string[] codeProps = codes[i].Split('\n');

                //Set text
                successActText.text = codeProps[1] + "\n+" + codeProps[2] + '$';
                successActBG.color = Color.green;
                //Enable text
                if (textEnable != null)
                    StopCoroutine(textEnable);
                textEnable = StartCoroutine(TextEnable());

                //Add money
                MoneySystem.Money += int.Parse(codeProps[2]);

                //Add to used hashes
                usedHashes.Add(hash);

                found = true;
            }
        }

        if (!found)
        {
            //Set text
            successActText.text = LangManager.GetString("incorrect_code");
            successActBG.color = Color.red;

            //Enable text
            if (textEnable != null)
                StopCoroutine(textEnable);
            textEnable = StartCoroutine(TextEnable());
        }
    }

    private IEnumerator TextEnable()
    {
        while (successActGroup.alpha < 1)
        {
            successActGroup.alpha += Time.deltaTime * 2;
            yield return null;
        }

        yield return new WaitForSeconds(5);

        while (successActGroup.alpha > 0)
        {
            successActGroup.alpha -= Time.deltaTime * 4;
            yield return null;
        }
    }

    private bool Contains(ref List<byte[]> list, ref byte[] bytes)
    {
        for (int i = 0; i < list.Count; i++)
            if (Enumerable.SequenceEqual(list[i], bytes))
                return true;

        return false;
    }
}