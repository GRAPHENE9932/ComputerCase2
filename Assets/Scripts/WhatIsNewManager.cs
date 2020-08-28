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
            TextAsset asset = Resources.Load<TextAsset>(
                Path.Combine("What is new", Application.version)
                );

            if (asset == null)
                return;

            string xmlData = asset.text;

            XmlSerializer serializer = new XmlSerializer(typeof(WhatIsNewContainer));
            WhatIsNewContainer container;
            using (TextReader reader = new StringReader(xmlData))
            {
                container = (WhatIsNewContainer)serializer.Deserialize(reader);
            }

            //What is new.
            string toShow = "<i>" + LangManager.GetString("what_is_new:") + "</i>\n";
            foreach (WhatIsNewContainer.WhatIsNew n in container.news)
            {
                toShow += $"► {(n.bold ? "<b>" : null)} ";
                switch (LangManager.lang)
                {
                    case "ENG":
                        toShow += n.ENG;
                        break;
                    case "RUS":
                        toShow += n.RUS;
                        break;
                    case "UKR":
                        toShow += n.UKR;
                        break;
                }
                toShow += $"{(n.bold ? "</b>" : null)}\n";
            }

            //In next update.
            if (container.planned != null && container.planned.Length != 0)
            {
                toShow += "\n<i>" + LangManager.GetString("planned_next_upd:") + "</i>\n";
                foreach (WhatIsNewContainer.WhatIsNew f in container.planned)
                {
                    toShow += $"► {(f.bold ? "<b>" : null)} ";
                    switch (LangManager.lang)
                    {
                        case "ENG":
                            toShow += f.ENG;
                            break;
                        case "RUS":
                            toShow += f.RUS;
                            break;
                        case "UKR":
                            toShow += f.UKR;
                            break;
                    }
                    toShow += $"{(f.bold ? "</b>" : null)}\n";
                }
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
        public WhatIsNew[] planned;

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
