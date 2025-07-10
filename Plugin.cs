using BeltSlot.Helpers;
using BeltSlot.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Screens;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeltSlot
{
    [BepInPlugin("BeltSlot", "BeltSlot", "0.9.9")]
    [BepInDependency("com.SPT.core", "3.11.0")]
    [BepInDependency("com.aaaWTT-PacknStrap.Core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private bool enableLogging = false;
        public bool packNStrapInstalled;
        public bool iconToggle = true;
        public bool inventoryScreenLoaded = false;
        public bool complexStashPanelLoaded = false;
        public string? itemId = "0000000000";
        public Item? itemToTest;
        internal static Plugin Instance { get; set; }
        internal ManualLogSource Log { get; set; }
        public InventoryEquipment inventoryEquipment;
        public InventoryScreen inventoryScreen;
        public CurrentScreenSingletonClass currentScreenSingletonClass = null;
        private static UI_Mappings uiMappings;
        internal static UI_Mappings UiMappings { get => uiMappings; set => uiMappings = value; }

        #region Test Methods
        // Check if the inventory screen is focused, if so, return true
        private bool isInventoryScreenFocus()
        {
            var inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            if (inventoryScreen.isActiveAndEnabled)
            {
                if (enableLogging)
                {
                    Log.LogInfo("Inventory screen is focused.");
                }
                return true;
            }
            return false;
        }
        // Check current screen type
        public EEftScreenType getCurrentScreen()
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
                    Log.LogInfo($"Current screen type: {_eScreenType}");
                }
                return _eScreenType;
            }
            else
            {
                if (enableLogging)
                {
                    Log.LogInfo("MenuTaskBar is not active, returning None");
                }
                return EEftScreenType.None;
            }
        }
        // Check if the health panel is active, if so, return true
        private bool checkInventoryTab()
        {
            if (uiMappings.healthPanel == null || uiMappings.healthParameter == null)
            {
                uiMappings.setHealthPanel_Mappings();
            }
            if (uiMappings.healthPanel.activeSelf || uiMappings.healthParameter.activeSelf)
            {
                if (enableLogging)
                {
                    Logger.LogInfo("Health panel active is focused.");
                }
                return true;
            }
            return false;
        }
        // Test the ArmBand slot, if it has an item, return true, otherwise return false
        private bool TestSlotHasItem()
        {
            if (inventoryEquipment != null)
            {
                Slot slot = inventoryEquipment.GetSlot(EquipmentSlot.ArmBand);
                if (slot.Items.IsNullOrEmpty())
                {
                    itemId = "0000000000"; // Reset the item ID
                    if (enableLogging)
                    {
                        Log.LogInfo("No item in ArmBand slot");
                    }
                    return false;
                }
                else
                {
                    Item item = slot.ContainedItem;
                    itemId = item.Id;
                    if (enableLogging)
                    {
                        Log.LogInfo("Item in ArmBand slot: " + item.Id);
                    }
                    return true;
                }
            }
            Log.LogError("InventoryEquipment is null, cannot check ArmBand slot");
            return false;
        }
        // Test the ArmBand slot, if it has a compound item, return true, otherwise return false
        public bool TestItemIsCompound()
        {
            //InventoryEquipment _inventoryEquipment = Singleton<InventoryEquipment>.Instance;
            if (inventoryEquipment != null)
                {
                    Slot slot = inventoryEquipment.GetSlot(EquipmentSlot.ArmBand);
                    Item item = slot.ContainedItem;
                    if (!item.IsContainer)
                    {
                        //itemId = item.Id; // Reset the item ID
                        if (enableLogging)
                        {
                            Log.LogInfo("Item in ArmBand slot is not a compound item: " + item.Id);
                        }
                        return false;
                    }
                    else
                    {
                        //itemId = item.Id; // Reset the item ID
                        if (enableLogging)
                        {
                            Log.LogInfo("Item in ArmBand slot is a compound item: " + item.Id);
                        }
                        return true;
                    }
                }
            return false;
        }
        // Test the item in the ArmBand slot, if it has changed, return true, otherwise return false
        private bool TestItemChanged(String? item)
        {
            string? itemTest = item;
            Slot slot = inventoryEquipment.GetSlot(EquipmentSlot.ArmBand);
            if (!TestSlotHasItem())
            {
                if (enableLogging)
                {
                    Log.LogInfo("No item in ArmBand slot, resetting itemToTest");
                }
                itemToTest = null;
                itemId = "0000000000"; // Reset the item ID
                return false;
            }
            if (itemTest != slot.ContainedItem.Id)
                {
                if (enableLogging)
                {
                    Log.LogInfo("Item in ArmBand slot has changed, updating itemToTest");
                }
                itemToTest = slot.ContainedItem;
                itemId = slot.ContainedItem.Id;
                return true;
            }
            return false;
        }
        // Check if the current scene is a raid scene, if so, return true, otherwise return false
        private bool testInRaidScene()
        {
            string _currentScene = getCurrentScene();
            switch (_currentScene)
            {
                case "Factory_Rework_Day_Scripts":
                case "Factory_Rework_Night_Scripts":
                case "Sandbox_Scripts":
                case "City_Scripts":
                case "Shopping_Mall_Scripts":
                case "custom_Scripts":
                case "woods_Scripts":
                case "Reserve_Base_Scripts":
                case "Lighthouse_Scripts":
                case "shoreline_scripts":
                case "Laboratory_Scripts":
                    return true;
                default: return false;
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
                    Log.LogInfo("Current scene is null or empty");
                }
                return "Unknown";
            }

            if (enableLogging)
            {
                Log.LogInfo($"Current scene: {_currentScene}");
            }

            return _currentScene;
        }
        #endregion

        #region BeltSettings
        private static EquipmentSlot[] aboveEquipmentSlots = new[]
        {
            EquipmentSlot.TacticalVest,
            EquipmentSlot.ArmBand,
            EquipmentSlot.Pockets,
            EquipmentSlot.Backpack,
            EquipmentSlot.SecuredContainer,
            EquipmentSlot.Dogtag
        };

        private static EquipmentSlot[] belowEquipmentSlots = new[]
        {
            EquipmentSlot.TacticalVest,
            EquipmentSlot.Pockets,
            EquipmentSlot.ArmBand,
            EquipmentSlot.Backpack,
            EquipmentSlot.SecuredContainer,
            EquipmentSlot.Dogtag
        };

        void SetEquipmentSlots()
        {
            if (Settings.BeltSlotLocation.Value == BeltSlotLocationOption.AbovePockets)
            {
                // Set the equipment slots to the aboveEquipmentSlots array
                typeof(ContainersPanel)
                    .GetField("equipmentSlot_0", BindingFlags.Static | BindingFlags.NonPublic)
                    .SetValue(null, aboveEquipmentSlots);
            }
            else if (Settings.BeltSlotLocation.Value == BeltSlotLocationOption.BelowPockets)
            {
                // Set the equipment slots to the belowEquipmentSlots array
                typeof(ContainersPanel)
                    .GetField("equipmentSlot_0", BindingFlags.Static | BindingFlags.NonPublic)
                    .SetValue(null, belowEquipmentSlots);
            }
        }
        #endregion

        private void Awake()
        {
            packNStrapInstalled = Chainloader.PluginInfos.Keys.Contains("com.aaaWTT-PacknStrap.Core");
            Settings.Init(Config);
            Instance = this;
            Log = Logger;
            UiMappings = new UI_Mappings();

            SetEquipmentSlots();
            new ContainersPanelPatch().Enable();
            new ContainersPanelPatch2().Enable();
            new ComplexStashPanelPatch().Enable();
            new ComplexStashPanelPatch2().Enable();
            new MainMenuControllerClassPatch().Enable();
            new EquipmentBuildsScreenPatch().Enable();
            new InventoryEquipmentPatch().Enable();
            new InventoryScreenPatch().Enable();
            //new EquipmentTabPatch().Enable();

            // Enables the correct patch based on if PackNStrap is installed or not
            if (packNStrapInstalled)
            {
                new GetPrioritizedContainersPatch().Disable();
                new GetPrioritizedContainersPackNStrapPatch().Enable();
            }
            else
            {
                new GetPrioritizedContainersPackNStrapPatch().Disable();
                new GetPrioritizedContainersPatch().Enable();
            }

        }

        void LateUpdate()
        {
            if (!Singleton<CommonUI>.Instantiated)
            {
                return;
            }
            if (!Singleton<PreloaderUI>.Instantiated)
            {
                return;
            }
            if (!inventoryScreenLoaded)
            {
                return;
            }

            UpdateArmBandSlot();

            UpdateRaidArmBandSlot();

            if (Input.GetKeyDown(KeyCode.P))
            {
                Log.LogInfo("Current Scene: " + getCurrentScene());
                Log.LogInfo("Current Screen: " + getCurrentScreen());
            }
        }

        // Open the belt slot if the inventory is open and the armband slot has a compound item
        // also sets the uiMappings if they are null
        public void OnEnterInventory()
        {
            // do not trigger if inventory screen is not focused or input field is focused
            if(isInventoryScreenFocus() && checkInventoryTab())
            {
                //if(!testInRaidScene())
                //{
                    if (UiMappings != null)
                    {
                        if (UiMappings.beltSlot == null)
                        {
                            UiMappings.setInventoryContainer_Mappings();
                        }
                        if(TestSlotHasItem())
                        {
                            //Log.LogInfo("" + getCurrentScreen());
                            RefreshBeltSlot(uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                        }
                        if (enableLogging)
                        {
                            Log.LogInfo("belt slot in the right place");
                        }
                        return;
                    }
                //}
            }
            if(getCurrentScreen() == EEftScreenType.Insurance)
            {
                if (UiMappings != null)
                {
                    if (UiMappings.insuranceBelt == null)
                    {
                        uiMappings.setInsuranceScreen_Mappings();
                    }
                    if (TestSlotHasItem())
                    {
                        RefreshBeltSlot(uiMappings.insuranceArmBand, EEftScreenType.Insurance, uiMappings.insuranceBelt);
                    }
                    if (enableLogging)
                    {
                        Log.LogInfo("belt slot in the right place");
                    }
                    return;
                }
            }
        }

        private void UpdateArmBandSlot()
        {
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                if (UiMappings.beltSlot == null)
                {
                    UiMappings.setInventoryContainer_Mappings();
                    RefreshBeltSlot(uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                }
                if (TestItemChanged(itemId))
                {
                    RefreshBeltSlot(uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                }
                return;
            }
        }

        // Needs a check for actually looting a dead bot
        private void UpdateRaidArmBandSlot()
        {
            if(!testInRaidScene())
            {
                return;
            }
            if (!complexStashPanelLoaded)
            {
                return;
            }
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                if (UiMappings.lootBeltSlot == null)
                {
                    UiMappings.setComplexLootUI_Mappings();
                    RefreshBeltSlot(uiMappings.lootArmBand, EEftScreenType.None, uiMappings.lootBeltSlot);
                }
                if (TestItemChanged(itemId))
                {
                    RefreshBeltSlot(uiMappings.lootArmBand, EEftScreenType.None, uiMappings.lootBeltSlot);
                }
                return;
            }
        }

        // Updates the armband slot and opens the belt slot if the item in the armband slot has changed
        public void SetArmbandSlotOnOpen()
        {
            if(isInventoryScreenFocus() && checkInventoryTab())
            {
                if(TestSlotHasItem())
                {
                    if (UiMappings.beltSlot == null)
                    {
                        UiMappings.setInventoryContainer_Mappings();
                        RefreshBeltSlot(uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                    }
                    if (TestItemChanged(itemId))
                    {
                        RefreshBeltSlot(uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                    }
                }
                else
                {
                    UiMappings.setInventoryContainer_Mappings();
                }
            }
        }

        public void SetRaidArmbandSlotOnOpen()
        {
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                if (TestSlotHasItem())
                {
                    if (UiMappings.lootArmBand == null)
                    {
                        UiMappings.setComplexLootUI_Mappings();
                        RefreshBeltSlot(uiMappings.lootArmBand, EEftScreenType.None, uiMappings.lootBeltSlot);
                    }
                    if (TestItemChanged(itemId))
                    {
                        RefreshBeltSlot(uiMappings.lootArmBand, EEftScreenType.None, uiMappings.lootBeltSlot);
                    }
                }
                else
                {
                    UiMappings.setComplexLootUI_Mappings();
                }
            }
        }

        public void SetInsuranceArmbandSlot()
        {
                if (UiMappings.insuranceBelt == null)
                {
                    UiMappings.setInsuranceScreen_Mappings();
                    RefreshBeltSlot(uiMappings.insuranceArmBand, EEftScreenType.Insurance, uiMappings.insuranceBelt);
                }
                if (TestItemChanged(itemId))
                {
                    RefreshBeltSlot(uiMappings.insuranceArmBand, EEftScreenType.Insurance, uiMappings.insuranceBelt);
                }
        }

        // Refreshes the belt slot if the item in the armband slot has not changed but the belt slot is not open
        private void RefreshBeltSlot(GameObject targetArm, EEftScreenType screenType, GameObject targetBelt)
        {
            if (TestItemIsCompound())
            {
                 UiMappings.toggleArmBandSlotFull(iconToggle, screenType, targetArm);
                 UiMappings.toggleBeltSlotFull(!iconToggle, screenType, targetBelt);
            }
            else
            {
                 UiMappings.toggleArmBandSlotFull(!iconToggle, screenType, targetArm);
                 UiMappings.toggleBeltSlotFull(iconToggle, screenType, targetBelt);
            }
            return;
        }
    }
}
