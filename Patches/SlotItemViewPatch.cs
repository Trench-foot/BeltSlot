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
    internal class SlotItemViewPatch : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // one way methods can be patched is by targeting both their class name and the name of the method itself
            // the example in this patch is the Jump() method in the Player class
            return AccessTools.Method(typeof(SlotItemView), nameof(SlotItemView.NewSlotItemView));
        }


        [PatchPrefix]
        static void Prefix(SlotItemView __instance, Item item, ItemContextAbstractClass sourceContext, ItemUiContext itemUiContext)
        {
            {
                if (!(item is CompoundItem lootItem))
                {
                    return;
                }
                if (lootItem.Grids != null && lootItem.Grids.Length <= 0)
                {
                    return;
                }
                if(Plugin.Instance != null)
                {
                    Plugin.Instance.beltItem = item as CompoundItem;
                    Plugin.Instance.newItemAdded = true;
                    Plugin.Instance.newSourceContext = sourceContext;
                    Plugin.Instance.newItemUiContext = itemUiContext;
                }
            }

            //Plugin.Instance.generatedGridsView = __instance.transform.parent.gameObject.GetComponentInChildren<GeneratedGridsView>().gameObject;
            //return true;
        }

        [PatchPostfix]
        static void Postfix(SlotItemView __instance)
        {

        }
    }

    internal class SlotItemViewPrefixPatch : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SlotItemView), nameof(SlotItemView.OnClick));
        }

        [PatchPostfix]
        static void Prefix(SlotItemView __instance, PointerEventData.InputButton button, Vector2 position, bool doubleClick)
        {
            
        }
    }
}