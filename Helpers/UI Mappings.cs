using EFT.UI;
using EFT.UI.DragAndDrop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeltSlot.Helpers
{
    internal class UI_Mappings
    {
        #region Variables
        // Main Menu UI Elements
        public GameObject? containerGameObject = null;
        public GameObject? slotTemplate = null;
        public GameObject? dogtagTemplate = null;
        public GameObject? compassTemplate = null;
        public GameObject? tacticalVestSlot = null;
        public GameObject? beltSlot = null;
        public GameObject? pocketsSlot = null;
        public GameObject? backpackSlot = null;
        public GameObject? securedContainerSlot = null;
        public GameObject? armBandSlot = null;
        public GameObject? windowsPlaceHolder = null;
        public GameObject? windowCloseButton = null;

        public GameObject[] defaultContainersViewArray = new GameObject[7];
        public GameObject[] newContainersViewArray = new GameObject[8];
        public GameObject[]? windowsPlaceHolderArray = null;
        #endregion


        public void setContainer_Mappings()
        {
            containerGameObject = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content")?.gameObject;
            slotTemplate = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content/Slot Template")?.gameObject;
            dogtagTemplate = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content/DogtagTemplate")?.gameObject;
            compassTemplate = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content/CompassTemplate")?.gameObject;
            tacticalVestSlot = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content/TacticalVest Slot")?.gameObject;
            pocketsSlot = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content/Pockets Slot")?.gameObject;
            backpackSlot = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content/Backpack Slot")?.gameObject;
            securedContainerSlot = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Containers Panel/Scrollview Parent/Containers Scrollview/Content/SecuredContainer Slot")?.gameObject;
        }

        public void setEquipment_Mappings()
        {
            armBandSlot = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/LeftSide/Left Panel/Gear Panel/ArmBand Slot")?.gameObject;
        }

        public void setPrelaoderUI_Mappings()
        {
            windowsPlaceHolder = GameObject.Find("Preloader UI/Preloader UI/UIContext/WindowsPlaceholder")?.gameObject;
            windowCloseButton = GameObject.Find("Preloader UI/Preloader UI/UIContext/WindowsPlaceholder/WindowCloseButton")?.gameObject;
        }

        public void setBeltSlot_Mappings()
        {

            beltSlot = GameObject.Instantiate(slotTemplate, containerGameObject.transform);

        }

        public void setDefaultContainersArray()
        {
            defaultContainersViewArray[0] = slotTemplate;
            defaultContainersViewArray[1] = dogtagTemplate;
            defaultContainersViewArray[2] = compassTemplate;
            defaultContainersViewArray[3] = tacticalVestSlot;
            defaultContainersViewArray[4] = pocketsSlot;
            defaultContainersViewArray[5] = backpackSlot;
            defaultContainersViewArray[6] = securedContainerSlot;
        }

        public void setNewContainersArray()
        {
            newContainersViewArray[0] = slotTemplate;
            newContainersViewArray[1] = dogtagTemplate;
            newContainersViewArray[2] = compassTemplate;
            newContainersViewArray[3] = tacticalVestSlot;
            newContainersViewArray[4] = beltSlot;
            newContainersViewArray[5] = pocketsSlot;
            newContainersViewArray[6] = backpackSlot;
            newContainersViewArray[7] = securedContainerSlot;
        }

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

        public GameObject getGridWindowClone()
        {
            setWindowsPlaceHolderArray();
            /*if ((windowsPlaceHolderArray?.Length) < 16)
            {
                return null; // Ensure there are enough windows to access the grid window
            }*/
            GameObject? gridWindowClone = windowsPlaceHolder?.transform.GetChild(16).gameObject; // Get the first child of the windows placeholder, which is the grid window template
            gridWindowClone = windowsPlaceHolderArray?[windowsPlaceHolderArray.Length - 1];
            //GameObject? captionPanel = gridWindowClone?.transform.GetChild(3).gameObject; // Get the close button of the grid window
            //GameObject? closeButton = captionPanel?.transform.GetChild(6).gameObject; // Get the close button of the grid window

            return gridWindowClone;
        }
        #region Button Setters

        #endregion

        public Button getCloseButton(GameObject button)
        {
            GameObject captionPanel = button.transform.GetChild(3).gameObject; // Get the close button of the grid window
            GameObject closeButton = captionPanel.transform.GetChild(6).gameObject; // Get the close button of the grid window
            return closeButton.GetComponentInChildren<Button>();
        }
        #region Button Getters
        /*public DefaultUIButton getButton(GameObject button)
        {
            return button.GetComponentInChildren<DefaultUIButton>();
        }

        public Image getBackground(GameObject button)
        {
            return button.transform.GetChild(1).GetComponent<Image>();
        }

        public TextMeshProUGUI getLabel(GameObject button)
        {
            GameObject _sizeLabel = button.transform.GetChild(2).gameObject;
            return _sizeLabel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        }

        public Image getIcon(GameObject button)
        {
            GameObject _sizeLabel = button.transform.GetChild(2).gameObject;
            GameObject _iconContainer = _sizeLabel.transform.GetChild(1).gameObject;
            return _iconContainer.transform.GetChild(1).GetComponent<Image>();
        }*/
        #endregion
    }
}
