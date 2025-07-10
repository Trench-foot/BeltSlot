using Comfort.Common;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFT.UI.Insurance;
using EFT.UI.Matchmaker;
using EFT.UI.Screens;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeltSlot.Helpers
{
    internal class UI_Mappings
    {
        #region Variables
        // Main Menu UI Elements
        public GameObject containerGameObject = null;
        public GameObject equipmentGameObject = null;
        public GameObject lootContainerGameObject = null;
        public GameObject lootEquipmentGameObject = null;
        public GameObject lootArmBand = null;
        public GameObject lootBeltSlot = null;
        public GameObject slotTemplate = null;
        public GameObject beltSlot = null;
        public GameObject armBandSlot = null;
        public GameObject healthParameter = null;
        public GameObject healthPanel = null;
        public GameObject insuranceScreenContainer = null;
        public GameObject insuranceScreenGearPanel = null;
        public GameObject insuranceBelt = null;
        public GameObject insuranceArmBand = null;
        public GameObject buildPanel = null;
        public GameObject buildBeltSlot = null;
        public ToggleButton toggleButton = null;
        public InventoryScreen inventoryScreen = null;
        public EquipmentBuildsScreen equipmentBuildsScreen = null;
        public MatchmakerInsuranceScreen insuranceScreen = null;
        public PreloaderUI preloaderUI = null;
        public bool noActiveWindow = false;

        public GameObject[] windowsPlaceHolderArray = null;
        #endregion

        #region Game Object Mappings

        // Mappings of the health panel in the inventory screen
        public void setHealthPanel_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            healthPanel = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel/Health Panel").gameObject;
            healthParameter = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel/Health Parameters").gameObject;
        }

        public void setBuildPanel_Mappings()
        {
            if (equipmentBuildsScreen == null)
            {
                equipmentBuildsScreen = Singleton<MenuUI>.Instance.EquipmentBuildsScreen;
            }
            buildPanel = equipmentBuildsScreen.transform.Find("Panels/Gear Panel/ViewPanel/Containers Panel/Containers Scrollview").gameObject;
            buildBeltSlot = buildPanel.transform.Find("Content/ArmBand Slot").gameObject;
            setBeltSlot_Settings(buildBeltSlot);
        }

        // Mappings of the inventory screen
        public void setInventoryContainer_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            containerGameObject = inventoryScreen.transform.Find("Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content").gameObject;
            equipmentGameObject = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel/Gear Panel").gameObject;
            armBandSlot = equipmentGameObject.transform.Find("ArmBand Slot").gameObject;
            beltSlot = containerGameObject.transform.Find("ArmBand Slot").gameObject;
            setBeltSlot_Settings(beltSlot);
        }

        // Mappings of the insurance screen in the matchmaker
        public void setInsuranceScreen_Mappings()
        {
            if(insuranceScreen == null)
            {
                insuranceScreen = Singleton<MenuUI>.Instance.MatchmakerInsuranceScreen;
            }
            insuranceScreenContainer = insuranceScreen.transform.Find("ItemsPanel/Complex Loot Panel/Containers Scrollview/Content").gameObject;
            insuranceScreenGearPanel = insuranceScreenContainer.transform.Find("Gear Panel Template(Clone)").gameObject;
            insuranceArmBand = insuranceScreenGearPanel.transform.Find("ArmBand Slot").gameObject;
            insuranceBelt = insuranceScreenContainer.transform.Find("ArmBand Slot").gameObject;
            setBeltSlot_Settings(insuranceBelt);
        }

        // Mappings of complex loot container view in the inventory screen, currently not used
        public void setComplexLootUI_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            lootContainerGameObject = inventoryScreen.transform.Find("Items Panel/Stash Panel/Complex Loot Panel/Containers Scrollview/Content").gameObject;
            if(countTransformChildren(lootContainerGameObject) < 5)
            {
                Plugin.Instance.complexStashPanelLoaded = false;
                return;
            }
            lootEquipmentGameObject = lootContainerGameObject.transform.Find("Gear Panel Template(Clone)").gameObject;
            lootArmBand = lootEquipmentGameObject.transform.Find("ArmBand Slot").gameObject;
            lootBeltSlot = lootContainerGameObject.transform.Find("ArmBand Slot").gameObject;
            setBeltSlot_Settings(lootBeltSlot);
        }

        #endregion

        public int countTransformChildren(GameObject target)
        {
            if (target == null)
            {
                Plugin.Instance.Log.LogError("[Belt Slots] Target GameObject is null.");
                return 0;
            }
            return target.transform.childCount;
        }
        // Set the settings of the belt slot, such as its position, name, and visibility
        public void setBeltSlot_Settings(GameObject targetBelt)
        {
            Plugin.Instance.Log.LogInfo($"[Belt Slots] setBeltSlot_Settings called for {targetBelt.name}");
            if (targetBelt != null)
            {
                GameObject _headerPanel = targetBelt.transform.GetChild(0).gameObject; // Header panel of the belt slot
                GameObject _slotPanel = targetBelt.transform.GetChild(1).gameObject; // Slot panel of the belt slot
                GameObject _slotViewHeader = _headerPanel.transform.GetChild(1).gameObject; // Slot view header of the belt slot
                GameObject _slotName = _slotViewHeader.transform.GetChild(2).gameObject; // Slot name of the belt slot

                _slotName.GetComponent<TextMeshProUGUI>().text = "BELT"; // Set the slot name to "BELT"
            }
            else
            {
                return;
            }
        }

        public void toggleBeltSlotFull(bool full, EEftScreenType currentScreen, GameObject target)
        {
            switch (currentScreen)
            {
                case EEftScreenType.Inventory:
                    if (beltSlot == null)
                    {
                        setInventoryContainer_Mappings();
                    }
                    break;
                case EEftScreenType.Insurance:
                    if (insuranceBelt == null)
                    {
                        setInsuranceScreen_Mappings();
                    }
                    break;
                case EEftScreenType.None:
                    if (lootBeltSlot == null)
                    {
                        setComplexLootUI_Mappings();
                    }
                    break;
                default:
                    return; // Exit if the current screen is not Inventory or MatchmakerInsurance
            }

            GameObject _slotPanel = target.transform.GetChild(1).gameObject; // Slot panel of the belt slot
            if(_slotPanel.transform.childCount >5)
            {
                GameObject _backImage = _slotPanel.transform.GetChild(0).gameObject; // Back image of the belt slot
                GameObject _backGround = _slotPanel.transform.GetChild(1).gameObject; // Background of the belt slot
                GameObject _emptyBorder = _slotPanel.transform.GetChild(2).gameObject; // Empty border of the belt slot
                GameObject _fullBorder = _slotPanel.transform.GetChild(3).gameObject; // Full border of the belt slot
                GameObject _slotLayout = _slotPanel.transform.GetChild(4).gameObject; // Slot layout of the belt slot
                    
                _backImage.SetActive(full); // Show the back image
                _backGround.SetActive(full); // Show the background
                _emptyBorder.SetActive(full); // Show the empty border
                _fullBorder.SetActive(!full); // Hide the full border
                _slotLayout.SetActive(!full); // Hide the slot layout
            }    
        }

        public void toggleArmBandSlotFull(bool full, EEftScreenType currentScreen, GameObject target)
        {
            switch (currentScreen)
            {
                case EEftScreenType.Inventory:
                    if (armBandSlot == null)
                    {
                        setInventoryContainer_Mappings();
                    }
                    break;
                case EEftScreenType.Insurance:
                    if (insuranceArmBand == null)
                    {
                        setInsuranceScreen_Mappings();
                    }
                    break;
                case EEftScreenType.None:
                    if (lootArmBand == null)
                    {
                        setComplexLootUI_Mappings();
                    }
                    break;
                default:
                    return; // Exit if the current screen is not Inventory or MatchmakerInsurance
            }

            GameObject _slotPanel = target.transform.GetChild(1).gameObject; // Slot panel of the armband slot
            if (target.transform.childCount > 8)
            {
                GameObject _backImage = target.transform.GetChild(4).gameObject; // Back image of the armband slot
                GameObject _backGround = target.transform.GetChild(5).gameObject; // Background of the armband slot
                GameObject _emptyBorder = target.transform.GetChild(6).gameObject; // Empty border of the armband slot
                GameObject _fullBorder = target.transform.GetChild(7).gameObject; // Full border of the armband slot
                GameObject _slotLayout = target.transform.GetChild(8).gameObject; // Slot layout of the armband slot
                    
                _backImage.SetActive(full); // Show the back image
                _backGround.SetActive(full); // Show the background
                _emptyBorder.SetActive(full); // Show the empty border
                _fullBorder.SetActive(!full); // Hide the full border
                _slotLayout.SetActive(!full); // Hide the slot layout
            }
        }
    }
}
