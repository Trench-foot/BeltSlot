using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Numerics;
using System.Reflection;
using UnityEngine.EventSystems;

namespace BeltSlot.Patches
{
    internal class ItemUiContextPatch : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // one way methods can be patched is by targeting both their class name and the name of the method itself
            // the example in this patch is the Jump() method in the Player class
            return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.QuickFindAppropriatePlace));
        }


        [PatchPrefix]
        static void Prefix(ItemUiContext __instance)
        {
           
            //Plugin.Instance.generatedGridsView = __instance.transform.parent.gameObject.GetComponentInChildren<GeneratedGridsView>().gameObject;
            //return true;
        }

        [PatchPostfix]
        static void Postfix(ItemUiContext __instance)
        {

        }
    }
}