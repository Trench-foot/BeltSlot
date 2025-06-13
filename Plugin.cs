using BeltSlot.Helpers;
using BeltSlot.Patches;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFT.UI.Screens;
using HarmonyLib;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static EFT.UI.TraderScreensGroup;

namespace BeltSlot
{
    [BepInPlugin("BeltSlot", "BeltSlot", "1.0.0")]
    [BepInDependency("com.SPT.core", "3.11.0")]
    public class Plugin : BaseUnityPlugin
    {
        #region Variables
        private static FieldInfo? _background = null;
        private Vector3 mousePosition = Vector3.zero;
        EEftScreenType previousScreenType = EEftScreenType.None;
        EEftScreenType eScreenType;
        ETraderMode eTraderMode = ETraderMode.Trade;
        CurrentScreenSingletonClass? currentScreenSingletonClass = null;
        public Item? itemToTest;
        public string? itemId = "0000000000";
        public InventoryEquipment? inventoryEquipment;
        private static UI_Mappings? uiMappings;
        public bool beltToggle = true;
        public static Plugin? Instance { get; private set; }
        internal static UI_Mappings? UiMappings { get => uiMappings; set => uiMappings = value; }
        public static ManualLogSource? LogSource;
        private bool enableLogging = false;
        #endregion

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

            // Removed (currentScene == "EftMainScene") check, see what this effects
            if (currentScene == "LoginUIScene")
            {
                return false;
            }
            return true;
        }

        // Check if the current scene is a raid scene, if so, return true, otherwise return false
        private bool testInRaidScene()
        {
            string _currentScene = getCurrentScene();
            if(_currentScene == "Factory_Rework_Day_Scripts" || _currentScene == "Factory_Rework_Night_Scripts")
            {
                // Current scene is Factory
                return true;
            }
            else if(_currentScene == "Sandbox_Scripts")
            {
                // Current scene is Ground Zero
                return true;
            }
            else if(_currentScene == "City_Scripts")
            {
                // Current scene is Streets of Tarkov
                return true;
            }
            else if(_currentScene == "Shopping_Mall_Scripts")
            {
                // Current scene is Shopping Mall
                return true;
            }
            else if(_currentScene == "custom_Scripts")
            {
                // Current scene is Customs
                return true;
            }
            else if(_currentScene == "woods_Scripts")
            {
                // Current scene is Woods
                return true;
            }
            else if(_currentScene == "Reserve_Base_Scripts")
            {
                // Current scene is Reserve
                return true;
            }
            else if(_currentScene == "Lighthouse_Scripts")
            {
                // Current scene is Lighthouse
                return true;
            }
            else if(_currentScene == "shoreline_scripts")
            {
                // Current scene is Shoreline
                return true;
            }
            else if(_currentScene == "Laboratory_Scripts")
            {
                // Current scene is Laboratory
                return true;
            }
            else
            {
                return false;
            }
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

        // Test the ArmBand slot, if it has a compound item, return true, otherwise return false
        private bool TestArmBand()
        {
            if (!testScene() && !testInRaidScene())
            {
                return false;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene" && !testInRaidScene() )
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

        // Test the item in the ArmBand slot, if it has changed, return true, otherwise return false
        private bool TestItem(String? item)
        {
            if (!testScene() && !testInRaidScene())
            {
                return false;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene" && !testInRaidScene() )
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

        // Test if the belt slot has a grid, if so, return true, otherwise return false
        private bool TestBeltHasGrid()
        {
            if (!testScene() && !testInRaidScene())
            {
                return false;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene" && !testInRaidScene() )
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
        #endregion

        #region Get Methods
        // Get Tarkov application instance, if it exists, otherwise return null
        // not currrenlty used, but might be useful in the future
        private TarkovApplication? getTarkovApplication()
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
        #endregion

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            Plugin.Instance = this;
            UiMappings = new UI_Mappings();
            // save the Logger to variable so we can use it elsewhere in the project
            LogSource = Logger;
            LogSource.LogInfo("plugin loaded!");

            // Currently just used to get an instance of the inventory equipment screen
            new InventoryEquipmentPatch().Enable();
            // Think this is where the logic for auto placement of items into the inventory is, need to explore it
            //new ItemUiContextPatch().Enable();
        }

        // Using lateupdate because hoping it would fix issues with the belt grid not opening when it should
        // not sure if it worked
        private void LateUpdate()
        {
            if (!Singleton<CommonUI>.Instantiated)
            {
                return;
            }
            // Checks if inventory open and sets the inventoryEquipment variable
            OnEnterInventory();

            // Handles updating the armband slot variables and sets the belt slot
            UpdateArmbandSlot();

            // Handles clearing the belt grid if the toggle is off
            ClearBeltGrid();

            //InRaidGridHelper();

            // Belt toggle
            /*if (Input.GetKeyDown(KeyCode.Pause))
            {
                beltToggle = !beltToggle;
            }

            // Test for current screen and send log message, for debuging purposes
            if (Input.GetKeyDown(KeyCode.O))
            {
                string _currentScene = getCurrentScene();
                //if(enableLogging)
                //{
                    LogSource?.LogInfo($"Current scene: {_currentScene}");
                //}
                EEftScreenType _currentScreen = getCurrentScreen();
                //if (enableLogging)
                //{
                    LogSource?.LogInfo($"Current screen type: {_currentScreen}");
                //}

            }*/
        }

        // A pause method that waits for a specified amount of time
        // Not currently used as it results in a sound bug currently
        // with the opening of the belt grid
        private static async Task pauseWait(int wait)
        {
            // A pause method that waits for a specified amount of time
            await Task.Delay(wait);
        }

        #region Methods
        // Open the belt slot if the inventory is open and the armband slot has a compound item
        // also sets the uiMappings if they are null
        private void OnEnterInventory()
        {
            if (!testScene() && !testInRaidScene())
            {
                return;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene" && !testInRaidScene())
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
                        //await pauseWait(2000); // Wait for 100 milliseconds to ensure the UI is ready
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

                        //await pauseWait(100); // Wait for 100 milliseconds to ensure the UI is ready
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

        // Updates the armband slot and opens the belt slot if the item in the armband slot has changed
        private void UpdateArmbandSlot()
        {
            if (!testScene() && !testInRaidScene())
            {
                return;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene" && !testInRaidScene())
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
                //await pauseWait(100); // Wait for 100 milliseconds to ensure the UI is ready
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

        // Refreshes the belt slot if the item in the armband slot has not changed but the belt slot is not open
        private void RefreshBeltSlot()
        {
            if (!testScene() && !testInRaidScene())
            {
                return;
            }
            if (getCurrentScene() != "CommonUIScene" && getCurrentScene() != "MenuUIScene" && !testInRaidScene() )
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
                        }
                    }
                }
            }
            return;
        }

        // Opens the belt slot by simulating a click on the armband slot
        private void OpenBelt(int wait)
        {
            //await pauseWait(wait); // Wait for 1 second to ensure the UI is ready
            Vector2 mousePosition = Input.mousePosition;
            //LogSource?.LogInfo("mouse position: " + mousePosition);
            // This bit of code finally does what I was wanting to do, it opens whatever item is in the ArmBand slot
            if(uiMappings?.armBandSlot?.transform.childCount <= 8)
            {
                return;
            }
            GameObject? armBandClone = uiMappings?.armBandSlot?.transform.GetChild(8).gameObject;
            //await pauseWait(wait); // Wait for 100 milliseconds to ensure the UI is ready
            SlotItemView? slotItemView = armBandClone?.GetComponent<SlotItemView>();
            //await pauseWait(wait); // Wait for 100 milliseconds to ensure the UI is ready
            slotItemView?.OnClick(PointerEventData.InputButton.Left, mousePosition, true);
            //await pauseWait(wait);
            UiMappings?.setBeltSlotGrid();
            return;
            // Code above is equivalent to clicking the ArmBand slot with the mouse, which opens the item in that slot
        }

        // Clears the belt grid if the toggle is off
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

        // Closes the belt slot by simulating a click on the close button
        private void CloseBelt()
        {
            itemToTest = null; // Clear the item to test
            itemId = "0000000000"; // Reset the item ID
            GameObject? beltGrids = UiMappings?.beltSlot?.transform.GetChild(5).gameObject;
            beltGrids.transform.parent = uiMappings?.getGridWindowClone().transform;
            UiMappings?.getGridWindowClone().SetActive(true);
            uiMappings?.getCloseButton(UiMappings?.getGridWindowClone()).OnPointerClick(new PointerEventData(EventSystem.current));
        }

        // Doesn't do what I wanted, just gives extra null reference errors.  Was meant to help and open the belt tab
        // while in raid.  Might want to revisit it
        /*private void InRaidGridHelper()
        {
            if(!testInRaidScene())
            {
                return;
            }
            if(isInventoryScreenFocus())
            {
                if (uiMappings?.lootContainerGameObject == null)
                {
                    uiMappings?.setComplexLootUI_Mappings();
                    if (beltToggle)
                    {
                        if (!TestBeltHasGrid())
                        {
                            if ((bool)(uiMappings?.lootContainerGameObject.activeSelf))
                            {
                                OpenBelt(100);
                            }
                        }
                    }
                }
                else if (beltToggle)
                {
                    if (!TestBeltHasGrid())
                    {
                        OpenBelt(100);
                    }
                }
            }
        }*/
        #endregion
    }
}
