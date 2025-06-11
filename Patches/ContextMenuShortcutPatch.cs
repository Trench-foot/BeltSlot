using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static EFT.SpeedTree.TreeWind;
using DragItemContext = ItemContextClass;

namespace BeltSlot.Patches
{
    public static class ContextMenuShortcutPatches
    {
        private static TMP_InputField LastSelectedInput = null;

        public static void Enable()
        {
            new ItemUiContextPatch().Enable();

            new HideoutItemViewRegisterContextPatch().Enable();

            new TradingPanelRegisterContextPatch().Enable();
            new TradingPanelUnregisterContextPatch().Enable();

            new SelectCurrentContextPatch().Enable();
            new DeselectCurrentContextPatch().Enable();
        }

        public class ItemUiContextPatch : ModulePatch
        {
            private static ItemInfoInteractionsAbstractClass<EItemInfoButton> Interactions;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.Update));
            }

            [PatchPostfix]
            public static void Postfix(ItemUiContext __instance, DragItemContext ___itemContextClass)
            {
                // Get instance of ItemUiContext
                if(Plugin.Instance != null)
                {
                    Plugin.Instance.ItemUiContext = __instance;
                }

                // Need an item context to operate on
                ItemContextAbstractClass itemContext = __instance.R().ItemContext;

                //itemContext.Item.Id;
                // itemContext is what the mouse is over
                // ___itemContextClass is the currently dragged item
                // Only do anything if the mouse is over an item and nothing is being dragged
                if (itemContext == null || ___itemContextClass != null)
                {
                    return;
                }

                if (Input.GetKeyDown(KeyCode.I))
                {
                    TryInteraction(__instance, itemContext, EItemInfoButton.Open);
                }
                Interactions = null;
            }

            private static void TryInteraction(ItemUiContext itemUiContext, ItemContextAbstractClass itemContext, EItemInfoButton interaction, EItemInfoButton[] fallbackInteractions = null)
            {
                Interactions ??= itemUiContext.GetItemContextInteractions(itemContext, null);
                if (!Interactions.ExecuteInteraction(interaction) && fallbackInteractions != null)
                {
                    foreach (var fallbackInteraction in fallbackInteractions)
                    {
                        if (Interactions.ExecuteInteraction(fallbackInteraction))
                        {
                            return;
                        }
                    }
                }
            }
        }

        // HideoutItemViews don't register themselves with ItemUiContext for some reason
        public class HideoutItemViewRegisterContextPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(HideoutItemView), nameof(HideoutItemView.OnPointerEnter));
            }

            [PatchPostfix]
            public static void Postfix(HideoutItemView __instance, ItemUiContext ___ItemUiContext)
            {
                ___ItemUiContext.RegisterCurrentItemContext(__instance.ItemContext);
            }
        }

        public class TradingPanelRegisterContextPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(TradingRequisitePanel), nameof(TradingRequisitePanel.method_1)); // OnHoverStart
            }

            [PatchPostfix]
            public static void Postfix(ItemUiContext ___itemUiContext_0, ItemContextAbstractClass ___itemContextAbstractClass)
            {
                ___itemUiContext_0.RegisterCurrentItemContext(___itemContextAbstractClass);
            }
        }

        public class TradingPanelUnregisterContextPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(TradingRequisitePanel), nameof(TradingRequisitePanel.method_2)); // OnHoverEnd
            }

            [PatchPostfix]
            public static void Postfix(ItemUiContext ___itemUiContext_0, ItemContextAbstractClass ___itemContextAbstractClass)
            {
                ___itemUiContext_0.UnregisterCurrentItemContext(___itemContextAbstractClass);
            }
        }

        // Keybinds don't work if a textbox has focus - setting the textbox to readonly seems the best way to fix this
        // without changing selection and causing weird side effects
        public class SelectCurrentContextPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.RegisterCurrentItemContext));
            }

            [PatchPostfix]
            public static void Postfix()
            {

                if (EventSystem.current?.currentSelectedGameObject != null)
                {
                    LastSelectedInput = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
                    if (LastSelectedInput != null)
                    {
                        LastSelectedInput.readOnly = true;
                    }
                }
            }
        }

        public class DeselectCurrentContextPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.UnregisterCurrentItemContext));
            }

            [PatchPostfix]
            public static void Postfix()
            {

                if (LastSelectedInput != null)
                {
                    LastSelectedInput.readOnly = false;
                }

                LastSelectedInput = null;
            }
        }
    }

}