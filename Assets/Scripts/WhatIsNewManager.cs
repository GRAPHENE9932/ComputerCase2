using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class WhatIsNewManager : MonoBehaviour
{
    public GameObject window;
    public Text text;
    private void Start()
    {
        if (GameSaver.Saves.version != Application.version)
        {
            string xmlData = Resources.Load<TextAsset>(
                Path.Combine("What is new", Application.version)
                ).text;

            XmlSerializer serializer = new XmlSerializer(typeof(WhatIsNewContainer));
            WhatIsNewContainer container;
            using (TextReader reader = new StringReader(xmlData))
            {
                container = (WhatIsNewContainer)serializer.Deserialize(reader);
            }

            string toShow = LangManager.GetString("what_is_new:") + "\n";
            foreach (WhatIsNewContainer.WhatIsNew w in container.news)
            {
                toShow += $"► {(w.bold ? "<b>" : null)} ";
                switch (LangManager.lang)
                {
                    case "ENG":
                        toShow += w.ENG;
                        break;
                    case "RUS":
                        toShow += w.RUS;
                        break;
                    case "UKR":
                        toShow += w.UKR;
                        break;
                }
                toShow += $"{(w.bold ? "</b>" : null)}\n";
            }

            window.SetActive(true);
            text.text = toShow;
        }
    }

    public void Close()
    {
        StartCoroutine(CloseCorutine());
    }

    private IEnumerator CloseCorutine()
    {
        CanvasGroup windowGroup = window.GetComponent<CanvasGroup>();
        float alpha = 1F;
        while (alpha > 0F)
        {
            alpha -= Time.deltaTime * 3;
            windowGroup.alpha = alpha;
            yield return null;
        }
        window.SetActive(false);
    }

    /// <summary>
    /// Container with "what is new" arrays.
    /// </summary>
    [Serializable]
    public class WhatIsNewContainer
    {
        /// <summary>
        /// What is new.
        /// </summary>
        public WhatIsNew[] news;

        [Serializable]
        public class WhatIsNew
        {
            public string ENG, RUS, UKR;
            [XmlAttribute]
            public bool bold;

            public WhatIsNew() { }
        }
    }
}
