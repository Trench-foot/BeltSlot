using BeltSlot.Helpers;
using BeltSlot.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
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
        public bool enableLogging = false;
        public bool packNStrapInstalled;
        public bool iconToggle = true;
        public bool inventoryScreenLoaded = false;
        public bool complexStashPanelLoaded = false;
        public string? itemId = "0000000000";
        public string? raidItemId = "0000000000";
        public Item? itemToTest;
        public Item? raidItemToTest;
        public Slot armbandSlot;
        public Slot raidArmbandSlot;
        private string player = "0000000000";
        public EPlayerSide side;
        public GClass1331 corpseJson;
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
        private bool TestSlotHasItem(Slot _slot)
        {
            if (inventoryEquipment != null)
            {
                //Slot slot = inventoryEquipment.GetSlot(EquipmentSlot.ArmBand);
                Slot slot = _slot;
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
        public bool TestItemIsCompound(Slot _slot)
        {
            if (inventoryEquipment != null)
            {
                //Slot slot = inventoryEquipment.GetSlot(EquipmentSlot.ArmBand);
                Slot slot = _slot;
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
        private bool TestItemChanged(String? item, Slot _slot)
        {
            string? itemTest = item;
            Slot slot = _slot;
            string owner1 = player; // Check if owner is corpse or player
            string owner2 = slot.ParentItem.Id; // Check if owner is corpse or player

            if(enableLogging)
            {
                Log.LogInfo("Owner1: " + owner1);
                Log.LogInfo("Owner2: " + owner2);
            }
            if(owner1 == owner2)
            {
                if (!TestSlotHasItem(slot))
                {
                    if (enableLogging)
                    {
                        Log.LogInfo("No item in ArmBand slot, resetting itemToTest");
                    }
                    itemToTest = null;
                    itemId = "0000000000"; // Reset the item ID
                    return false;
                }
                else if (itemTest != slot.ContainedItem.Id)
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
            else
            {
                if (!TestSlotHasItem(slot))
                {
                    if (enableLogging)
                    {
                        Log.LogInfo("No item in ArmBand slot, resetting itemToTest");
                    }
                    raidItemToTest = null;
                    raidItemId = "0000000000"; // Reset the item ID
                    return false;
                }
                else if (itemTest != slot.ContainedItem.Id)
                    {
                    if (enableLogging)
                    {
                        Log.LogInfo("Item in ArmBand slot has changed, updating itemToTest");
                    }
                    raidItemToTest = slot.ContainedItem;
                    raidItemId = slot.ContainedItem.Id;
                    return true;
                }
                return false;
            }
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

        private bool testGameReady()
        {
            if (!Singleton<CommonUI>.Instantiated)
            {
                return false;
            }
            if (!Singleton<PreloaderUI>.Instantiated)
            {
                return false;
            }
            if (!inventoryScreenLoaded)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Belt Settings
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
            new ItemUiContextPatch().Enable();
            new EquipmentBuildsScreenPatch().Enable();
            new EquipmentBuildsScreenPatch2().Enable();
            new InventoryEquipmentPatch().Enable();
            new InventoryScreenPatch().Enable();
            new ItemViewPatch().Enable();
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

            if (Input.GetKeyDown(KeyCode.V))
            { 
                IItemOwner id = armbandSlot.ContainedItem.Parent.GetOwner(); // Ensure the item is owned by the player
                string stuff = id.ID;
                Log.LogInfo("Item in ArmBand slot: " + stuff);
            }

            
            if (enableLogging)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    Log.LogInfo("Current Scene: " + getCurrentScene());
                    Log.LogInfo("Current Screen: " + getCurrentScreen());
                }
            }
        }

        // Updates the armband and belt slots dynamically when inventory is open and not in raid
        // Needs a check to make sure the item being interacted with is owned by the player for in raid
        public void UpdateArmBandSlot()
        {
            if (!testGameReady())
            {
                return;
            }
            //if (testInRaidScene())
            //{
            //    return;
            //}
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                //if (EEftScreenType.EquipmentBuilds == getCurrentScreen())
                //{
                //    SetBuildsArmbandSlot();
                //}
                Slot _slot = UiMappings.setInventoryContainer_Mappings();
                if (TestSlotHasItem(_slot))
                {
                    RefreshBeltSlot(_slot, uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                    if (TestItemChanged(itemId, _slot))
                    {
                        Log.LogInfo("Item in ArmBand slot has changed, updating belt slot");
                        RefreshBeltSlot(_slot, uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                    }
                    return;
                }
                return;
            }
            return;
        }

        // Updates the bot armband and belt slots dynamically when looting in raid
        // Just realized that these Slot and Item checks don't specifically target the body being looted
        // but that of the player, need to fix if I want to use this
        public void UpdateRaidArmBandSlot()
        {
            if (!testGameReady())
            {
                return;
            }
            if (!testInRaidScene())
            {
                return;
            }
            if (!complexStashPanelLoaded)
            {
                return;
            }
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                Slot _slot = UiMappings.setComplexLootUI_Mappings();
                if (_slot == null)
                {
                    complexStashPanelLoaded = false;
                    return;
                }
                if (TestSlotHasItem(_slot))
                {
                    RefreshBeltSlot(_slot, uiMappings.lootArmBand, EEftScreenType.None, uiMappings.lootBeltSlot);
                    if (TestItemChanged(raidItemId, _slot))
                    {
                        RefreshBeltSlot(_slot, uiMappings.lootArmBand, EEftScreenType.None, uiMappings.lootBeltSlot);
                    }
                    return;
                }
                return;
            }
            return;
        }

        // Sets the armband and belt slots when the inventory is opened
        public void SetArmbandSlotOnOpen()
        {
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                Slot _slot = UiMappings.setInventoryContainer_Mappings();
                player = _slot.ParentItem.Id;
                if (TestSlotHasItem(_slot))
                {
                    RefreshBeltSlot(_slot, uiMappings.armBandSlot, EEftScreenType.Inventory, uiMappings.beltSlot);
                    itemToTest = _slot.ContainedItem;
                    itemId = _slot.ContainedItem.Id;
                }
                else
                {                     
                    itemToTest = null; // Reset the item ID
                    itemId = "0000000000"; // Reset the item ID
                }
            }
        }

        // Sets the bot armband and belt slots when the looting screen is opened
        public void SetRaidArmbandSlotOnOpen()
        {
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                if(!complexStashPanelLoaded)
                {
                    return;
                }
                Slot _slot = UiMappings.setComplexLootUI_Mappings();
                if (_slot == null)
                {
                    complexStashPanelLoaded = false;
                    return;
                }
                if (TestSlotHasItem(_slot))
                {
                    RefreshBeltSlot(_slot, uiMappings.lootArmBand, EEftScreenType.None, uiMappings.lootBeltSlot);
                    raidItemToTest = _slot.ContainedItem;
                    raidItemId = _slot.ContainedItem.Id;
                }
                else
                {
                    raidItemToTest = null; // Reset the item ID
                    raidItemId = "0000000000"; // Reset the item ID
                }
            }
        }

        // Sets the armband and belt slots when the insurance screen is opened
        public void SetInsuranceArmbandSlot()
        {

            RefreshBeltSlot(UiMappings.setInsuranceScreen_Mappings(), uiMappings.insuranceArmBand, EEftScreenType.Insurance, uiMappings.insuranceBelt);
        }

        // Sets the armband and belt slots when the builds screen is opened
        public void SetBuildsArmbandSlot()
        {
            UiMappings.setBuildPanel_Mappings();
        }

        // Sets the armband and belt slots when the time has come screen is opened
        public void SetDeployArmbandSlot()
        {
            RefreshBeltSlot(UiMappings.setDeployPanel_Mappings(), uiMappings.deployArmbandSlot, EEftScreenType.TimeHasCome, uiMappings.deployBeltSlot);
        }

        // Refreshes the armband and belt slots
        private void RefreshBeltSlot(Slot _slot, GameObject targetArm, EEftScreenType screenType, GameObject targetBelt)
        {
            if (TestItemIsCompound(_slot))
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
