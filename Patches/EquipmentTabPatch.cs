using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BeltSlot.Patches
{
    internal class EquipmentTabPatch : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // one way methods can be patched is by targeting both their class name and the name of the method itself
            // the example in this patch is the Jump() method in the Player class
            return AccessTools.Method(typeof(EquipmentTab), nameof(EquipmentTab.Awake));
        }


        [PatchPrefix]
        static bool Prefix(EquipmentTab __instance)
        {

            {
                //__instance.transform.parent.gameObject.AddComponent<SearchableSlotView>();
                if(Plugin.Instance != null)
                {
                    Plugin.Instance.armBandSlot = __instance.transform.GetChild(4).gameObject;
                    //GameObject _armBandSlot = __instance.transform.GetChild(4).gameObject;
                    //Plugin.Instance.armBandSlot.AddComponent<SearchableSlotView>();
                    //Plugin.Instance.armBandSlot.AddComponent<SearchableItemView>();
                    //Plugin.Instance.armBandSlot.AddComponent<SearchableView>();
                    //Plugin.Instance.armBandSlot.transform.parent.gameObject
                }
            }
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            /*Vector3 vector3 = new Vector3(0, 0, 0);
            Quaternion rotation = new Quaternion(0, 0, 0, 0);
            if (Plugin.Instance != null)
            {
                if (Plugin.Instance.tacticalSlot != null)
                {
                    GameObject testObject = GameObject.Instantiate(Plugin.Instance.tacticalSlot, vector3, rotation);
                    testObject.transform.SetParent(__instance.transform, false);
                    //Plugin.Instance.tacticalSlot.
                    //Plugin.Instance.armBandSlot = __instance.transform.GetChild(4).gameObject;
                    //Plugin.Instance.armBandSlot
                }
            }*/
        }
    }
}