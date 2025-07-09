using BeltSlot.Helpers;
using BeltSlot.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static EFT.UI.InventoryScreen;

namespace BeltSlot
{
    [BepInPlugin("BeltSlot", "BeltSlot", "0.9.9")]
    [BepInDependency("com.SPT.core", "3.11.0")]
    [BepInDependency("com.aaaWTT-PacknStrap.Core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private bool enableLogging = true;
        public bool packNStrapInstalled;
        public bool iconToggle = true;
        public bool inventoryScreenLoaded = false;
        public string? itemId = "0000000000";
        public Item? itemToTest;
        internal static Plugin Instance { get; set; }
        internal ManualLogSource Log { get; set; }
        public InventoryEquipment inventoryEquipment;
        public InventoryScreen inventoryScreen;
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
                    Logger.LogInfo("Inventory screen is focused.");
                }
                return true;
            }
            return false;
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
        private bool TestArmBandHasItem()
        {
            if (inventoryEquipment != null)
            {
                if (uiMappings?.armBandSlot == null)
                {
                    uiMappings?.setArmBand_Mappings();
                }
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
            return false;
        }
        // Test the ArmBand slot, if it has a compound item, return true, otherwise return false
        private bool TestArmBandCompound()
        {
                if (inventoryEquipment != null)
                {
                    if (uiMappings?.armBandSlot == null)
                    {
                        uiMappings?.setArmBand_Mappings();
                    }
                    Slot slot = inventoryEquipment.GetSlot(EquipmentSlot.ArmBand);
                    //if (!TestArmBandHasItem())
                    //{
                    //    return false;
                    //}
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
            Slot? slot = inventoryEquipment?.GetSlot(EquipmentSlot.ArmBand);
            if (!TestArmBandHasItem())
            {
                if (enableLogging)
                {
                    Log.LogInfo("No item in ArmBand slot, resetting itemToTest");
                }
                itemToTest = null;
                itemId = "0000000000"; // Reset the item ID
                return false;
            }
            if (itemTest != slot?.ContainedItem.Id)
                {
                if (enableLogging)
                {
                    Log.LogInfo("Item in ArmBand slot has changed, updating itemToTest");
                }
                itemToTest = slot?.ContainedItem;
                itemId = slot?.ContainedItem.Id;
                return true;
            }
            return false;
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

            OnEnterInventory();

            UpdateArmbandSlot();

            if (Input.GetKeyDown(KeyCode.M))
            {
                UiMappings.setContainer_Mappings();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                UiMappings.setBeltSlot_Settings();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                iconToggle = !iconToggle;
                UiMappings.toggleArmBandSlotFull(!iconToggle);
                UiMappings.toggleBeltSlotFull(iconToggle);
            }
        }

        // Open the belt slot if the inventory is open and the armband slot has a compound item
        // also sets the uiMappings if they are null
        private void OnEnterInventory()
        {
            //EInventoryTab currentTab == EInventoryTab.Health;
            // do not trigger if inventory screen is not focused or input field is focused
            if (isInventoryScreenFocus() && checkInventoryTab())
            {
                if (UiMappings != null)
                {
                    if (UiMappings.beltSlot == null)
                    {
                        UiMappings.setContainer_Mappings();
                        UiMappings.setBeltSlot_Settings();
                    }
                    if(TestArmBandHasItem())
                    {
                        RefreshBeltSlot();
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
            if(isInventoryScreenFocus() && checkInventoryTab())
            {
                if (UiMappings.beltSlot == null)
                {
                    UiMappings.setContainer_Mappings();
                    UiMappings.setBeltSlot_Settings();
                };
                if (TestItemChanged(itemId))
                {
                    RefreshBeltSlot();
                }
            }
        }

        // Refreshes the belt slot if the item in the armband slot has not changed but the belt slot is not open
        private void RefreshBeltSlot()
        {
            if (TestArmBandCompound())
            {
                 UiMappings.toggleArmBandSlotFull(iconToggle);
                 UiMappings.toggleBeltSlotFull(!iconToggle);
            }
            else
            {
                 UiMappings.toggleArmBandSlotFull(!iconToggle);
                 UiMappings.toggleBeltSlotFull(iconToggle);
            }
            return;
        }
    }
}
