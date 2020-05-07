using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ComputerScript : MonoBehaviour
{
    private readonly List<GameObject> GPUSlots = new List<GameObject>();
    private readonly List<GameObject> RAMSlots = new List<GameObject>();
    private readonly List<GameObject> equipSlots = new List<GameObject>();
    private List<PCComponent> equipComponents;
    private ComponentType selectedType;
    private int indexOfSelected;
    public GameObject CPUSlot, motherboardSlot;
    public GameObject slotPrefab;

    public Transform GPUParent, RAMParent, equipParent;
    public Text equipText;

    private CPU mainCPU;
    private Motherboard mainMotherboard;
    private readonly List<GPU> GPUs = new List<GPU>();
    private readonly List<RAM> RAMs = new List<RAM>();

    public CaseScroller caseScroller;
    public Inventory inventory;

    public Sprite emptyPixel;

    private void Start()
    {
        UpdateComputer();
    }

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
            for (int i = 0; i < GPUSlots.Count; i++)
            {
                Destroy(GPUSlots[i]);
            }
            GPUSlots.Clear();
            for (int i = 0; i < mainMotherboard.GPUInterfaces.Length; i++)
            {
                GPUSlots.Add(Instantiate(slotPrefab, GPUParent));
                if (i < GPUs.Count)
                {
                    //Set image.
                    GPUSlots[i].transform.Find("Image").GetComponent<Image>().sprite = GPUs[i].image;
                    //Set rarity line color.
                    GPUSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)GPUs[i].rarity];
                    //Set text of name.
                    GPUSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = GPUs[i].shortName;
                }
                else
                {
                    //Set empty pixel instead of image.
                    GPUSlots[i].transform.Find("Image").GetComponent<Image>().sprite = emptyPixel;
                    //Set white rarity line.
                    GPUSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = Color.white;
                    //Set name of videocard "No videocard!".
                    GPUSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = "Empty";
                }
                //Set button event.
                int index = i;
                indexOfSelected = -1;
                selectedType = ComponentType.GPU;
                GPUSlots[i].GetComponent<Button>().onClick.AddListener(delegate { GPUClicked(index); });
            }

            //RAM updating.
            for (int i = 0; i < RAMSlots.Count; i++)
            {
                Destroy(RAMSlots[i]);
            }
            RAMSlots.Clear();
            for (int i = 0; i < mainMotherboard.RAMCount; i++)
            {
                RAMSlots.Add(Instantiate(slotPrefab, RAMParent));
                if (i < RAMs.Count)
                {
                    //Set image.
                    RAMSlots[i].transform.Find("Image").GetComponent<Image>().sprite = RAMs[i].image;
                    //Set rarity line color.
                    RAMSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)RAMs[i].rarity];
                    //Set text of name.
                    RAMSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = RAMs[i].shortName;
                }
                else
                {
                    //Set empty pixel instead of image.
                    RAMSlots[i].transform.Find("Image").GetComponent<Image>().sprite = emptyPixel;
                    //Set white rarity line.
                    RAMSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = Color.white;
                    //Set name of videocard "No videocard!".
                    RAMSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = "Empty";
                }
                //Set button event.
                int index = i;
                indexOfSelected = -1;
                selectedType = ComponentType.RAM;
                RAMSlots[i].GetComponent<Button>().onClick.AddListener(delegate { RAMClicked(index); });
            }
        }
    }

    public void CPUClicked()
    {
        if (mainMotherboard != null)
        {
            equipComponents = inventory.components.Where(x => x is CPU && ((CPU)x).socket == mainMotherboard.socket).ToList();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();

            if (equipComponents.Count != 0)
            {
                for (int i = 0; i < equipComponents.Count; i++)
                {
                    equipSlots.Add(Instantiate(slotPrefab, equipParent));
                    //Set image.
                    equipSlots[i].transform.Find("Image").GetComponent<Image>().sprite = equipComponents[i].image;
                    //Set rarity line color.
                    equipSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)equipComponents[i].rarity];
                    //Set text of name.
                    equipSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = equipComponents[i].shortName;
                    //Set button event.
                    int index = i;
                    indexOfSelected = -1;
                    selectedType = ComponentType.CPU;
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(index); });
                }
            }
            else
            {
                equipText.text = "No equippable CPU!";
            }
        }
        else
        {
            equipText.text = "No motherboard!";
        }
    }

    public void MotherboardClicked()
    {
        if (mainCPU == null && RAMs.Count == 0 && GPUs.Count == 0)
        {
            equipComponents = inventory.components.Where(x => x is Motherboard).ToList();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();

            if (equipComponents.Count != 0)
            {
                for (int i = 0; i < equipComponents.Count; i++)
                {
                    equipSlots.Add(Instantiate(slotPrefab, equipParent));
                    //Set image.
                    equipSlots[i].transform.Find("Image").GetComponent<Image>().sprite = equipComponents[i].image;
                    //Set rarity line color.
                    equipSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)equipComponents[i].rarity];
                    //Set text of name.
                    equipSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = equipComponents[i].shortName;
                    //Set button event.
                    int index = i;
                    indexOfSelected = -1;
                    selectedType = ComponentType.Motherboard;
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(index); });
                }
            }
            else
            {
                equipText.text = "No motherboards!";
            }
        }
        else
        {
            equipText.text = "Dissasemble computer first!";
        }
    }

    public void RAMClicked(int index)
    {
        if (mainMotherboard != null)
        {
            equipComponents = inventory.components.Where(x => x is RAM && ((RAM)x).type == mainMotherboard.RAMType).ToList();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();

            if (equipComponents.Count != 0)
            {
                for (int i = 0; i < equipComponents.Count; i++)
                {
                    equipSlots.Add(Instantiate(slotPrefab, equipParent));
                    //Set image.
                    equipSlots[i].transform.Find("Image").GetComponent<Image>().sprite = equipComponents[i].image;
                    //Set rarity line color.
                    equipSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)equipComponents[i].rarity];
                    //Set text of name.
                    equipSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = equipComponents[i].shortName;
                    //Set button event.
                    int eventIndex = i;
                    indexOfSelected = index;
                    selectedType = ComponentType.RAM;
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(eventIndex); });
                }
                equipText.text = null;
            }
            else
            {
                equipText.text = "No equippable RAM!";
            }
        }
        else
        {
            equipText.text = "No motherboard!";
        }
    }

    public void GPUClicked(int index)
    {
        if (mainMotherboard != null)
        {
            equipComponents = inventory.components.Where(x => x is GPU && ((GPU)x).motherboardInterface == mainMotherboard.GPUInterfaces[index]).ToList();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();

            if (equipComponents.Count != 0)
            {
                for (int i = 0; i < equipComponents.Count; i++)
                {
                    equipSlots.Add(Instantiate(slotPrefab, equipParent));
                    //Set image.
                    equipSlots[i].transform.Find("Image").GetComponent<Image>().sprite = equipComponents[i].image;
                    //Set rarity line color.
                    equipSlots[i].transform.Find("RarityLine").GetComponent<Image>().color = caseScroller.rarityColors[(int)equipComponents[i].rarity];
                    //Set text of name.
                    equipSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = equipComponents[i].shortName;
                    //Set button event.
                    int eventIndex = i;
                    indexOfSelected = index;
                    selectedType = ComponentType.GPU;
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(eventIndex); });
                }
                equipText.text = null;
            }
            else
            {
                equipText.text = "No equippable GPUs!";
            }
        }
        else
        {
            equipText.text = "No motherboard!";
        }
    }

    public void EquipClicked(int index)
    {
        inventory.components.Remove(equipComponents[index]);
        switch (selectedType)
        {
            case ComponentType.CPU:
                mainCPU = (CPU)equipComponents[index];
                CPUClicked();
                break;
            case ComponentType.Motherboard:
                mainMotherboard = (Motherboard)equipComponents[index];
                MotherboardClicked();
                break;
            case ComponentType.RAM:
                if (RAMs.Count <= indexOfSelected)
                    RAMs.Add((RAM)equipComponents[index]);
                else
                    RAMs[indexOfSelected] = (RAM)equipComponents[index];
                RAMClicked(indexOfSelected);
                break;
            case ComponentType.GPU:
                if (GPUs.Count <= indexOfSelected)
                    GPUs.Add((GPU)equipComponents[index]);
                else
                    GPUs[indexOfSelected] = (GPU)equipComponents[index];
                GPUClicked(indexOfSelected);
                break;
            default:
                throw new System.Exception("Error! Code 6.");
        }
        UpdateComputer();
    }
}
