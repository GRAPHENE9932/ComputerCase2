using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ComputerScript : MonoBehaviour
{
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
    /// The example of slot.
    /// </summary>
    public GameObject slotPrefab, equipSlotPrefab;
    public Transform equipParent;

    /// <summary>
    /// Parent of GPUs, RAMs or equipComponents.
    /// </summary>
    public Transform componentsParent;
    /// <summary>
    /// Text of equip slots group. "No motherboard", for example.
    /// </summary>
    public Text equipText;
    /// <summary>
    /// Image to view selected computer component.
    /// </summary>
    public Image componentImage;
    /// <summary>
    /// Parent GameObject of image to view selected computer component.
    /// </summary>
    public GameObject componentImageObj;
    /// <summary>
    /// GameObject of main info button.
    /// </summary>
    public Button infoButton;

    public static CPU mainCPU;
    public static Motherboard mainMotherboard;
    public static List<GPU> GPUs = new List<GPU>();
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
    public AudioClip[] equipClips;
    public AudioClip removeClip;

    public Sprite emptyCPU, emptyGPU, emptyRAM, emptyMotherboard;

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
        //Update computer at start.
        UpdateComputer();
    }
    public static void ApplySaves()
    {
        mainCPU = GameSaver.Saves.cpu;
        if (mainCPU != null)
            mainCPU.RegenerateImage();

        mainMotherboard = GameSaver.Saves.motherboard;
        if (mainMotherboard != null)
            mainMotherboard.RegenerateImage();

        GPUs = GameSaver.Saves.gpus.ToList();
        foreach (GPU gpu in GPUs)
            if (gpu != null)
                gpu.RegenerateImage();

        RAMs = GameSaver.Saves.rams.ToList();
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
    /// Adds component slot with provided parameters.
    /// </summary>
    private void AddSlot(UnityAction onClick, Sprite image, Color color, string text)
    {
        //Instantiate slot.
        GameObject slot = Instantiate(slotPrefab, componentsParent);
        //Button event.
        slot.transform.GetComponent<Button>().onClick.AddListener(onClick);
        //Set image.
        slot.transform.Find("Image").GetComponent<Image>().sprite = image;
        //Set rarity line color.
        slot.transform.Find("Rarity line").GetComponent<Image>().color = color;
        //Set text.
        slot.GetComponentInChildren<Text>().text = text;
    }

    /// <summary>
    /// Adds component slot with provided parameters.
    /// </summary>
    private void AddSlot(UnityAction onClick, Sprite image, Rarity rarity, string text)
    {
        //Instantiate slot.
        GameObject slot = Instantiate(slotPrefab, componentsParent);
        //Button event.
        slot.transform.GetComponent<Button>().onClick.AddListener(onClick);
        //Set image.
        slot.transform.Find("Image").GetComponent<Image>().sprite = image;
        //Set rarity line color.
        slot.transform.Find("Rarity line").GetComponent<Image>().color =
                caseScroller.rarityColors[(int)rarity];
        //Set text.
        slot.GetComponentInChildren<Text>().text = text;
    }

    /// <summary>
    /// Updating slots of computer (CPU, motherboard, GPUs and RAMs).
    /// </summary>
    public void UpdateComputer()
    {
        //Destroy slots.
        int childCount = componentsParent.childCount;
        for (int i = 0; i < childCount; i++)
            Destroy(componentsParent.GetChild(i).gameObject);

        //Update CPU.
        if (mainCPU != null && mainMotherboard != null)
        {
            AddSlot(delegate { CPU_Clicked(); },
                mainCPU.image,
                mainCPU.rarity,
                $"<b>{LangManager.GetString("cpu")}, {mainCPU.socket}</b>\n{mainCPU.fullName}");
        }
        else if (mainMotherboard != null)
        {
            AddSlot(delegate { CPU_Clicked(); },
                emptyCPU,
                Color.white,
                $"<b>{LangManager.GetString("no_cpu")}</b>\n{mainMotherboard.socket}");
        }

        //Update motherboard.
        if (mainMotherboard != null)
        {
            AddSlot(delegate { Motherboard_Clicked(); },
                mainMotherboard.image,
                mainMotherboard.rarity,
                $"<b>{LangManager.GetString("motherboard")}," +
                $" {mainMotherboard.chipset.ToString().TrimStart('_')}</b>\n" +
                mainMotherboard.fullName);
        }
        else
        {
            AddSlot(delegate { Motherboard_Clicked(); },
                emptyMotherboard,
                Color.white,
                $"<b>{LangManager.GetString("no_motherboard")}</b>");
        }

        //Update GPUs.
        if (mainMotherboard != null)
        {
            for (int i = 0; i < mainMotherboard.busMultipliers.Length; i++)
            {
                //Fill list od GPUs.
                if (GPUs.Count <= i)
                    GPUs.Add(null);

                if (GPUs[i] != null)
                {
                    int index = i;
                    AddSlot(delegate { GPU_Clicked(index); },
                        GPUs[i].image,
                        GPUs[i].rarity,
                        $"<b>{LangManager.GetString("gpu")}," +
                        $" PCIe {GPUs[i].busVersion}.0 x{GPUs[i].busMultiplier} {LangManager.GetString("in")}" +
                        $" PCIe {mainMotherboard.busVersions[i]}.0 x{mainMotherboard.busMultipliers[i]}</b>\n" +
                        GPUs[i].fullName);
                }
                else
                {
                    int index = i;
                    AddSlot(delegate { GPU_Clicked(index); },
                        emptyGPU,
                        Color.white,
                        $"<b>{LangManager.GetString("no_gpu")}" +
                        $" PCIe {mainMotherboard.busVersions[i]}.0 x{mainMotherboard.busMultipliers[i]}</b>");
                }
            }
        }

        //Update RAMs.
        if (mainMotherboard != null)
        {
            for (int i = 0; i < mainMotherboard.RAMCount; i++)
            {
                if (RAMs.Count <= i)
                    RAMs.Add(null);

                if (RAMs[i] != null)
                {
                    int index = i;
                    AddSlot(delegate { RAM_Clicked(index); },
                        RAMs[i].image,
                        RAMs[i].rarity,
                        $"<b>{LangManager.GetString("ram")}," +
                        $" DDR{(RAMs[i].type == 1 ? null : RAMs[i].type.ToString())} </b>\n" +
                        RAMs[i].fullName);
                }
                else
                {
                    int index = i;
                    AddSlot(delegate { RAM_Clicked(index); },
                        emptyRAM,
                        Color.white,
                        $"<b>{LangManager.GetString("no_ram")}" +
                        $" DDR{(mainMotherboard.RAMType == 1 ? null : mainMotherboard.RAMType.ToString())}</b>");
                }
            }
        }
    }

    /// <summary>
    /// Instantiates equip slots.
    /// </summary>
    /// <param name="type">Type of equip components.</param>
    private void CreateEquipSlots(ComponentType type)
    {
        if (type == ComponentType.All)
            throw new System.ArgumentException("Argument \"type\" has value ComponentType.All");

        for (int i = 0; i < equipComponents.Count; i++)
        {
            GameObject currentSlot = Instantiate(equipSlotPrefab, equipParent);
            //Set image.
            currentSlot.transform.Find("Image").GetComponent<Image>().sprite = equipComponents[i].image;
            //Set rarity line color.
            currentSlot.transform.Find("RarityLine").GetComponent<Image>().color = caseScroller
                .rarityColors[(int)equipComponents[i].rarity];
            //Set text of name.
            currentSlot.transform.Find("Name").GetComponentInChildren<Text>().text = equipComponents[i].shortName;
            //Set button event.
            int index = i;
            selectedType = type;
            currentSlot.GetComponent<Button>().onClick.RemoveAllListeners();
            currentSlot.GetComponent<Button>().onClick.AddListener(delegate { EquipClicked(index); });
            currentSlot.transform.Find("InfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
            currentSlot.transform.Find("InfoButton").GetComponent<Button>().onClick
                .AddListener(delegate { ComponentInfo_Clicked(ComponentType.All, index); });
            equipText.text = null;
        }
        if (equipComponents.Count == 0)
        {
            switch (type)
            {
                case ComponentType.CPU:
                    equipText.text = LangManager.GetString("no_equippable_cpu");
                    break;
                case ComponentType.GPU:
                    equipText.text = LangManager.GetString("no_equippable_gpu");
                    break;
                case ComponentType.RAM:
                    equipText.text = LangManager.GetString("no_equippable_ram");
                    break;
                case ComponentType.Motherboard:
                    equipText.text = LangManager.GetString("no_equippable_motherboard");
                    break;
            }
        }
    }

    /// <summary>
    /// Shows image and activates info button.
    /// </summary>
    private void ShowImageOf(PCComponent component, ComponentType type, int index = -1)
    {
        //If component != null, show image of this component and activate info button.
        if (component != null)
        {
            componentImage.sprite = component.image;
            componentImageObj.SetActive(true);
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(delegate { ShowInfoOf(component, type, index); });
            infoButton.gameObject.SetActive(true);
        }
        else
        {
            componentImage.sprite = null;
            componentImageObj.SetActive(false);
            infoButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Event of CPU slot click.
    /// </summary>
    public void CPU_Clicked()
    {
        //If computer contains motherboard.
        if (mainMotherboard != null)
        {
            //Set equip components.
            equipComponents = Inventory.components.Where(x => x is CPU CPU && CPU.socket == mainMotherboard.socket).ToList();
            //Destroying all equip slots.
            DestroyEquipSlots();
            //Creating new equip slots.
            CreateEquipSlots(ComponentType.CPU);
        }
        else
        {
            //Set message "No motherboard" and clear equip components.
            equipText.text = LangManager.GetString("no_motherboard");
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            DestroyEquipSlots();
        }

        ShowImageOf(mainCPU, ComponentType.CPU);

        indexOfSelected = -1;
    }

    public void Motherboard_Clicked()
    {
        //If no components on computer.
        if (!ComputerHasComponents)
        {
            equipComponents = Inventory.components.Where(x => x is Motherboard).ToList();
            //Destroying all equip slots.
            DestroyEquipSlots();
            //Creating new equip slots.
            CreateEquipSlots(ComponentType.Motherboard);
        }
        else
        {
            //Set message "No motherboard!" and clear equip components.
            equipText.text = "No motherboard!";
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            DestroyEquipSlots();
        }

        ShowImageOf(mainMotherboard, ComponentType.Motherboard);

        indexOfSelected = -1;
    }

    public void RAM_Clicked(int index)
    {
        if (mainMotherboard != null)
        {
            equipComponents = Inventory.components.Where(x => x is RAM RAM && RAM.type == mainMotherboard.RAMType).ToList();
            //Destroying all equip slots.
            DestroyEquipSlots();
            //Creating new equip slots.
            CreateEquipSlots(ComponentType.RAM);
        }
        else
        {
            //Set message "No motherboard" and clear equip components.
            equipText.text = LangManager.GetString("no_motherboard");
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            DestroyEquipSlots();
        }

        ShowImageOf(RAMs[index], ComponentType.RAM, index);

        indexOfSelected = index;
    }

    public void GPU_Clicked(int index)
    {
        if (mainMotherboard != null)
        {
            equipComponents = Inventory.components
                .Where(x => x is GPU GPU)
                .ToList();
            //Destroying all equip slots.
            DestroyEquipSlots();
            //Creating new equip slots.
            CreateEquipSlots(ComponentType.GPU);
        }
        else
        {
            //Set message "No motherboard" and clear equip components.
            equipText.text = LangManager.GetString("no_motherboard");
            //Clear components to equip.
            equipComponents.Clear();
            //Destroying all equip slots.
            DestroyEquipSlots();
        }

        ShowImageOf(GPUs[index], ComponentType.GPU, index);

        indexOfSelected = index;
    }
    /// <summary>
    /// Destroys equip slots.
    /// </summary>
    private void DestroyEquipSlots()
    {
        int childCount = equipParent.childCount;
        for (int i = 0; i < childCount; i++)
            Destroy(equipParent.GetChild(i).gameObject);
    }
    /// <summary>
    /// Shows info window of component.
    /// </summary>
    /// <param name="component">Component which info will be showed.</param>
    /// <param name="type">Type of component.</param>
    /// <param name="index">Index of component (for GPUs and RAMs)</param>
    private void ShowInfoOf(PCComponent component, ComponentType type, int index)
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

                //Add listener for sell button.
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(delegate { Sell(type, index); });
                //Add listener for remove button.
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(delegate { Remove(type, index); });
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

                        //Add listener for sell button.
                        sellButton.onClick.RemoveAllListeners();
                        sellButton.onClick.AddListener(delegate { Sell(type, index); });
                        //Add listener for remove button.
                        removeButton.onClick.RemoveAllListeners();
                        removeButton.onClick.AddListener(delegate { Remove(type, index); });
                        //Add listener for unequip remove button.
                        unequipButton.onClick.RemoveAllListeners();
                        unequipButton.onClick.AddListener(delegate { Unequip(type, index); });
                    }
                }
                else
                {
                    //Enable unequip button.
                    unequipText.text = LangManager.GetString("unequip");
                    unequipButton.interactable = true;
                    unequipButtonAnimation.disabled = false;

                    //Enable sell button.
                    sellButton.interactable = true;
                    sellButtonAnimation.disabled = false;
                    sellText.text = $"{LangManager.GetString("sell")} (+{component.price / 20}$)";

                    //Enable remove button.
                    removeButton.interactable = true;
                    removeButtonAnimation.disabled = false;
                    removeImage.enabled = true;

                    //Add listener for sell button.
                    sellButton.onClick.RemoveAllListeners();
                    sellButton.onClick.AddListener(delegate { Sell(type, index); });
                    //Add listener for remove button.
                    removeButton.onClick.RemoveAllListeners();
                    removeButton.onClick.AddListener(delegate { Remove(type, index); });
                    //Add listener for unequip button.
                    unequipButton.onClick.RemoveAllListeners();
                    unequipButton.onClick.AddListener(delegate { Unequip(type, index); });
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
                        infoText.text = string.Format("{0}\n{1}",
                            LangManager.GetString("no_cpu"), LangManager.GetString("no_socket"));
                    else
                        infoText.text = string.Format("{0}\n{1} {2}",
                            LangManager.GetString("no_cpu"), LangManager.GetString("socket:"), mainMotherboard.socket);
                    break;
                case ComponentType.Motherboard:
                    infoText.text = LangManager.GetString("no_motherboard");
                    break;
                case ComponentType.GPU:
                    infoText.text = string.Format("{0}\n{1} PCIe {2}.0 x{3}.",
                        LangManager.GetString("no_gpu"), LangManager.GetString("interface:"),
                        mainMotherboard.busVersions[index], mainMotherboard.busMultipliers[index]);
                    break;
                case ComponentType.RAM:
                    infoText.text = string.Format("{0} DDR{1}.",
                        LangManager.GetString("gen:"),
                        mainMotherboard.RAMType == 1 ? null : mainMotherboard.RAMType.ToString());
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
    /// <summary>
    /// Event of clicked info button.
    /// </summary>
    /// <param name="type">Type of component (All - it is equip slot).</param>
    /// <param name="index">Index of component.</param>
    public void ComponentInfo_Clicked(ComponentType type, int index)
    {
        //Open info by component.
        switch (type)
        {
            case ComponentType.CPU:
                ShowInfoOf(mainCPU, type, index);
                break;
            case ComponentType.Motherboard:
                ShowInfoOf(mainMotherboard, type, index);
                break;
            case ComponentType.GPU:
                if (GPUs.Count > index)
                    ShowInfoOf(GPUs[index], type, index);
                else
                    ShowInfoOf(null, type, index);
                break;
            case ComponentType.RAM:
                if (RAMs.Count > index)
                    ShowInfoOf(RAMs[index], type, index);
                else
                    ShowInfoOf(null, type, index);
                break;
            case ComponentType.All:
                ShowInfoOf(equipComponents[index], type, index);
                break;
        }
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
                    CPU_Clicked();
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
                    Motherboard_Clicked();
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
                    GPU_Clicked(index);
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
                    RAM_Clicked(index);
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
                        CPU_Clicked();
                        break;
                    case ComponentType.Motherboard:
                        Motherboard_Clicked();
                        break;
                    case ComponentType.GPU:
                        GPU_Clicked(index);
                        break;
                    case ComponentType.RAM:
                        RAM_Clicked(index);
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
    /// Removes button clicked.
    /// </summary>
    /// <param name="type">Type of component.</param>
    /// <param name="index">Index of component.</param>
    public void Remove(ComponentType type, int index)
    {
        //Play sound.
        soundManager.PlaySound(removeClip);
        switch (type)
        {
            case ComponentType.CPU:
                mainCPU = null;
                CPU_Clicked();
                Back();
                break;
            case ComponentType.Motherboard:
                mainMotherboard = null;
                Motherboard_Clicked();
                Back();
                break;
            case ComponentType.GPU:
                GPUs[index] = null;
                GPU_Clicked(index);
                Back();
                break;
            case ComponentType.RAM:
                RAMs[index] = null;
                RAM_Clicked(index);
                Back();
                break;
            case ComponentType.All:
                Inventory.components.Remove(equipComponents[index]);
                switch (selectedType)
                {
                    case ComponentType.CPU:
                        CPU_Clicked();
                        break;
                    case ComponentType.Motherboard:
                        Motherboard_Clicked();
                        break;
                    case ComponentType.GPU:
                        GPU_Clicked(index);
                        break;
                    case ComponentType.RAM:
                        RAM_Clicked(index);
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
        soundManager.PlaySound(equipClips[UnityEngine.Random.Range(0, equipClips.Length)]);
        switch (type)
        {
            case ComponentType.CPU:
                Inventory.components.Add((PCComponent)mainCPU.Clone());
                mainCPU = null;
                CPU_Clicked();
                Back();
                break;
            case ComponentType.Motherboard:
                Inventory.components.Add((PCComponent)mainMotherboard.Clone());
                mainMotherboard = null;
                Motherboard_Clicked();
                Back();
                break;
            case ComponentType.GPU:
                Inventory.components.Add((PCComponent)GPUs[index].Clone());
                GPUs[index] = null;
                GPU_Clicked(index);
                Back();
                break;
            case ComponentType.RAM:
                Inventory.components.Add((PCComponent)RAMs[index].Clone());
                RAMs[index] = null;
                RAM_Clicked(index);
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
        soundManager.PlaySound(equipClips[UnityEngine.Random.Range(0, equipClips.Length)]);

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
                CPU_Clicked();
                break;
            case ComponentType.Motherboard:
                if (mainMotherboard != null)
                    Inventory.components.Add(mainMotherboard);
                mainMotherboard = (Motherboard)equipComponents[index];
                Motherboard_Clicked();
                break;
            case ComponentType.RAM:
                if (RAMs.Count > indexOfSelected && RAMs[indexOfSelected] != null)
                    Inventory.components.Add(RAMs[indexOfSelected]);
                RAMs[indexOfSelected] = (RAM)equipComponents[index];
                RAM_Clicked(indexOfSelected);
                break;
            case ComponentType.GPU:
                if (GPUs.Count > index && GPUs[indexOfSelected] != null)
                    Inventory.components.Add(GPUs[indexOfSelected]);
                GPUs[indexOfSelected] = (GPU)equipComponents[index];
                GPU_Clicked(indexOfSelected);
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
