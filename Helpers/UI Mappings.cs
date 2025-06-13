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
        public GameObject? lootContainerGameObject = null;
        public GameObject slotTemplate = null;
        //public GameObject? dogtagTemplate = null;
        //public GameObject? compassTemplate = null;
        //public GameObject? tacticalVestSlot = null;
        public GameObject? beltSlot = null;
        //public GameObject? pocketsSlot = null;
        //public GameObject? backpackSlot = null;
        //public GameObject? securedContainerSlot = null;
        public GameObject armBandSlot = null;
        public GameObject? windowsPlaceHolder = null;
        public GameObject? windowCloseButton = null;
        public InventoryScreen? inventoryScreen = null;
        public PreloaderUI? preloaderUI = null;

        public GameObject[]? windowsPlaceHolderArray = null;
        #endregion

        #region Game Object Mappings
        // Mappings of container view in the inventory screen
        public void setContainer_Mappings()
        {
            if(inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }

            //containerGameObject = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content")?.gameObject;
            containerGameObject = inventoryScreen.transform.Find("Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content").gameObject;
            slotTemplate = containerGameObject.transform.Find("Slot Template").gameObject;
            //dogtagTemplate = containerGameObject?.transform.Find("Dogtag Template")?.gameObject;
            //compassTemplate = containerGameObject?.transform.Find("Compass Template")?.gameObject;
            //tacticalVestSlot = containerGameObject?.transform.Find("TacticalVest Slot")?.gameObject;
            //pocketsSlot = containerGameObject?.transform.Find("Pockets Slot")?.gameObject;
            //backpackSlot = containerGameObject?.transform.Find("Backpack Slot")?.gameObject;
            //securedContainerSlot = containerGameObject?.transform.Find("SecuredContainer Slot")?.gameObject;
        }

        // Mappings of equipment view in the inventory screen, currently just the armband slot
        public void setEquipment_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            GameObject leftPanel = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel").gameObject;
            //armBandSlot = inventoryScreen.transform.Find("Items Panel/LeftSide/Left Panel/Gear Panel/ArmBand Slot").gameObject;
            armBandSlot = leftPanel.transform.Find("Gear Panel/ArmBand Slot").gameObject;
        }

        // Mappings of preloader UI elements
        public void setPrelaoderUI_Mappings()
        {
            if(preloaderUI == null)
            {
                preloaderUI = Singleton<PreloaderUI>.Instance;
            }
            //windowsPlaceHolder = GameObject.Find("Preloader UI/Preloader UI/UIContext/WindowsPlaceholder")?.gameObject;
            windowsPlaceHolder = preloaderUI.transform.Find("Preloader UI/UIContext/WindowsPlaceholder")?.gameObject;
            windowCloseButton = preloaderUI.transform.Find("Preloader UI/UIContext/WindowsPlaceholder/WindowCloseButton")?.gameObject;
        }

        // Mappings of complex loot container view in the inventory screen, currently not used
        public void setComplexLootUI_Mappings()
        {
            if (inventoryScreen == null)
            {
                inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            }
            lootContainerGameObject = inventoryScreen.transform.Find("Items Panel/Items Panel/Stash Panel/Complex Loot Panel/Containers Scrollview/Content")?.gameObject;
        }

        // Mappings of the belt slot, which is instantiated from the slot template
        public void setBeltSlot_Mappings()
        {

            beltSlot = GameObject.Instantiate(slotTemplate, containerGameObject?.transform);

        }
        #endregion

        // Set the windows placeholder array by getting all the child GameObjects of the windows placeholder GameObject
        public void setWindowsPlaceHolderArray()
        {
            // Create a new array to hold the GameObjects
            windowsPlaceHolderArray = new GameObject[windowsPlaceHolder.transform.childCount];

            // Loop through the transforms and add the corresponding GameObject to the array
            for (int i = 0; i < windowsPlaceHolder.transform.childCount; i++)
            {
                windowsPlaceHolderArray[i] = windowsPlaceHolder.transform.GetChild(i).gameObject;
            }
        }

        // Set the settings of the belt slot, such as its position, name, and visibility
        public void setBeltSlot_Settings()
        {
            if(beltSlot == null)
            {
                setBeltSlot_Mappings();
            }
            if(beltSlot != null)
            {
                beltSlot.transform.SetSiblingIndex(4); // Set the belt slot to be the 4th item in the container list
                GameObject _headerPanel = beltSlot.transform.GetChild(0).gameObject; // Header panel of the belt slot
                GameObject _slotPanel = beltSlot.transform.GetChild(1).gameObject; // Slot panel of the belt slot
                GameObject _slotViewHeader = _headerPanel.transform.GetChild(1).gameObject; // Slot view header of the belt slot
                GameObject _slotName = _slotViewHeader.transform.GetChild(2).gameObject; // Slot name of the belt slot

                _slotPanel.SetActive(false); // Hide the slot panel
                _slotViewHeader.GetComponent<SlotViewHeader>().Interactable = false; // Make the slot view header not interactable
                _slotViewHeader.transform.GetChild(3).gameObject.SetActive(false); // Hide the arrow icon
                _slotName.GetComponent<TextMeshProUGUI>().text = "BELT"; // Set the slot name to "BELT"

                beltSlot.SetActive(true);
            }
        }

        // Finds the grid window clone, moves the grids to the belt slot, and hides the window clone
        public void setBeltSlotGrid()
        {
            setWindowsPlaceHolderArray();
            if((windowsPlaceHolderArray?.Length) <= 15)
            {
                return;
            }
            GameObject windowClone = getGridWindowClone();
            if(windowClone == null)
            {
                return;
            }
            if (windowClone.transform.childCount <= 6)
            {
                return;
            }
            GameObject beltGrid = windowClone.transform.GetChild(6).gameObject; // Get the sixth child of the window clone, which is the grid window

            beltGrid.transform.parent = beltSlot?.transform; // Set the parent of the grid to the belt slot
            windowClone.SetActive(false); // Hide the window clone
        }

        // Get the grid window clone from the windows placeholder array
        public GameObject getGridWindowClone()
        {
            setWindowsPlaceHolderArray();
            GameObject? gridWindowClone = windowsPlaceHolder?.transform.GetChild(16).gameObject; // Get the first child of the windows placeholder, which is the grid window template
            gridWindowClone = windowsPlaceHolderArray?[windowsPlaceHolderArray.Length - 1];

            return gridWindowClone;
        }

        // Get the close button of the grid window
        public Button getCloseButton(GameObject button)
        {
            GameObject captionPanel = button.transform.GetChild(3).gameObject; // Get the close button of the grid window
            GameObject closeButton = captionPanel.transform.GetChild(6).gameObject; // Get the close button of the grid window
            return closeButton.GetComponentInChildren<Button>();
        }

        private static async Task pauseWait(int wait)
        {
            // A pause method that waits for a specified amount of time
            await Task.Delay(wait);
        }
    }
}
