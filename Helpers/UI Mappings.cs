using Comfort.Common;
using EFT.UI;
using EFT.UI.DragAndDrop;
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
        public GameObject lootContainerGameObject = null;
        public GameObject slotTemplate = null;
        public GameObject beltSlot = null;
        public GameObject armBandSlot = null;
        public GameObject healthParameter = null;
        public GameObject healthPanel = null;
        public GameObject windowsPlaceHolder = null;
        public GameObject windowCloseButton = null;
        public ToggleButton toggleButton = null;
        public InventoryScreen inventoryScreen = null;
        public PreloaderUI preloaderUI = null;
        public bool noActiveWindow = false;

        public GameObject[] windowsPlaceHolderArray = null;
        #endregion

        #region Game Object Mappings
        // Mappings of container view in the inventory screen
        public void setContainer_Mappings()
        {
            if(inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            containerGameObject = inventoryScreen.transform.Find("Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content").gameObject;
            slotTemplate = containerGameObject.transform.Find("Slot Template").gameObject;
        }

        public void setHealthPanel_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            healthPanel = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel/Health Panel").gameObject;
            healthParameter = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel/Health Parameters").gameObject;
        }

        // Mappings of preloader UI elements
        public void setPrelaoderUI_Mappings()
        {
            if(preloaderUI == null)
            {
                preloaderUI = Singleton<PreloaderUI>.Instance;
            }
            windowsPlaceHolder = preloaderUI.transform.Find("Preloader UI/UIContext/WindowsPlaceholder").gameObject;
        }

        // Mappings of complex loot container view in the inventory screen, currently not used
        public void setComplexLootUI_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            lootContainerGameObject = inventoryScreen.transform.Find("Items Panel/Items Panel/Stash Panel/Complex Loot Panel/Containers Scrollview/Content").gameObject;
        }

        // Mappings of the armband slot
        public void setArmBand_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            armBandSlot = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel/Gear Panel/ArmBand Slot").gameObject;
        }

        // Mappings of the belt slot
        public void setBeltSlot_Mappings()
        {
            beltSlot = containerGameObject.transform.Find("ArmBand Slot").gameObject;
        }
        #endregion

        // Set the settings of the belt slot, such as its position, name, and visibility
        public void setBeltSlot_Settings()
        {
            if(beltSlot == null)
            {
                setBeltSlot_Mappings();
            }
            if(beltSlot != null)
            {
                GameObject _headerPanel = beltSlot.transform.GetChild(0).gameObject; // Header panel of the belt slot
                GameObject _slotPanel = beltSlot.transform.GetChild(1).gameObject; // Slot panel of the belt slot
                GameObject _slotViewHeader = _headerPanel.transform.GetChild(1).gameObject; // Slot view header of the belt slot
                GameObject _slotName = _slotViewHeader.transform.GetChild(2).gameObject; // Slot name of the belt slot

                _slotName.GetComponent<TextMeshProUGUI>().text = "BELT"; // Set the slot name to "BELT"
            }
        }

        public void toggleBeltSlotFull(bool full)
        {
            if (beltSlot == null)
            {
                setBeltSlot_Mappings();
            }
            if (beltSlot != null)
            {
                GameObject _slotPanel = beltSlot.transform.GetChild(1).gameObject; // Slot panel of the belt slot

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
        }

        public void toggleArmBandSlotFull(bool full)
        {
            if (armBandSlot == null)
            {
                setArmBand_Mappings();
            }
            if (armBandSlot != null)
            {
                GameObject _slotPanel = armBandSlot.transform.GetChild(1).gameObject; // Slot panel of the armband slot
                if (armBandSlot.transform.childCount > 8)
                {
                    GameObject _backImage = armBandSlot.transform.GetChild(4).gameObject; // Back image of the armband slot
                    GameObject _backGround = armBandSlot.transform.GetChild(5).gameObject; // Background of the armband slot
                    GameObject _emptyBorder = armBandSlot.transform.GetChild(6).gameObject; // Empty border of the armband slot
                    GameObject _fullBorder = armBandSlot.transform.GetChild(7).gameObject; // Full border of the armband slot
                    GameObject _slotLayout = armBandSlot.transform.GetChild(8).gameObject; // Slot layout of the armband slot
                    
                    _backImage.SetActive(full); // Show the back image
                    _backGround.SetActive(full); // Show the background
                    _emptyBorder.SetActive(full); // Show the empty border
                    _fullBorder.SetActive(!full); // Hide the full border
                    _slotLayout.SetActive(!full); // Hide the slot layout
                }
            }
        }
    }
}
