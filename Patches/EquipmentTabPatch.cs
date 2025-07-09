using System;
using System.Collections.Generic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;

namespace BeltSlot.Patches
{
    public class EquipmentTabPatch : ModulePatch
    {
        private static FieldInfo? slotViews = AccessTools.Field(typeof(EquipmentTab), "_slotViews");
        private static FieldInfo? armbandSlot = AccessTools.Field(typeof(EquipmentTab), "_armbandSlot");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EquipmentTab), nameof(EquipmentTab.Awake));
        }

        [PatchPostfix]
        static void PostFix(EquipmentTab __instance)
        {
            if (Plugin.Instance != null)
            {
                Plugin.Instance.inventoryScreenLoaded = true;
            }
            //try
            //{
            //    // Plugin.Instance.Log.LogInfo($"[Belt Slots] ContainersPanelPatch.PostFix called");

            //    Dictionary<EquipmentSlot, SlotView> views = slotViews?.GetValue(__instance) as Dictionary<EquipmentSlot, SlotView>;
            //    views?.Remove(EquipmentSlot.ArmBand);

            //    SlotView? armband = armbandSlot?.GetValue(__instance) as SlotView;
            //    armband?.gameObject.SetActive(false);
            //}
            //catch (Exception ex)
            //{
            //    Plugin.Instance.Log.LogInfo($"[Belt Slots] Exception: {ex}");
            //}
        }
    }
}
