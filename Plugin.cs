using BepInEx;
using BepInEx.Logging;
using BeltSlot.Patches;
using EFT.InventoryLogic;
using UnityEngine;
using EFT.UI.DragAndDrop;
using EFT.UI;
using Comfort.Common;
using EFT.InputSystem;
using BeltSlot.Helpers;
using UnityEngine.EventSystems;
using EFT;
using HarmonyLib;
using EFT.Hideout;
using EFT.UI.Screens;
using UnityEngine.SceneManagement;
using static EFT.UI.TraderScreensGroup;
using System.Reflection;
using System.Timers;
using System;
using UnityEngine.UI;
using EFT.Interactive;
using System.Threading.Tasks;

namespace BeltSlot
{
    [BepInPlugin("BeltSlot", "BeltSlot", "1.0.0")]
    [BepInDependency("com.SPT.core", "3.11.0")]
    public class Plugin : BaseUnityPlugin
    {
        #region Variables
        private const string GCLASS_FIELD_NAME = "gclass3521_0";
        private const string ALL_TABS_FIELD_NAME = "tab_0";
        private const string CURRENT_TAB_FIELD_NAME = "tab_2";

        private static FieldInfo _gclass = null;
        private static FieldInfo _servicesScreen = null;
        private static FieldInfo _background = null;
        private static FieldInfo _allTabs = null;
        private static FieldInfo _currentTab = null;
        private Vector3 mousePosition = Vector3.zero;
        int currentTraderIndex = 0;
        int currentButtonIndex = 0;
        EEftScreenType previousScreenType = EEftScreenType.None;
        EEftScreenType eScreenType;
        ETraderMode eTraderMode = ETraderMode.Trade;
        CurrentScreenSingletonClass currentScreenSingletonClass = null;

        Timer buttonTimer = new Timer(2000);

        private bool inventoryOpen = false;
        private bool tradersOpen = false;
        private bool fleaOpen = false;
        private bool hideOutOpen = false;
        private bool buttonSelected = false;
        private bool buttonPressedBool = false;
        private bool escapePressedBool = false;

        private bool enableLogging = false;
        #endregion

        public GameObject? armBandSlot;
        public GameObject? tacticalSlot;
        public GameObject? commonUI;
        public GameObject? generatedGridsView;
        public ContainedGridsView? containedGridsView;
        public ItemUiContext? ItemUiContext;
        public CompoundItem? beltItem;
        public Item? itemToTest;
        public string? itemId = "0000000000";
        public ItemContextAbstractClass? newSourceContext;
        public ItemUiContext? newItemUiContext;
        public InventoryEquipment? inventoryEquipment;
        private static UI_Mappings? uiMappings;
        public bool newItemAdded = false;
        public bool beltToggle = true;
        public static Plugin? Instance { get; private set; }
        internal static UI_Mappings? UiMappings { get => uiMappings; set => uiMappings = value; }

        public static ManualLogSource? LogSource;

        #region Test Methods
        // Check if the input field is focused, if so, return true
        private bool isInputFieldFocused()
        {
            if (EventSystem.current.currentSelectedGameObject != null
            && EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            {
                if (enableLogging)
                {
                    Logger.LogInfo("Text field in focus.");
                }
                return true;
            }
            return false;
        }

        // Check if the inventory screen is focused, if so, return true
        private bool isInventoryScreenFocus()
        {
            var inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            if (inventoryScreen.isActiveAndEnabled)
            {
                if (enableLogging)
                {
                    Logger.LogInfo("Inventory screen is focused.");
                }
                return true;
            }
            return false;
        }

        // Check if the trader screen is focused, if so, return true
        private bool isTraderScreenFocus()
        {
            var traderScreensGroup = getTraderScreensGroup();
            if (traderScreensGroup.isActiveAndEnabled)
            {
                if (enableLogging)
                {
                    Logger.LogInfo("Trader screen is focused.");
                }
                return true;
            }
            return false;
        }

        // Get the current game world, if it is hideout, return true, otherwise return false
        private bool getCurrentGameWorld()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld is HideoutGameWorld)
            {
                if (enableLogging)
                {
                    Logger.LogInfo("Game world is hideout");
                }
                return true;
            }
            if (gameWorld is ClientGameWorld)
            {
                if (enableLogging)
                {
                    Logger.LogInfo("Game world is main player");
                }
                return false;
            }
            return true;
        }

        // Check if the menutaskbar is active, if so check the current screen type
        private bool getButtonInteractable()
        {
            MenuTaskBar menuTaskBar = MonoBehaviourSingleton<PreloaderUI>.Instance.MenuTaskBar;

            if (menuTaskBar.isActiveAndEnabled)
            {
                eScreenType = getCurrentScreen();

                if (eScreenType == EEftScreenType.MainMenu || eScreenType == EEftScreenType.Inventory || eScreenType == EEftScreenType.Trader || eScreenType == EEftScreenType.Hideout || eScreenType == EEftScreenType.FleaMarket)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        // Check if the current scene is EftMainScene, if so, return false, otherwise return true
        private bool testScene()
        {
            string currentScene = getCurrentScene();
            if (currentScene == "Unkown")
            {
                return false;
            }

            if (currentScene == "EftMainScene" || currentScene == "LoginUIScene")
            {
                return false;
            }
            return true;
        }

        // Check if screen has changed
        private bool testScreenChange()
        {
            if (previousScreenType == getCurrentScreen())
            {
                return false;
            }
            return true;
        }

        // Check if the hideout loading screen is active, if so, return false, otherwise return true
        private bool getHideoutLoading()
        {
            var _hideoutLoadingScreen = MonoBehaviourSingleton<PreloaderUI>.Instance.HideoutLoadingScreen;

            if (_hideoutLoadingScreen == null)
            {
                return true;
            }

            if (!_hideoutLoadingScreen.isActiveAndEnabled)
            {
                if (enableLogging)
                {
                    Logger.LogInfo($"loading screen is not active");
                }
                return true;
            }

            Type type = typeof(HideoutLoadingScreen);
            if (_background == null)
            {
                _background = AccessTools.Field(type, "_background");
            }

            Image background = (Image)_background.GetValue(_hideoutLoadingScreen);

            if (background.enabled)
            {
                return false;
            }
            return true;
        }

        // Check current screen type
        private EEftScreenType getCurrentScreen()
        {
            if (currentScreenSingletonClass == null)
            {
                currentScreenSingletonClass = CurrentScreenSingletonClass.Instance;
            }

            MenuTaskBar menuTaskBar = MonoBehaviourSingleton<PreloaderUI>.Instance.MenuTaskBar;

            if (menuTaskBar.isActiveAndEnabled)
            {
                EEftScreenType _eScreenType = currentScreenSingletonClass.CurrentScreenController.ScreenType;

                if (enableLogging)
                {
                    Logger.LogInfo($"Current screen type: {_eScreenType}");
                }
                return _eScreenType;
            }
            else
            {
                if (enableLogging)
                {
                    Logger.LogInfo("MenuTaskBar is not active, returning None");
                }
                return EEftScreenType.None;
            }
        }

        // Check current scene
        private string getCurrentScene()
        {
            string _currentScene = SceneManager.GetActiveScene().name;
            if (_currentScene == null || _currentScene == string.Empty)
            {
                if (enableLogging)
                {
                    Logger.LogInfo("Current scene is null or empty");
                }
                return "Unknown";
            }

            if (enableLogging)
            {
                Logger.LogInfo($"Current scene: {_currentScene}");
            }

            return _currentScene;
        }

        private bool testMouseMovement(Vector3 position)
        {
            mousePosition = Input.mousePosition;
            if (mousePosition != position)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Get Methods
        // Get Tarkov application instance, if it exists, otherwise return null
        private TarkovApplication getTarkovApplication()
        {
            // OH MY GOD THIS WORKS!!!!
            // It took me two days to figure this out
            TarkovApplication _tarkovApplication;
            if (TarkovApplication.Exist(out _tarkovApplication))

                if (_tarkovApplication == null)
                {
                    if (enableLogging)
                    {
                        Logger.LogInfo("Tarkov application is null");
                    }
                    Logger.LogInfo("Tarkov application is null");
                    return null;
                }
            return _tarkovApplication;
        }

        // Get the TraderScreensGroup instance, if it exists, otherwise return null
        private TraderScreensGroup getTraderScreensGroup()
        {
            MenuUI menuUI = MenuUI.Instance;
            TraderScreensGroup traderScreensGroup = menuUI.TraderScreensGroup;
            return traderScreensGroup;
        }

        // Get the ServicesScreen instance, if it exists, otherwise return null
        private ServicesScreen getServicesScreen()
        {
            var _traderGroupScreen = getTraderScreensGroup();

            if (_traderGroupScreen == null)
            {
                return null;
            }

            if (!_traderGroupScreen.isActiveAndEnabled)
            {
                if (enableLogging)
                {
                    Logger.LogInfo($"inventory screen is not active");
                }
                return null;
            }

            Type type = typeof(TraderScreensGroup);
            if (_servicesScreen == null)
            {
                _servicesScreen = AccessTools.Field(type, "_servicesScreen");
            }

            return (ServicesScreen)_servicesScreen.GetValue(_traderGroupScreen);
        }
        #endregion
        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            Plugin.Instance = this;
            UiMappings = new UI_Mappings();
            // save the Logger to variable so we can use it elsewhere in the project
            LogSource = Logger;
            LogSource.LogInfo("plugin loaded!");

            // uncomment line(s) below to enable desired example patch, then press F6 to build the project:
            //new ContainedGridsViewPatch().Enable();
            //new EquipmentTabPatch().Enable();
            //new ContainersPanelPatch().Enable();
            new SlotItemViewPatch().Enable();
            new InventoryEquipmentPatch().Enable();
            ContextMenuShortcutPatches.Enable();
            R.Init();
        }

        private void Update()
        {
            OnEnterInventory();

            UpdateArmbandSlot();

            ClearBeltGrid();

            if (Input.GetKeyDown(KeyCode.B))
            {
                beltToggle = !beltToggle;
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                Vector2 mousePosition = Input.mousePosition;
                LogSource?.LogInfo("mouse position: " + mousePosition);

                if(!TestArmBand())
                {
                    LogSource?.LogInfo("No item in ArmBand slot or item is not a compound item.");
                    return;
                }
                if(uiMappings != null)
                {
                    OpenBelt(10);
                }

            }

            if(Input.GetKeyDown(KeyCode.K))
            {

                UiMappings?.setBeltSlotGrid();
                LogSource?.LogInfo("Belt slot child count: " + UiMappings?.beltSlot?.transform.childCount);
            }
        }

        private static async Task pauseWait(int wait)
        {
            // A pause method that waits for a specified amount of time
            await Task.Delay(wait);
        }

        private async void OnEnterInventory()
        {
            if (!testScene())
            {
                return;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene")
            {
                return;
            }
            if (!getHideoutLoading())
            {
                return;
            }
            // do not trigger if inventory screen is not focused or input field is focused
            if (isInventoryScreenFocus() && !isInputFieldFocused())
            {
                if (UiMappings != null)
                {
                    if (UiMappings.beltSlot == null)
                    {

                        UiMappings.setContainer_Mappings();
                        UiMappings.setBeltSlot_Settings();
                        UiMappings?.setPrelaoderUI_Mappings();
                        await pauseWait(2000); // Wait for 100 milliseconds to ensure the UI is ready
                        if(beltToggle)
                        {
                            if (!TestBeltHasGrid())
                            {
                                if (TestArmBand())
                                {
                                    OpenBelt(1000);
                                    //await pauseWait(100); // Wait for 100 milliseconds to ensure the UI is ready
                                    Slot? slot = inventoryEquipment?.GetSlot(EquipmentSlot.ArmBand);
                                    itemToTest = slot?.ContainedItem;
                                    itemId = slot?.ContainedItem.Id;
                                    return;
                                }
                            }
                        }
                    }
                    else if(UiMappings.beltSlot.transform.GetSiblingIndex() != 4)
                    {
                        UiMappings.setBeltSlot_Settings();
                        if (enableLogging)
                        {
                            Logger.LogInfo("belt slot in the wrong place, fixing");
                        }

                        await pauseWait(100); // Wait for 100 milliseconds to ensure the UI is ready
                        if(beltToggle)
                        {
                            if(!TestBeltHasGrid())
                            {
                                if (TestArmBand())
                                {
                                    OpenBelt(10);
                                    //await pauseWait(100); // Wait for 100 milliseconds to ensure the UI is ready
                                    Slot? slot = inventoryEquipment?.GetSlot(EquipmentSlot.ArmBand);
                                    itemToTest = slot?.ContainedItem;
                                    itemId = slot?.ContainedItem.Id;
                                    return;
                                }
                            }
                        }
                    }
                    if (enableLogging)
                    {
                        Logger?.LogInfo("belt slot in the right place");
                    }
                    return;
                }
            }
        }

        private async void UpdateArmbandSlot()
        {
            if (!testScene())
            {
                return;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene")
            {
                return;
            }
            if (!getHideoutLoading())
            {
                return;
            }
            // do not trigger if inventory screen is not focused or input field is focused
            if (isInventoryScreenFocus() && !isInputFieldFocused())
            {
                if (UiMappings?.beltSlot == null)
                {
                    UiMappings?.setContainer_Mappings();
                    UiMappings?.setBeltSlot_Settings();
                    UiMappings?.setPrelaoderUI_Mappings();
                }
                await pauseWait(100); // Wait for 100 milliseconds to ensure the UI is ready
                if (beltToggle)
                {
                    if(TestItem(itemId))
                    {
                        if (!TestBeltHasGrid())
                        {
                            OpenBelt(10);
                        }
                    }
                }
                RefreshBeltSlot();
            }
        }

        private void ClearBeltGrid()
        {
            if(!beltToggle)
            {
                if(TestBeltHasGrid())
                {
                    CloseBelt();
                }
            }
        }

        private bool TestArmBand()
        {
            if (!testScene())
            {
                return false;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene")
            {
                return false;
            }
            if (!getHideoutLoading())
            {
                return false;
            }
            // do not trigger if inventory screen is not focused or input field is focused
            if (isInventoryScreenFocus() && !isInputFieldFocused())
            {
                if (inventoryEquipment != null)
                {
                    if(uiMappings?.armBandSlot == null)
                    {
                        uiMappings?.setEquipment_Mappings();
                    }
                    Slot slot = inventoryEquipment.GetSlot(EquipmentSlot.ArmBand);
                    if(slot.Items.IsNullOrEmpty())
                    {
                        itemId = "0000000000"; // Reset the item ID
                        if(enableLogging)
                        {
                            LogSource?.LogInfo("No item in ArmBand slot");
                        }
                        return false;
                    }
                    Item item = slot.ContainedItem;
                    if(!item.IsContainer)
                    {
                        itemId = "0000000000"; // Reset the item ID
                        if (enableLogging)
                        {
                            LogSource?.LogInfo("Item in ArmBand slot is not a compound item: " + item.Id);
                        }
                        return false;
                    }
                    else
                    {
                        if (enableLogging)
                        {
                            LogSource?.LogInfo("Item in ArmBand slot is a compound item: " + item.Id);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TestItem(String? item)
        {
            if (!testScene())
            {
                return false;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene")
            {
                return false;
            }
            if (!getHideoutLoading())
            {
                return false;
            }
            // do not trigger if inventory screen is not focused or input field is focused
            if (isInventoryScreenFocus() && !isInputFieldFocused())
            {
                if(TestArmBand())
                {
                    string? itemTest = item;
                    Slot? slot = inventoryEquipment?.GetSlot(EquipmentSlot.ArmBand);
                    /*if(itemTest == null)
                    {
                        if(enableLogging)
                        {
                            LogSource?.LogInfo("No item in ArmBand slot, either first start or ");
                        }
                        itemToTest = slot?.ContainedItem;
                        return true;
                    }*/
                    if(itemTest != slot?.ContainedItem.Id)
                    {
                        if(enableLogging)
                        {
                            LogSource?.LogInfo("Item in ArmBand slot has changed, updating itemToTest");
                        }
                        itemToTest = slot?.ContainedItem;
                        itemId = slot?.ContainedItem.Id;
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        private bool TestBeltHasGrid()
        {
            if (!testScene())
            {
                return false;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene")
            {
                return false;
            }
            if (!getHideoutLoading())
            {
                return false;
            }
            // do not trigger if inventory screen is not focused or input field is focused
            if (isInventoryScreenFocus() && !isInputFieldFocused())
            {
                if(UiMappings?.beltSlot?.transform.childCount >= 6)
                {
                    return true;
                }
            }
            return false;
        }

        private void RefreshBeltSlot()
        {
            if (!testScene())
            {
                return;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene")
            {
                return;
            }
            if (!getHideoutLoading())
            {
                return;
            }
            // do not trigger if inventory screen is not focused or input field is focused
            if (isInventoryScreenFocus() && !isInputFieldFocused())
            {
                if(beltToggle)
                {
                    if (TestArmBand())
                    {
                        if (!TestBeltHasGrid())
                        {
                            OpenBelt(10);
                            //OpenBelt(10); // Open the belt slot after closing it
                        }
                    }
                }
            }
            return;
        }

        private async void OpenBelt(int wait)
        {
            await pauseWait(wait); // Wait for 1 second to ensure the UI is ready
            Vector2 mousePosition = Input.mousePosition;
            //LogSource?.LogInfo("mouse position: " + mousePosition);
            // This bit of code finally does what I was wanting to do, it opens whatever item is in the ArmBand slot
            if(uiMappings?.armBandSlot?.transform.childCount <= 8)
            {
                return;
            }
            GameObject? armBandClone = uiMappings?.armBandSlot?.transform.GetChild(8).gameObject;
            await pauseWait(wait); // Wait for 100 milliseconds to ensure the UI is ready
            SlotItemView? slotItemView = armBandClone?.GetComponent<SlotItemView>();
            await pauseWait(wait); // Wait for 100 milliseconds to ensure the UI is ready
            slotItemView?.OnClick(PointerEventData.InputButton.Left, mousePosition, true);
            await pauseWait(wait);
            UiMappings?.setBeltSlotGrid();
            return;
            // Code above is equivalent to clicking the ArmBand slot with the mouse, which opens the item in that slot
        }

        private void CloseBelt()
        {
            itemToTest = null; // Clear the item to test
            itemId = "0000000000"; // Reset the item ID
            GameObject? beltGrids = UiMappings?.beltSlot?.transform.GetChild(5).gameObject;
            beltGrids.transform.parent = uiMappings?.getGridWindowClone().transform;
            UiMappings?.getGridWindowClone().SetActive(true);
            uiMappings?.getCloseButton(UiMappings?.getGridWindowClone()).OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }
}
