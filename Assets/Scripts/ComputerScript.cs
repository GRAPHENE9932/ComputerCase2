using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.PlayerLoop;

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
    public MoneySystem moneySystem;
    public NavigationScript navigation;

    public Sprite emptyPixel;

    public GameObject infoObject;
    public Text infoText, sellText, unequipText, removeText;
    public Button sellButton, unequipButton, removeButton;
    public ButtonAnimation sellButtonAnimation, unequipButtonAnimation, removeButtonAnimation;

    public Text monitorText;

    /// <summary>
    /// Text of the computer monitor with errors and recommendations (RichText required).
    /// </summary>
    public string MonitorText
    {
        get
        {
            string errors = null;
            //Check of availability of CPU and messaging about it.
            if (mainCPU == null)
                errors += "<color=red>No CPU!</color>\n";
            //Check of availability of motherboard and messaging about it.
            if (mainMotherboard == null)
                errors += "<color=red>No motherboard!</color>\n";

            //Does computer contains graphics?
            bool containsGPUs = false;
            for (int i = 0; i < GPUs.Count; i++)
            {
                containsGPUs = containsGPUs || GPUs[i] != null;
            }
            //If CPU contains graphics, containsGPUs = true.
            if (mainCPU != null)
                containsGPUs |= mainCPU.integratedGraphics;
            if (!containsGPUs)
                //Message about unavailability of graphics.
                errors += "<color=red>No graphics!</color>\n";

            //Does computer contains RAM?
            bool containsRAMs = false;
            for (int i = 0; i < RAMs.Count; i++)
            {
                containsRAMs = containsRAMs || RAMs[i] != null;
            }
            if (!containsRAMs)
                //Message about unavailability of RAM.
                errors += "<color=red>No RAM!</color>\n";

            //If no errors, show default monitor.
            if (errors == null)
            {
                //Result string.
                string result = null;
                //CPU part.
                result += $"CPU: {mainCPU.shortName};\n";
                //Getting not null GPUs.
                GPU[] realGPUs = GPUs.Where(x => x != null).ToArray();
                //If count of GPUs == 1, write "GPU: ...".
                if (realGPUs.Length == 1)
                {
                    result += $"GPU: {realGPUs[0]};\n";
                }
                //If cout of GPUs > 1, write list of GPUs: 
                //"GPUs:
                //GPU 0: ...;
                //GPU 1: ...;
                else if (realGPUs.Length > 1)
                {
                    result += "GPUs:\n";
                    for (int i = 0; i < realGPUs.Length; i++)
                    {
                        result += $"GPU {i}: {realGPUs[i].fullName};\n";
                    }
                }
                //But if no GPUs, do not write anything about it.
                //Search  for min frequency of RAM.
                int minFrequency = int.MaxValue;
                for (int i = 0; i < RAMs.Count; i++)
                {
                    if (RAMs[i] != null && RAMs[i].frequency < minFrequency)
                        minFrequency = RAMs[i].frequency;
                }
                //Writing RAM`s title: "RAM (DDR4, 2666 MHz):"
                result += $"RAM (DDR{(mainMotherboard.RAMType == 1 ? null : mainMotherboard.RAMType.ToString())}, {minFrequency} MHz):\n";
                for (int i = 0; i < mainMotherboard.RAMCount; i++)
                {
                    if (RAMs[i] != null)
                        result += $"Slot {i}: {RAMs[i].memory} MB;\n";
                    else
                        result += $"Slot {i}: - ;\n";
                }
                //Writing chipset.
                result += $"Motherboard chipset: {mainMotherboard.chipset.ToString().Replace("_", "")}";

                //Add recomendations.
                string recommendations = null;

                //Check for enabled multiple channel mode of RAM.
                if (CheckMultipleChannels() && RAMs.Count(x => x != null) >= mainMotherboard.RAMCount / 2)
                    recommendations += $"<color=green>You using single-channel mode of RAM, but can use {mainMotherboard.RAMCount / 2}-channel mode, just replace your RAM planks with \"Zebra-style\". It contributes to improvement of RAM performance.\n</color>";

                return result + recommendations;
            }
            else
            {
                return errors;
            }
        }
    }
    private void Start()
    {
        UpdateComputer();
    }
    /// <summary>
    /// Check the using of multiple RAM channels.
    /// </summary>
    /// <returns>
    /// True - using multiple channels, false - not using.
    /// </returns>
    public bool CheckMultipleChannels()
    {
        //usedDouble - is slots with indexes 0, 2, 4, 6, ... used? True - used, false - unused, null - different.
        //usedNotDouble - is slots with indexes 1, 3, 5, 7, ... used? True - used, false - unused, null - different.
        bool? usedDouble = RAMs[0] != null, usedNotDouble = RAMs[1] != null;
        for (int i = 2; i < mainMotherboard.RAMCount; i += 2)
        {
            if (usedDouble != (RAMs[i] != null))
                usedDouble = null;
        }
        for (int i = 3; i < mainMotherboard.RAMCount; i += 2)
        {
            if (usedNotDouble != (RAMs[i] != null))
                usedNotDouble = null;
        }
        return usedDouble == null && usedNotDouble == null;
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
        CPUSlot.transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.CPU, -1); });

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
        motherboardSlot.transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.Motherboard, -1); });

        //GPUs and RAMs updating.
        if (mainMotherboard != null)
        {
            //GPUs updating.
            for (int i = 0; i < GPUSlots.Count; i++)
            {
                Destroy(GPUSlots[i]);
            }
            GPUSlots.Clear();
            for (int i = 0; i < mainMotherboard.busVersions.Length; i++)
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
                GPUSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.GPU, index); });
                GPUParent.transform.position = new Vector2(GPUParent.transform.position.x, GPUParent.localScale.y);
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
                if (i < RAMs.Count && RAMs[i] != null)
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
                RAMSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.RAM, index); });
                RAMParent.transform.position = new Vector2(RAMParent.transform.position.x, RAMParent.localScale.y);
            }
            //Fill in list of RAMs.
            for (int i = RAMs.Count; i < mainMotherboard.RAMCount; i++)
            {
                RAMs.Add(null);
            }
        }
        else
        {
            //Clear GPU and RAM slots because no motherboard.
            for (int i = 0; i < GPUSlots.Count; i++)
            {
                Destroy(GPUSlots[i]);
            }
            GPUSlots.Clear();
            for (int i = 0; i < RAMSlots.Count; i++)
            {
                Destroy(RAMSlots[i]);
            }
            RAMSlots.Clear();
        }
    }

    public void CPUClicked()
    {
        if (mainMotherboard != null)
        {
            equipComponents = inventory.components.Where(x => x is CPU CPU && CPU.socket == mainMotherboard.socket).ToList();
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
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.All, index); });
                    equipText.text = null;
                }
            }
            else
            {
                equipText.text = "No equippable CPU!";
            }
        }
        else
        {
            //Set message "No motherboard" and clear equip components.
            equipText.text = "No motherboard!";
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();
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
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.All, index); });
                    equipText.text = null;
                }
            }
            else
            {
                equipText.text = "No motherboards!";
            }
        }
        else
        {
            //Set message "Dissasemble computer first!" and clear equip components.
            equipText.text = "No motherboard!";
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();
        }
    }

    public void RAMClicked(int index)
    {
        if (mainMotherboard != null)
        {
            equipComponents = inventory.components.Where(x => x is RAM RAM && RAM.type == mainMotherboard.RAMType).ToList();
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
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.All, index); });
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
            //Set message "No motherboard" and clear equip components.
            equipText.text = "No motherboard!";
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();
        }
    }

    public void GPUClicked(int index)
    {
        if (mainMotherboard != null)
        {
            equipComponents = inventory.components.Where(x => x is GPU GPU && GPU.busVersion == mainMotherboard.busVersions[index] && GPU.busMultiplier == mainMotherboard.busMultipliers[index]).ToList();
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
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.All, index); });
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
            //Set message "No motherboard" and clear equip components.
            equipText.text = "No motherboard!";
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
            {
                Destroy(equipSlots[i]);
            }
            equipSlots.Clear();
        }
    }

    public void InfoClicked(ComponentType type, int index)
    {
        void InfoOpen(PCComponent component)
        {
            if (component != null)
            {
                infoText.text = component.Properties + "\nTime: " + component.time.ToString("dd.MM.yyyy hh:mm:ss");

                sellText.text = "Sell (+" + component.price / 20 + "$)";
                sellButton.interactable = true;
                sellButtonAnimation.disabled = false;

                if (type == ComponentType.All)
                {
                    unequipText.text = null;
                    unequipButton.interactable = false;
                    unequipButtonAnimation.disabled = true;
                }
                else
                {
                    unequipText.text = "Unequip";
                    unequipButton.interactable = true;
                    unequipButtonAnimation.disabled = false;
                }

                removeText.text = "Remove";
                removeButton.interactable = true;
                removeButtonAnimation.disabled = false;
            }
            else
            {
                switch (type)
                {
                    case ComponentType.CPU:
                        infoText.text = "No processor!\n" + (mainMotherboard == null ? null : ("Socket: " + mainMotherboard.socket));
                        break;
                    case ComponentType.Motherboard:
                        infoText.text = "No motherboard!";
                        break;
                    case ComponentType.GPU:
                        infoText.text = "No GPU!\n" + mainMotherboard == null ? null : $"Interface: PCIe {mainMotherboard.busVersions[index]}.0 x{mainMotherboard.busMultipliers[index]}.";
                        break;
                    case ComponentType.RAM:
                        infoText.text = "No RAM!\n" + mainMotherboard == null ? null : "Type: DDR" + (mainMotherboard.RAMType == 1 ? null : mainMotherboard.RAMType.ToString()) + ".";
                        break;
                }
                sellText.text = null;
                sellButton.interactable = false;
                sellButtonAnimation.disabled = true;

                unequipText.text = null;
                unequipButton.interactable = false;
                unequipButtonAnimation.disabled = true;

                removeText.text = null;
                removeButton.interactable = false;
                removeButtonAnimation.disabled = true;
            }
            
            infoObject.SetActive(true);
        }
        infoObject.SetActive(true);
        switch (type)
        {
            case ComponentType.CPU:
                InfoOpen(mainCPU);
                break;
            case ComponentType.Motherboard:
                InfoOpen(mainMotherboard);
                break;
            case ComponentType.GPU:
                if (GPUs.Count > index)
                    InfoOpen(GPUs[index]);
                else
                    InfoOpen(null);
                break;
            case ComponentType.RAM:
                if (RAMs.Count > index)
                    InfoOpen(RAMs[index]);
                else
                    InfoOpen(null);
                break;
            case ComponentType.All:
                InfoOpen(equipComponents[index]);
                break;
        }
        sellButton.onClick.AddListener(delegate { Sell(type, index); });
        removeButton.onClick.AddListener(delegate { Remove(type, index); });
        unequipButton.onClick.AddListener(delegate { Unequip(type, index); });
    }

    public void Sell(ComponentType type, int index)
    {
        switch (type)
        {
            case ComponentType.CPU:
                if (mainMotherboard != null)
                {
                    moneySystem.Money += mainCPU.price / 20;
                    mainCPU = null;
                    CPUClicked();
                    Back();
                }
                break;
            case ComponentType.Motherboard:
                if (mainMotherboard != null)
                {
                    moneySystem.Money += mainMotherboard.price / 20;
                    mainMotherboard = null;
                    MotherboardClicked();
                    Back();
                }
                break;
            case ComponentType.GPU:
                if (GPUs.Count > index && GPUs[index] != null)
                {
                    moneySystem.Money += GPUs[index].price / 20;
                    GPUs[index] = null;
                    GPUClicked(index);
                    Back();
                }
                break;
            case ComponentType.RAM:
                if (RAMs.Count > index && RAMs[index] != null)
                {
                    moneySystem.Money += RAMs[index].price / 20;
                    RAMs[index] = null;
                    RAMClicked(index);
                    Back();
                }
                break;
            case ComponentType.All:
                moneySystem.Money += equipComponents[index].price / 20;
                inventory.components.Remove(equipComponents[index]);
                switch (selectedType)
                {
                    case ComponentType.CPU:
                        CPUClicked();
                        break;
                    case ComponentType.Motherboard:
                        MotherboardClicked();
                        break;
                    case ComponentType.GPU:
                        GPUClicked(index);
                        break;
                    case ComponentType.RAM:
                        RAMClicked(index);
                        break;
                    default:
                        throw new System.Exception("Error! Code 7.");
                }
                Back();
                break;
        }
        UpdateComputer();
    }

    public void Remove(ComponentType type, int index)
    {
        switch (type)
        {
            case ComponentType.CPU:
                mainCPU = null;
                CPUClicked();
                Back();
                break;
            case ComponentType.Motherboard:
                mainMotherboard = null;
                MotherboardClicked();
                Back();
                break;
            case ComponentType.GPU:
                GPUs[index] = null;
                GPUClicked(index);
                Back();
                break;
            case ComponentType.RAM:
                RAMs[index] = null;
                RAMClicked(index);
                Back();
                break;
            case ComponentType.All:
                inventory.components.Remove(equipComponents[index]);
                switch (selectedType)
                {
                    case ComponentType.CPU:
                        CPUClicked();
                        break;
                    case ComponentType.Motherboard:
                        MotherboardClicked();
                        break;
                    case ComponentType.GPU:
                        GPUClicked(index);
                        break;
                    case ComponentType.RAM:
                        RAMClicked(index);
                        break;
                    default:
                        throw new System.Exception("Error! Code 8.");
                }
                Back();
                break;
        }
        UpdateComputer();
    }

    public void Unequip(ComponentType type, int index)
    {
        switch (type)
        {
            case ComponentType.CPU:
                inventory.components.Add(mainCPU);
                mainCPU = null;
                CPUClicked();
                Back();
                break;
            case ComponentType.Motherboard:
                inventory.components.Add(mainMotherboard);
                mainMotherboard = null;
                MotherboardClicked();
                Back();
                break;
            case ComponentType.GPU:
                inventory.components.Add(GPUs[index]);
                GPUs[index] = null;
                GPUClicked(index);
                Back();
                break;
            case ComponentType.RAM:
                inventory.components.Add(RAMs[index]);
                RAMs[index] = null;
                RAMClicked(index);
                Back();
                break;
            case ComponentType.All:
                throw new System.Exception("Error! Code 9.");
        }
        UpdateComputer();
    }

    public void Back()
    {
        infoObject.SetActive(false);
    }

    public void EquipClicked(int index)
    {
        inventory.components.Remove(equipComponents[index]);
        switch (selectedType)
        {
            case ComponentType.CPU:
                if (mainCPU != null)
                    inventory.components.Add(mainCPU);
                mainCPU = (CPU)equipComponents[index];
                CPUClicked();
                break;
            case ComponentType.Motherboard:
                if (mainMotherboard != null)
                    inventory.components.Add(mainMotherboard);
                mainMotherboard = (Motherboard)equipComponents[index];
                MotherboardClicked();
                break;
            case ComponentType.RAM:
                if (RAMs[index] != null)
                    inventory.components.Add(RAMs[index]);
                if (RAMs.Count <= index)
                    RAMs.Add((RAM)equipComponents[index]);
                else
                    RAMs[indexOfSelected] = (RAM)equipComponents[index];
                RAMClicked(indexOfSelected);
                break;
            case ComponentType.GPU:
                if (GPUs[index] != null)
                    inventory.components.Add(GPUs[index]);
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
    /// <summary>
    /// Click on monitor event.
    /// </summary>
    public void MonitorClicked()
    {
        monitorText.text = MonitorText;
        navigation.MenuItemClicked(9);
    }
    /// <summary>
    /// Click on "Go back" button on monitor page.
    /// </summary>
    public void MonitorBack()
    {
        navigation.MenuItemClicked(2);
    }
}
