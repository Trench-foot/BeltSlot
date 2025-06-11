using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BeltSlot.Patches
{
    internal class ContainersPanelPatch : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // one way methods can be patched is by targeting both their class name and the name of the method itself
            // the example in this patch is the Jump() method in the Player class
            return AccessTools.Method(typeof(ContainersPanel), nameof(ContainersPanel.Show));
        }


        [PatchPrefix]
        static bool Prefix(ContainersPanel __instance)
        {

            {
                if (Plugin.Instance != null)
                {

                }
            }
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {

        }
    }
}