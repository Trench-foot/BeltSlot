using System;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.InventoryLogic;
using BeltSlot.Helpers;

namespace BeltSlot.Patches
{
    public class EquipmentBuildsScreenPatch : ModulePatch
    {
        private static FieldInfo? equipmentSlot = AccessTools.Field(typeof(EquipmentBuildsScreen), "equipmentSlot_1");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Constructor(typeof(EquipmentBuildsScreen));
        }

        [PatchPostfix]
        static void PostFix(EquipmentBuildsScreen __instance)
        {
            try
            {
                // Plugin.Instance.Log.LogInfo($"[Belt Slots] EquipmentBuildsScreenPatch.PostFix called");

                EquipmentSlot[]? slots = equipmentSlot?.GetValue(__instance) as EquipmentSlot[];
                if (slots != null)
                {
                    if (slots.Length != 4)
                    {
                        // Plugin.Instance.Log.LogInfo($"[Belt Slots] Already patched to {slots.Length} slots");
                        return;
                    }

                    EquipmentSlot[] newSlots = new[]
                    {
                        EquipmentSlot.TacticalVest,
                        EquipmentSlot.ArmBand,
                        EquipmentSlot.Pockets,
                        EquipmentSlot.Backpack,
                        EquipmentSlot.SecuredContainer
                    };

                    equipmentSlot?.SetValue(__instance, newSlots);
                }
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogInfo($"[Belt Slots] Exception: {ex}");
            }
        }
    }

    public class EquipmentBuildsScreenPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EquipmentBuildsScreen), nameof(EquipmentBuildsScreen.method_6));
        }
        [PatchPostfix]
        static void Postfix(EquipmentBuildsScreen __instance)
        {
            Plugin.Instance.SetBuildsArmbandSlot();
        }
    }
}
