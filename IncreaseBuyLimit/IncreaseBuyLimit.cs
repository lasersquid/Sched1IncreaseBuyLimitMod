using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

#if MONO_BUILD
using ScheduleOne.Money;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Shop;
using CartEntry = ScheduleOne.UI.Shop.CartEntry;
#else
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.UI.Phone.Delivery;
using Il2CppScheduleOne.UI.Shop;
using CartEntry = Il2CppScheduleOne.UI.Shop.CartEntry;
#endif

namespace IncreaseBuyLimit
{
    public class Sched1PatchesBase
    {
        public static object GetField(Type type, string fieldName, object target)
        {
#if MONO_BUILD
            return AccessTools.Field(type, fieldName).GetValue(target);
#else
            return AccessTools.Property(type, fieldName).GetValue(target);
#endif
        }

        public static void SetField(Type type, string fieldName, object target, object value)
        {
#if MONO_BUILD
            AccessTools.Field(type, fieldName).SetValue(target, value);
#else
            AccessTools.Property(type, fieldName).SetValue(target, value);
#endif
        }

        public static object GetProperty(Type type, string fieldName, object target)
        {
            return AccessTools.Property(type, fieldName).GetValue(target);
        }

        public static void SetProperty(Type type, string fieldName, object target, object value)
        {
            AccessTools.Property(type, fieldName).SetValue(target, value);
        }

        public static object CallMethod(Type type, string methodName, object target, object[] args)
        {
            return AccessTools.Method(type, methodName).Invoke(target, args);
        }

        public static T CastTo<T>(object o) where T : class
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                return null;
            }
        }

        public static bool Is<T>(object o)
        {
            return o is T;
        }

#if !MONO_BUILD
        public static T CastTo<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return o.TryCast<T>();
        }

        public static bool Is<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return o.TryCast<T>() != null;
        }
#endif

        public static UnityAction ToUnityAction(Action action)
        {
#if MONO_BUILD
            return new UnityAction(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction>(action);
#endif
        }

        public static UnityAction<T> ToUnityAction<T>(Action<T> action)
        {
#if MONO_BUILD
            return new UnityAction<T>(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction<T>>(action);
#endif
        }
    }


    [HarmonyPatch]
    public class ShopPatches : Sched1PatchesBase
    {
        // allow user to enter values up to 999999
        [HarmonyPatch(typeof(ShopAmountSelector), "OnValueChanged")]
        [HarmonyPrefix]
        public static bool OnValueChangedPrefix(ShopAmountSelector __instance, string value)
        {
            int value2;
            if (int.TryParse(value, out value2))
            {
                SetProperty(typeof(ShopAmountSelector), "SelectedAmount", __instance, Mathf.Clamp(value2, 1, 999999));
                __instance.InputField.SetTextWithoutNotify(__instance.SelectedAmount.ToString());
                return false;
            }
            SetProperty(typeof(ShopAmountSelector), "SelectedAmount", __instance, 1);
            __instance.InputField.SetTextWithoutNotify(string.Empty);

            return false;
        }

        // Call to OnValueChanged probably optimized out
        [HarmonyPatch(typeof(ShopAmountSelector), "OnSubmitted")]
        [HarmonyPrefix]
        public static bool OnSubmittedPrefix(ShopAmountSelector __instance, string value)
        {
            if (!__instance.IsOpen)
            {
                return false;
            }
            CallMethod(typeof(ShopAmountSelector), "OnValueChanged", __instance, [value]);
            if (__instance.onSubmitted != null)
            {
                __instance.onSubmitted.Invoke(__instance.SelectedAmount);
            }
            __instance.Close();

            return false;
        }

        // Modify shop amount selector size so user can enter large numbers
        [HarmonyPatch(typeof(ShopAmountSelector), "Open")]
        [HarmonyPrefix]
        public static void OpenPrefix(ShopAmountSelector __instance)
        {
            if (__instance.InputField.characterLimit != 6)
            {
                __instance.InputField.characterLimit = 6;
                __instance.InputField.pointSize -= 2;
                float width = __instance.Container.rect.width * 1.5f;
                __instance.Container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }

        // Call to ShopAmountSelector::Open was inlined
        [HarmonyPatch(typeof(ShopInterface), "OpenAmountSelector")]
        [HarmonyPrefix]
        public static bool OpenAmountSelectorPrefix(ShopInterface __instance, ListingUI listing)
        {
            if (!listing.Listing.Item.IsUnlocked)
            {
                return false;
            }
            if (!listing.CanAddToCart())
            {
                return false;
            }
            SetField(typeof(ShopInterface), "selectedListing", __instance, listing);
            __instance.AmountSelector.transform.position = listing.TopDropdownAnchor.position;
            SetField(typeof(ShopInterface), "dropdownMouseUp", __instance, false);
            __instance.AmountSelector.Open();
            return false;
        }

        // make cart entry red X actually remove the entire stack
        [HarmonyPatch(typeof(CartEntry), "Initialize")]
        [HarmonyPrefix]
        public static bool CartEntryInitializePrefix(CartEntry __instance, Cart cart, ShopListing listing, int quantity)
        {
            SetProperty(typeof(CartEntry), "Cart", __instance, cart);
            SetProperty(typeof(CartEntry), "Listing", __instance, listing);
            SetProperty(typeof(CartEntry), "Quantity", __instance, quantity);

            Action incrementAction = () => CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [1]);
            Action decrementAction = () => CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [-1]);
            Action removeAction = () => CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [-999999]);
            __instance.IncrementButton.onClick.AddListener(ToUnityAction(incrementAction));
            __instance.DecrementButton.onClick.AddListener(ToUnityAction(decrementAction));
            __instance.RemoveButton.onClick.AddListener(ToUnityAction(removeAction));

            CallMethod(typeof(CartEntry), "UpdateTitle", __instance, []);
            CallMethod(typeof(CartEntry), "UpdatePrice", __instance, []);

            return false;
        }

        // Enable user to input more than 999 in delivery app
        // call to OnQuantityInputSubmitted was inlined as well
        [HarmonyPatch(typeof(ListingEntry), "Initialize")]
        [HarmonyPrefix]
        public static bool ListingEntryInitializePrefix(ListingEntry __instance, ShopListing match)
        {
            SetProperty(typeof(ListingEntry), "MatchingListing", __instance, match);
            __instance.Icon.sprite = __instance.MatchingListing.Item.Icon;
            __instance.ItemNameLabel.text = __instance.MatchingListing.Item.Name;
            __instance.ItemPriceLabel.text = MoneyManager.FormatAmount(__instance.MatchingListing.Price, false, false);

            Action<string> onSubmitAction = (string value) => CallMethod(typeof(ListingEntry), "OnQuantityInputSubmitted", __instance, [value]);
            Action<string> onEndEditAction = (string value) => CallMethod(typeof(ListingEntry), "ValidateInput", __instance, []);
            Action incrementAction = () => CallMethod(typeof(ListingEntry), "ChangeQuantity", __instance, [1]);
            Action decrementAction = () => CallMethod(typeof(ListingEntry), "ChangeQuantity", __instance, [-1]);
            __instance.QuantityInput.onSubmit.AddListener(ToUnityAction<string>(onSubmitAction));
            __instance.QuantityInput.onEndEdit.AddListener(ToUnityAction<string>(onEndEditAction));
            __instance.IncrementButton.onClick.AddListener(ToUnityAction(incrementAction));
            __instance.DecrementButton.onClick.AddListener(ToUnityAction(decrementAction));

            __instance.QuantityInput.SetTextWithoutNotify(__instance.SelectedQuantity.ToString());
            __instance.RefreshLocked();

            if (__instance.QuantityInput.characterLimit != 6)
            {
                __instance.QuantityInput.characterLimit = 6;
                __instance.QuantityInput.textComponent.fontSize = 16;
                CastTo<RectTransform>(__instance.QuantityInput.transform).sizeDelta = new Vector2(80, 40);
            }

            return false;
        }

        // Allow user to purchase more than 999 items at a time from phone app
        [HarmonyPatch(typeof(ListingEntry), "SetQuantity")]
        [HarmonyPrefix]
        public static bool SetQuantityPrefix(ListingEntry __instance, int quant, bool notify)
        {
            if (!__instance.MatchingListing.Item.IsUnlocked)
            {
                quant = 0;
            }
            SetProperty(typeof(ListingEntry), "SelectedQuantity", __instance, Mathf.Clamp(quant, 0, 999999));
            __instance.QuantityInput.SetTextWithoutNotify(__instance.SelectedQuantity.ToString());
            if (notify && __instance.onQuantityChanged != null)
            {
                __instance.onQuantityChanged.Invoke();
            }

            return false;
        }

        // Call to SetQuantity probably optimized out
        [HarmonyPatch(typeof(ListingEntry), "OnQuantityInputSubmitted")]
        [HarmonyPrefix]
        public static bool OnQuantityInputSubmittedPrefix(ListingEntry __instance, string value)
        {
            int quant;
            if (int.TryParse(value, out quant))
            {
                __instance.SetQuantity(quant, true);
                return false;
            }
            __instance.SetQuantity(0, true);

            return false;
        }

        // Call to SetQuantity probably optimized out
        [HarmonyPatch(typeof(ListingEntry), "ChangeQuantity")]
        [HarmonyPrefix]
        public static bool ChangeQuantityPrefix(ListingEntry __instance, int change)
        {
            __instance.SetQuantity(__instance.SelectedQuantity + change, true);

            return false;
        }

        // call to OnQuantityInputSubmitted probably optimized out
        [HarmonyPatch(typeof(ListingEntry), "ValidateInput")]
        [HarmonyPrefix]
        public static bool ValidateInputPrefix(ListingEntry __instance)
        {
            CallMethod(typeof(ListingEntry), "OnQuantityInputSubmitted", __instance, [__instance.QuantityInput.text]);
            return false;
        }
    }
}
