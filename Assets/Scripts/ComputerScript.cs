using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ComputerScript : MonoBehaviour
{
    private List<GameObject> GPUSlots, RAMSlots;

    public GameObject CPUSlot, motherboardSlot;
    public GameObject slotPrefab;

    public Transform GPUParent, RAMParent;

    public CPU mainCPU;
    public Motherboard mainMotherboard;
    public List<GPU> GPUs;
    public List<RAM> RAMs;

    public CaseScroller caseScroller;
    public Sprite emptyPixel;

    /// <summary>
    /// Updating slots of computer (CPU, motherboard, GPUs and RAMs).
    /// </summary>
    public void UpdateComputer()
    {
        //CPU updating.
        if (mainCPU != null)
        {
            //Set image.
            CPUSlot.transform.Find("Image").GetComponent<Image>().sprite = mainCPU.image;
            //Set rarity line color.
            CPUSlot.transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)mainCPU.rarity];
            //Set text of name.
            CPUSlot.transform.Find("Name").GetComponentInChildren<Text>().text = mainCPU.shortName;

            //Set text of socket.
            if (mainMotherboard != null)
                CPUSlot.transform.Find("Socket").GetComponentInChildren<Text>().text = mainMotherboard.socket;
            else
                CPUSlot.transform.Find("Socket").GetComponentInChildren<Text>().text = "No socket!";
        }
        else
        {
            //Set empty pixel instead of image.
            CPUSlot.transform.Find("Image").GetComponent<Image>().sprite = emptyPixel;
            //Set white rarity line.
            CPUSlot.transform.Find("RarityLine").GetComponent<Image>().color = Color.white;
            //Set name of processor "No processor!".
            CPUSlot.transform.Find("Name").GetComponentInChildren<Text>().text = "No processor!";

            //Set text of socket.
            if (mainMotherboard != null)
                CPUSlot.transform.Find("Socket").GetComponentInChildren<Text>().text = mainMotherboard.socket;
            else
                CPUSlot.transform.Find("Socket").GetComponentInChildren<Text>().text = "No socket!";
        }

        //Motherboard updating.
        if (mainMotherboard != null)
        {
            //Set image.
            motherboardSlot.transform.Find("Image").GetComponent<Image>().sprite = mainMotherboard.image;
            //Set rarity line color.
            motherboardSlot.transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)mainMotherboard.rarity];
            //Set text of name.
            motherboardSlot.transform.Find("Name").GetComponentInChildren<Text>().text = mainMotherboard.shortName;
        }
        else
        {
            //Set empty pixel instead of image.
            motherboardSlot.transform.Find("Image").GetComponent<Image>().sprite = emptyPixel;
            //Set white rarity line.
            motherboardSlot.transform.Find("RarityLine").GetComponent<Image>().color = Color.white;
            //Set name of motherboard "No motherboard!".
            motherboardSlot.transform.Find("Name").GetComponentInChildren<Text>().text = "No motherboard!";
        }

        //GPUs and RAMs updating.
        if (mainMotherboard != null)
        {
            //GPUs updating.
            //Add slots, if it not enough.
            for (int i = 0; i < mainMotherboard.GPUInterfaces.Length; i++)
            {
                if (GPUSlots.Count - 1 < i)
                {
                    GPUSlots.Add(Instantiate(slotPrefab, GPUParent));
                }
            }
            //Remove slots, if is too much.
            for (int i = mainMotherboard.GPUInterfaces.Length - 1; i < 0; i++)
            {
                if (GPUSlots.Count - 1 > i)
                {
                    GPUSlots.RemoveAt(i);
                }
            }
            //Setting slots.
            for (int i = 0; i < mainMotherboard.GPUInterfaces.Length; i++)
            {
                if (GPUs[i] != null)
                {
                    //Set image.
                    GPUSlots[i].transform.Find("Image").GetComponent<Image>().sprite = GPUs[i].image;
                    //Set rarity line color.
                    GPUSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)GPUs[i].rarity];
                    //Set text of name.
                    GPUSlots[i].transform.Find("Name").GetComponent<Text>().text = GPUs[i].shortName;
                }
                else
                    Debug.Log("Err");
            }
        }
    }
}
