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
    private List<PCComponent> equipComponents = new List<PCComponent>();
    private ComponentType selectedType;
    private int indexOfSelected;
    public GameObject CPUSlot, motherboardSlot;
    public GameObject slotPrefab;

    public Transform GPUParent, RAMParent, equipParent;
    public Text equipText;

    [HideInInspector]
    public CPU mainCPU;
    [HideInInspector]
    public Motherboard mainMotherboard;
    [HideInInspector]
    public readonly List<GPU> GPUs = new List<GPU>();
    [HideInInspector]
    public readonly List<RAM> RAMs = new List<RAM>();
    public Sprite emptyPixel;

    public GameObject infoObject;
    public CanvasGroup infoGroup;
    public Text infoText, sellText, unequipText, removeText;
    public Button sellButton, unequipButton, removeButton;
    public ButtonAnimation sellButtonAnimation, unequipButtonAnimation, removeButtonAnimation;

    public Text errorsText;
    public GameObject wallpaper, pages;
    [Space]
    public CaseScroller caseScroller;
    public Inventory inventory;
    public MoneySystem moneySystem;
    public NavigationScript navigation;
    public OSScript osscript;

    /// <summary>
    /// Text of the computer monitor with errors (RichText required).
    /// </summary>
    public void FindErrors()
    {
        string errors = null;
        //Check of availability of CPU and messaging about it.
        if (mainCPU == null)
            errors += "No CPU!\n";
        //Check of availability of motherboard and messaging about it.
        if (mainMotherboard == null)
            errors += "No motherboard!\n";

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
            errors += "No graphics!\n";

        //Does computer contains RAM?
        bool containsRAMs = false;
        for (int i = 0; i < RAMs.Count; i++)
        {
            containsRAMs = containsRAMs || RAMs[i] != null;
        }
        if (!containsRAMs)
            //Message about unavailability of RAM.
            errors += "No RAM!\n";

        //If no errors, show default monitor.
        if (errors == null)
        {
            wallpaper.SetActive(true);
            pages.SetActive(true);
            errorsText.gameObject.SetActive(false);
        }
        else
        {
            wallpaper.SetActive(false);
            pages.SetActive(false);
            errorsText.gameObject.SetActive(true);
            errorsText.text = errors;
        }
    }
    private void Start()
    {
        UpdateComputer();
        CPUSlot.transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.CPU, -1); });
        motherboardSlot.transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.Motherboard, -1); });

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
            for (int i = 0; i < mainMotherboard.busVersions.Length; i++)
            {
                GPUSlots.Add(Instantiate(slotPrefab, GPUParent));
                if (i < GPUs.Count && GPUs[i] != null)
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
                selectedType = ComponentType.GPU;
                GPUSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                GPUSlots[i].GetComponent<Button>().onClick.AddListener(delegate { GPUClicked(index); });
                GPUSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
                GPUSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.GPU, index); });
                GPUParent.transform.position = new Vector2(GPUParent.transform.position.x, GPUParent.localScale.y);
            }
            //Fill in list of GPUs.
            for (int i = GPUs.Count; i < mainMotherboard.busVersions.Length; i++)
            {
                GPUs.Add(null);
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
                    equipSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(index); });
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
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
                    equipSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(index); });
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
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
                    equipSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(eventIndex); });
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
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
                    equipSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    equipSlots[i].GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(eventIndex); });
                    equipSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
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
                    if (type == ComponentType.Motherboard)
                    {
                        //compHasComponents is "Does computer has components except for motherboard".
                        bool compHasComponents = false;
                        //Check for CPU.
                        compHasComponents |= mainCPU != null;
                        //Check for GPUs.
                        for (int i = 0; i < GPUs.Count; i++)
                            compHasComponents |= GPUs[i] != null;
                        //Check for RAMs.
                        for (int i = 0; i < RAMs.Count; i++)
                            compHasComponents |= RAMs[i] != null;

                        if (compHasComponents)
                        {
                            unequipText.text = "Dissasemble computer first!";
                            unequipButton.interactable = false;
                            unequipButtonAnimation.disabled = true;
                        }
                        else
                        {
                            unequipText.text = "Unequip";
                            unequipButton.interactable = true;
                            unequipButtonAnimation.disabled = false;
                        }
                    }
                    else
                    {
                        unequipText.text = "Unequip";
                        unequipButton.interactable = true;
                        unequipButtonAnimation.disabled = false;
                    }
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
            StartCoroutine(InfoInAnimation());
        }
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
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(delegate { Sell(type, index); });
        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(delegate { Remove(type, index); });
        unequipButton.onClick.RemoveAllListeners();
        unequipButton.onClick.AddListener(delegate { Unequip(type, index); });
    }

    private IEnumerator InfoInAnimation()
    {
        //Duration: 0.5 s.
        float time = 0F;
        while (time < 0.25F)
        {
            time += Time.deltaTime;
            infoGroup.alpha = time * 4F;
            yield return null;
        }
    }

    private IEnumerator InfoOutAnimation()
    {
        // Duration: 0.5 s.
        float time = 0.25F;
        while (time > 0F)
        {
            time -= Time.deltaTime;
            infoGroup.alpha = time * 4F;
            yield return null;
        }
        infoObject.SetActive(false);
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
                inventory.components.Add(mainCPU.Clone());
                mainCPU = null;
                CPUClicked();
                Back();
                break;
            case ComponentType.Motherboard:
                inventory.components.Add(mainMotherboard.Clone());
                mainMotherboard = null;
                MotherboardClicked();
                Back();
                break;
            case ComponentType.GPU:
                inventory.components.Add(GPUs[index].Clone());
                GPUs[index] = null;
                GPUClicked(index);
                Back();
                break;
            case ComponentType.RAM:
                inventory.components.Add(RAMs[index].Clone());
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
        StartCoroutine(InfoOutAnimation());
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
                if (RAMs.Count > indexOfSelected && RAMs[indexOfSelected] != null)
                    inventory.components.Add(RAMs[indexOfSelected]);
                RAMs[indexOfSelected] = (RAM)equipComponents[index];
                RAMClicked(indexOfSelected);
                break;
            case ComponentType.GPU:
                if (GPUs.Count > index && GPUs[index] != null)
                    inventory.components.Add(GPUs[indexOfSelected]);
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
        FindErrors();
        //monitorText.text = MonitorText;
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
