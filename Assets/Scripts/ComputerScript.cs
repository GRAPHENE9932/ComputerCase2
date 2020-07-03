using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.PlayerLoop;
using System.Data;

public class ComputerScript : MonoBehaviour
{
    /// <summary>
    /// List of GPU slots (not components, game objects).
    /// </summary>
    private readonly List<GameObject> GPUSlots = new List<GameObject>();
    /// <summary>
    /// List of RAM slots (not components, game objects).
    /// </summary>
    private readonly List<GameObject> RAMSlots = new List<GameObject>();
    /// <summary>
    /// List of equip slots (not components, game objects).
    /// </summary>
    private readonly List<GameObject> equipSlots = new List<GameObject>();
    /// <summary>
    /// List of components to equip.
    /// </summary>
    private List<PCComponent> equipComponents = new List<PCComponent>();
    /// <summary>
    /// Selected component type.
    /// </summary>
    private ComponentType selectedType;
    /// <summary>
    /// Index of selected component.
    /// </summary>
    private int indexOfSelected;
    /// <summary>
    /// Slot of CPU or motherboard.
    /// </summary>
    public GameObject CPUSlot, motherboardSlot;
    /// <summary>
    /// The example of slot.
    /// </summary>
    public GameObject slotPrefab;

    /// <summary>
    /// Parent of GPUs, RAMs or equipComponents.
    /// </summary>
    public Transform GPUParent, RAMParent, equipParent;
    /// <summary>
    /// Text of equip slots group. "No motherboard", for example.
    /// </summary>
    public Text equipText;

    [HideInInspector]
    public static CPU mainCPU;
    [HideInInspector]
    public static Motherboard mainMotherboard;
    [HideInInspector]
    public static List<GPU> GPUs = new List<GPU>();
    [HideInInspector]
    public static List<RAM> RAMs = new List<RAM>();
    public Sprite emptyPixel;

    /// <summary>
    /// Object of info window.
    /// </summary>
    public GameObject infoObject;
    /// <summary>
    /// Canvas group of info window.
    /// </summary>
    public CanvasGroup infoGroup;
    /// <summary>
    /// Button text of info window.
    /// </summary>
    public Text infoText, sellText, unequipText;
    public SVGImage removeImage;
    /// <summary>
    /// Button of info window.
    /// </summary>
    public Button sellButton, unequipButton, removeButton;
    /// <summary>
    /// Button animation of button of info window.
    /// </summary>
    public ButtonAnimation sellButtonAnimation, unequipButtonAnimation, removeButtonAnimation;

    /// <summary>
    /// Errors text of monitor.
    /// </summary>
    public Text errorsText;
    /// <summary>
    /// Wallpaper or pages group of monitor
    /// </summary>
    public GameObject wallpaper, pages;
    /// <summary>
    /// Case scroller script.
    /// </summary>
    [Space]
    public CaseScroller caseScroller;
    /// <summary>
    /// Inventory script.
    /// </summary>
    public Inventory inventory;
    /// <summary>
    /// Money system script.
    /// </summary>
    public MoneySystem moneySystem;
    /// <summary>
    /// Navigation script.
    /// </summary>
    public NavigationScript navigation;
    /// <summary>
    /// OS script (monitor).
    /// </summary>
    public OSScript osscript;
    public SoundManager soundManager;
    public AudioSource mainSource;
    public AudioClip[] equipClips;

    /// <summary>
    /// Does computer contains components except for motherboard?
    /// </summary>
    public bool ComputerHasComponents
    {
        get
        {
            //compHasComponents is "Does computer has components except for motherboard?".
            bool compHasComponents = false;
            //Check for CPU.
            compHasComponents |= mainCPU != null;
            //Check for GPUs.
            for (int i = 0; i < GPUs.Count; i++)
                compHasComponents |= GPUs[i] != null;
            //Check for RAMs.
            for (int i = 0; i < RAMs.Count; i++)
                compHasComponents |= RAMs[i] != null;

            return compHasComponents;
        }
    }

    private void Awake()
    {
        ApplySaves();
    }
    private void Start()
    {
        //AddListeners of info buttons of CPU slot and motherboard slot.
        CPUSlot.transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.CPU, -1); });
        motherboardSlot.transform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(delegate { InfoClicked(ComponentType.Motherboard, -1); });
        //Update computer at start.
        UpdateComputer();
    }
    public static void ApplySaves()
    {
        mainCPU = GameSaver.savesPack.cpu;
        if (mainCPU != null)
            mainCPU.RegenerateImage();

        mainMotherboard = GameSaver.savesPack.motherboard;
        if (mainMotherboard != null)
            mainMotherboard.RegenerateImage();

        GPUs = GameSaver.savesPack.gpus.ToList();
        foreach (GPU gpu in GPUs)
            if (gpu != null)
                gpu.RegenerateImage();

        RAMs = GameSaver.savesPack.rams.ToList();
        foreach (RAM ram in RAMs)
            if (ram != null)
                ram.RegenerateImage();
    }

    /// <summary>
    /// Text of the computer monitor with errors (RichText required).
    /// </summary>
    public void FindErrors()
    {
        string errors = null;
        //Check of availability of CPU and messaging about it.
        if (mainCPU == null)
            errors += $"{LangManager.GetString("no_cpu")}\n";
        //Check of availability of motherboard and messaging about it.
        if (mainMotherboard == null)
            errors += $"{LangManager.GetString("no_motherboard")}\n";

        //Does computer contains graphics?
        bool containsGPUs = false;
        for (int i = 0; i < GPUs.Count; i++)
            containsGPUs = containsGPUs || GPUs[i] != null;
        //If CPU contains graphics, containsGPUs = true.
        if (mainCPU != null)
            containsGPUs |= mainCPU.integratedGraphics;
        if (!containsGPUs)
            //Message about unavailability of graphics.
            errors += $"{LangManager.GetString("no_graphics")}\n";

        //Does computer contains RAM?
        bool containsRAMs = false;
        for (int i = 0; i < RAMs.Count; i++)
            containsRAMs = containsRAMs || RAMs[i] != null;
        if (!containsRAMs)
            //Message about unavailability of RAM.
            errors += $"{LangManager.GetString("no_ram")}\n";

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
                CPUSlot.transform.Find("Socket").GetComponentInChildren<Text>().text = LangManager.GetString("no_socket");
        }
        else
        {
            //Set empty pixel instead of image.
            CPUSlot.transform.Find("Image").GetComponent<Image>().sprite = emptyPixel;
            //Set white rarity line.
            CPUSlot.transform.Find("RarityLine").GetComponent<Image>().color = Color.white;
            //Set name of processor "No processor!".
            CPUSlot.transform.Find("Name").GetComponentInChildren<Text>().text = LangManager.GetString("no_cpu");

            //Set text of socket.
            if (mainMotherboard != null)
                CPUSlot.transform.Find("Socket").GetComponentInChildren<Text>().text = mainMotherboard.socket;
            else
                CPUSlot.transform.Find("Socket").GetComponentInChildren<Text>().text = LangManager.GetString("no_socket");
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
            motherboardSlot.transform.Find("Name").GetComponentInChildren<Text>().text = LangManager.GetString("no_motherboard");
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
                    GPUSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = LangManager.GetString("empty");
                }
                //Set button events.
                int index = i;
                //selectedType = ComponentType.GPU;
                GPUSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                //GPU clicked event.
                GPUSlots[i].GetComponent<Button>().onClick.AddListener(delegate { GPUClicked(index); });
                //Sound play event.
                GPUSlots[i].GetComponent<Button>().onClick.AddListener(delegate { soundManager.PlayRandomButton(); });
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
                    RAMSlots[i].transform.Find("Name").GetComponentInChildren<Text>().text = LangManager.GetString("empty");
                }
                //Set button event.
                int index = i;
                //selectedType = ComponentType.RAM;
                RAMSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                //RAM clicked event.
                RAMSlots[i].GetComponent<Button>().onClick.AddListener(delegate { RAMClicked(index); });
                //Play sound event.
                RAMSlots[i].GetComponent<Button>().onClick.AddListener(delegate { soundManager.PlayRandomButton(); });
                RAMSlots[i].transform.Find("InfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
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
    /// <summary>
    /// Event of CPU slot click.
    /// </summary>
    public void CPUClicked()
    {
        //If computer contains motherboard.
        if (mainMotherboard != null)
        {
            //Set equip components.
            equipComponents = Inventory.components.Where(x => x is CPU CPU && CPU.socket == mainMotherboard.socket).ToList();
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
                equipText.text = LangManager.GetString("no_equippable_cpu");
            }
        }
        else
        {
            //Set message "No motherboard" and clear equip components.
            equipText.text = LangManager.GetString("no_motherboard");
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            for (int i = 0; i < equipSlots.Count; i++)
                Destroy(equipSlots[i]);
            equipSlots.Clear();
        }
    }

    public void MotherboardClicked()
    {
        //If no components on computer.
        if (!ComputerHasComponents)
        {
            equipComponents = Inventory.components.Where(x => x is Motherboard).ToList();
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
            //Set message "No motherboard!" and clear equip components.
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
            equipComponents = Inventory.components.Where(x => x is RAM RAM && RAM.type == mainMotherboard.RAMType).ToList();
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
                equipText.text = LangManager.GetString("no_equippable_ram");
            }
        }
        else
        {
            //Set message "No motherboard" and clear equip components.
            equipText.text = LangManager.GetString("no_motherboard");
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
            equipComponents = Inventory.components.Where(x => x is GPU GPU && GPU.busVersion == mainMotherboard.busVersions[index] && GPU.busMultiplier == mainMotherboard.busMultipliers[index]).ToList();
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
                equipText.text = LangManager.GetString("no_equippable_gpu");
            }
        }
        else
        {
            //Set message "No motherboard" and clear equip components.
            equipText.text = LangManager.GetString("no_motherboard");
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
    /// <summary>
    /// Event of clicked info button.
    /// </summary>
    /// <param name="type">Type of component (All - it is equip slot).</param>
    /// <param name="index">Index of component.</param>
    public void InfoClicked(ComponentType type, int index)
    {
        void InfoOpen(PCComponent component)
        {
            if (component != null)
            {
                //Write full properties with time in info text.
                infoText.text = component.FullProperties + $"\n{LangManager.GetString("time:")} " + component.time.ToString("dd.MM.yyyy HH:mm:ss");

                //Set text of sell button (real price / 20).
                sellText.text = $"{LangManager.GetString("sell")} (+{component.price / 20}$)";
                //Set sell button interactable.
                sellButton.interactable = true;
                sellButtonAnimation.disabled = false;

                //If it info of equip slot, disable unequip button.
                if (type == ComponentType.All)
                {
                    unequipText.text = null;
                    unequipButton.interactable = false;
                    unequipButtonAnimation.disabled = true;
                }
                else
                {
                    //If component == motherboard, and if computer has components, except for motherboard, disable unequip button and set text of it "Disassemble computer first!".
                    if (type == ComponentType.Motherboard)
                    {
                        if (ComputerHasComponents)
                        {
                            unequipText.text = LangManager.GetString("dis_comp_first");
                            unequipButton.interactable = false;
                            unequipButtonAnimation.disabled = true;

                            //Disable sell button.
                            sellButton.interactable = false;
                            sellButtonAnimation.disabled = true;
                            sellText.text = null;

                            //Disable remove button.
                            removeButton.interactable = false;
                            removeButtonAnimation.disabled = true;
                            removeImage.enabled = false;
                        }
                        else
                        {
                            unequipText.text = LangManager.GetString("unequip");
                            unequipButton.interactable = true;
                            unequipButtonAnimation.disabled = false;

                            sellButton.interactable = true;
                            sellButtonAnimation.disabled = false;
                            sellText.text = $"{LangManager.GetString("sell")} (+{component.price / 20}$)";

                            removeButton.interactable = true;
                            removeButtonAnimation.disabled = false;
                            removeImage.enabled = true;
                        }
                    }
                    else
                    {
                        unequipText.text = LangManager.GetString("unequip");
                        unequipButton.interactable = true;
                        unequipButtonAnimation.disabled = false;

                        //Enable sell button.
                        removeButton.interactable = true;
                        removeButtonAnimation.disabled = false;
                        removeImage.enabled = true;

                        //Enable remove button.
                        removeButton.interactable = true;
                        removeButtonAnimation.disabled = false;
                        removeImage.enabled = true;
                    }
                }
            }
            else
            {
                //If component == null.
                switch (type)
                {
                    case ComponentType.CPU:
                        if (mainMotherboard == null)
                            infoText.text = string.Format("{0}\n{1}", LangManager.GetString("no_cpu"), LangManager.GetString("no_socket"));
                        else
                            infoText.text = string.Format("{0}\n{1} {2}", LangManager.GetString("no_cpu"), LangManager.GetString("socket:"), mainMotherboard.socket);
                        break;
                    case ComponentType.Motherboard:
                        infoText.text = LangManager.GetString("no_motherboard");
                        break;
                    case ComponentType.GPU:
                        infoText.text = string.Format("{0}\n{1} PCIe {2}.0 x{3}.",
                            LangManager.GetString("no_gpu"), LangManager.GetString("interface:"), mainMotherboard.busVersions[index], mainMotherboard.busMultipliers[index]);
                        break;
                    case ComponentType.RAM:
                        infoText.text = string.Format("{0} DDR{1}.", 
                            LangManager.GetString("gen:"), mainMotherboard.RAMType == 1 ? null : mainMotherboard.RAMType.ToString());
                        break;
                }
                //Disable sell button.
                sellText.text = null;
                sellButton.interactable = false;
                sellButtonAnimation.disabled = true;

                //Disable unequip button.
                unequipText.text = null;
                unequipButton.interactable = false;
                unequipButtonAnimation.disabled = true;

                //Disable remove button.
                removeImage.enabled = false;
                removeButton.interactable = false;
                removeButtonAnimation.disabled = true;
            }
            
            //Enable info window.
            infoObject.SetActive(true);
            //Start InfoInAnimation.
            StartCoroutine(InfoInAnimation());
        }
        //Open info by component.
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
        //Add listener for sell button.
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(delegate { Sell(type, index); });
        //Add listener for remove button.
        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(delegate { Remove(type, index); });
        //Add listener for remove button.
        unequipButton.onClick.RemoveAllListeners();
        unequipButton.onClick.AddListener(delegate { Unequip(type, index); });
    }
    /// <summary>
    /// Animation of info window opening.
    /// </summary>
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
    /// <summary>
    /// Animation of info window closing.
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// Sell button clicked.
    /// </summary>
    /// <param name="type">Type of component.</param>
    /// <param name="index">Index of component.</param>
    public void Sell(ComponentType type, int index)
    {
        switch (type)
        {
            case ComponentType.CPU:
                if (mainMotherboard != null)
                {
                    //Add money.
                    MoneySystem.Money += mainCPU.price / 20;
                    //Play sound.
                    if (mainCPU.price / 20 > 0)
                        soundManager.PlayRandomSell();
                    //Delete main CPU.
                    mainCPU = null;
                    CPUClicked();
                    Back();
                }
                break;
            case ComponentType.Motherboard:
                if (mainMotherboard != null)
                {
                    //Add money.
                    MoneySystem.Money += mainMotherboard.price / 20;
                    //Play sound.
                    if (mainMotherboard.price / 20 > 0)
                        soundManager.PlayRandomSell();
                    //Delete main motherboard.
                    mainMotherboard = null;
                    MotherboardClicked();
                    Back();
                }
                break;
            case ComponentType.GPU:
                if (GPUs.Count > index && GPUs[index] != null)
                {
                    //Add money.
                    MoneySystem.Money += GPUs[index].price / 20;
                    //Play sound.
                    if (GPUs[index].price / 20 > 0)
                        soundManager.PlayRandomSell();
                    //Delete GPU[index].
                    GPUs[index] = null;
                    GPUClicked(index);
                    Back();
                }
                break;
            case ComponentType.RAM:
                if (RAMs.Count > index && RAMs[index] != null)
                {
                    //Add money.
                    MoneySystem.Money += RAMs[index].price / 20;
                    //Play sound.
                    if (RAMs[index].price / 20 > 0)
                        soundManager.PlayRandomSell();
                    //Delete RAM[index].
                    RAMs[index] = null;
                    RAMClicked(index);
                    Back();
                }
                break;
            case ComponentType.All:
                //Add money.
                MoneySystem.Money += equipComponents[index].price / 20;
                //Play sound.
                if (equipComponents[index].price / 20 > 0)
                    soundManager.PlayRandomSell();
                //Remove component.
                Inventory.components.Remove(equipComponents[index]);
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
    /// <summary>
    /// Remove button clicked.
    /// </summary>
    /// <param name="type">Type of component.</param>
    /// <param name="index">Index of component.</param>
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
                Inventory.components.Remove(equipComponents[index]);
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
    /// <summary>
    /// Unequip button clicked.
    /// </summary>
    /// <param name="type">Type of component.</param>
    /// <param name="index">Index of component.</param>
    public void Unequip(ComponentType type, int index)
    {
        //Play sound.
        mainSource.PlayOneShot(equipClips[Random.Range(0, equipClips.Length)]);
        switch (type)
        {
            case ComponentType.CPU:
                Inventory.components.Add((PCComponent)mainCPU.Clone());
                mainCPU = null;
                CPUClicked();
                Back();
                break;
            case ComponentType.Motherboard:
                Inventory.components.Add((PCComponent)mainMotherboard.Clone());
                mainMotherboard = null;
                MotherboardClicked();
                Back();
                break;
            case ComponentType.GPU:
                Inventory.components.Add((PCComponent)GPUs[index].Clone());
                GPUs[index] = null;
                GPUClicked(index);
                Back();
                break;
            case ComponentType.RAM:
                Inventory.components.Add((PCComponent)RAMs[index].Clone());
                RAMs[index] = null;
                RAMClicked(index);
                Back();
                break;
            case ComponentType.All:
                throw new System.Exception("Error! Code 9.");
        }
        UpdateComputer();
    }
    /// <summary>
    /// Back button clicked.
    /// </summary>
    public void Back()
    {
        StartCoroutine(InfoOutAnimation());
    }
    /// <summary>
    /// Equip slot clicked.
    /// </summary>
    /// <param name="index">Index of clicked slot.</param>
    public void EquipClicked(int index)
    {
        //Play sound.
        mainSource.PlayOneShot(equipClips[Random.Range(0, equipClips.Length)]);

        //Remove this component from inventory.
        Inventory.components.Remove(equipComponents[index]);

        //If component != null, add it to invertory.
        //Then, set new component to computer.
        switch (selectedType)
        {
            case ComponentType.CPU:
                if (mainCPU != null)
                    Inventory.components.Add(mainCPU);
                mainCPU = (CPU)equipComponents[index];
                CPUClicked();
                break;
            case ComponentType.Motherboard:
                if (mainMotherboard != null)
                    Inventory.components.Add(mainMotherboard);
                mainMotherboard = (Motherboard)equipComponents[index];
                MotherboardClicked();
                break;
            case ComponentType.RAM:
                if (RAMs.Count > indexOfSelected && RAMs[indexOfSelected] != null)
                    Inventory.components.Add(RAMs[indexOfSelected]);
                RAMs[indexOfSelected] = (RAM)equipComponents[index];
                RAMClicked(indexOfSelected);
                break;
            case ComponentType.GPU:
                if (GPUs.Count > index && GPUs[index] != null)
                    Inventory.components.Add(GPUs[indexOfSelected]);
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
