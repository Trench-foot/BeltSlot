using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BeltSlot.Patches
{
    internal class ContainedGridsViewPatch : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // one way methods can be patched is by targeting both their class name and the name of the method itself
            // the example in this patch is the Jump() method in the Player class
            return AccessTools.Method(typeof(ContainedGridsView), nameof(ContainedGridsView.Display));
        }


        [PatchPrefix]
        static void Prefix(ContainedGridsView __instance)
        {
            if(Plugin.Instance != null)
            {
                Plugin.Instance.containedGridsView = __instance;
            }

        }

        [PatchPostfix]
        static void Postfix()
        {

        }
    }
}