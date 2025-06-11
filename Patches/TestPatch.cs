using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using InventoryInteractions = GClass3471; // There are two child versions?
using BeltSlot.Helpers;

namespace BeltSlot.Patches
{
    // Create the submenu options (inventory screen)
    public class TestPatch : ModulePatch
    {
        private static bool LoadingInsuranceActions = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InventoryInteractions), nameof(InventoryInteractions.CreateSubInteractions));
        }

        [PatchPrefix]
        public static bool Prefix(
            EItemInfoButton parentInteraction,
            ISubInteractions subInteractionsWrapper,
            Item ___item_0,
            ItemContextAbstractClass ___itemContextAbstractClass,
            ItemUiContext ___itemUiContext_1)
        {
            // Clear this, since something else should be active (even a different mouseover of the insurance button) 
            LoadingInsuranceActions = false;


            if (true && parentInteraction == EItemInfoButton.Open)
            {
                subInteractionsWrapper.SetSubInteractions(new OpenInteractions(___itemContextAbstractClass, ___itemUiContext_1));
                return false;
            }

            return true;
        }
    }
}