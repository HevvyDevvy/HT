using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using HustleThrough.Progression;

namespace HustleThrough.IAP
{
    /// <summary>
    /// Offline equivalent of the old billingService.ts. Since there's no
    /// backend to validate receipts against anymore, this uses Unity IAP's
    /// built-in CrossPlatformValidator (receipt obfuscation) for on-device
    /// tamper resistance instead of server-side verification.
    ///
    /// HONEST TRADE-OFF: this is weaker than server-side verification — a
    /// sufficiently determined person could still patch a device build to
    /// fake a purchase. For a single-player game with no player-vs-player
    /// competition, that risk is mainly "lost revenue from a small number of
    /// determined cheaters," not "broken game economy for everyone else,"
    /// which is why it's an acceptable trade for going fully serverless. If
    /// you later want the stronger guarantee, the old backend in
    /// /future-online-backend already has real Apple/Google server
    /// verification built and ready to reconnect.
    ///
    /// The "no IAP touches rank" rule still holds structurally: this class
    /// only ever calls PlayerProgress.CreditNotes, never anything that could
    /// change rank or level.
    /// </summary>
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        public static IAPManager Instance { get; private set; }

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private CrossPlatformValidator _validator;

        // SKU -> Notes granted. Must match product IDs configured in App Store
        // Connect / Google Play Console. Five-tier ladder: small packs convert
        // impulse buyers, the "best value" flag on Large steers most spend
        // toward the pack with the best real-money-to-Notes ratio (standard,
        // non-manipulative freemium anchoring — no pack is a bad deal, some
        // are just a better deal, and that's shown honestly in the UI).
        private static readonly (string sku, long notes, bool bestValue)[] NotesPacks =
        {
            ("notes_pack_small",  100,  false),  // ~£0.99
            ("notes_pack_medium", 550,  false),  // ~£4.49
            ("notes_pack_large",  1200, true),   // ~£8.99 — best Notes-per-£
            ("notes_pack_mega",   2600, false),  // ~£17.99
            ("notes_pack_whale",  7000, false),  // ~£39.99 — for the small % of players who want to go big
        };

        // One-time, non-consumable purchases.
        public const string FoundersPackSku = "founders_pack";   // ~£3.99, one-time, new-player-only
        public const string RemoveAdsSku = "remove_ads";         // ~£2.99, one-time

        public static readonly string[] AllConsumableSkus = System.Array.ConvertAll(NotesPacks, p => p.sku);
        public static readonly string[] AllNonConsumableSkus = { FoundersPackSku, RemoveAdsSku };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePurchasing();
        }

        private void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var sku in AllConsumableSkus)
            {
                builder.AddProduct(sku, ProductType.Consumable);
            }
            foreach (var sku in AllNonConsumableSkus)
            {
                builder.AddProduct(sku, ProductType.NonConsumable);
            }
            UnityPurchasing.Initialize(this, builder);

#if !UNITY_EDITOR
            try
            {
                _validator = new CrossPlatformValidator(
                    GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("IAP receipt obfuscation not configured yet: " + e.Message +
                    " — run Unity IAP's 'Generate Obfuscated Class' from the Services window " +
                    "once real store credentials are set up.");
            }
#endif
        }

        public void BuyProduct(string productId)
        {
            if (_storeController == null)
            {
                Debug.LogError("IAP not initialized yet.");
                return;
            }

            if (productId == FoundersPackSku && !IsFoundersPackEligible())
            {
                Debug.LogWarning("Founder's Pack is a new-player-only offer and is no longer available to this save.");
                return;
            }

            _storeController.InitiatePurchase(productId);
        }

        /// <summary>
        /// Founder's Pack is only offered before a player reaches Rank 3
        /// (Short General) — a real new-player deal, not something a
        /// long-time player stumbles into paying full separate prices to
        /// replicate. Adjust the threshold as needed once you have real
        /// playtime data on conversion timing.
        /// </summary>
        public bool IsFoundersPackEligible()
        {
            return PlayerProgress.Instance != null
                && PlayerProgress.Instance.Rank < 3
                && !PlayerProgress.Instance.OwnsStoreItem("cosmetic_founders_mask_gold_trim");
        }

        public void BuyStoreItem(string sku)
        {
            var item = StoreCatalog.Find(sku);
            if (item == null)
            {
                Debug.LogWarning("Unknown store SKU: " + sku);
                return;
            }
            if (!item.NotesPurchasable)
            {
                Debug.LogWarning("This item is bundle-exclusive and cannot be bought directly: " + sku);
                return;
            }
            if (PlayerProgress.Instance.OwnsStoreItem(sku))
            {
                Debug.Log("Already owned: " + sku);
                return;
            }
            if (PlayerProgress.Instance.SpendNotes(item.PriceNotes))
            {
                PlayerProgress.Instance.GrantStoreItem(sku);
            }
            else
            {
                Debug.Log("Not enough Notes for: " + sku);
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;

            bool validReceipt = true;
            if (_validator != null)
            {
                try
                {
                    _validator.Validate(args.purchasedProduct.receipt);
                }
                catch (IAPSecurityException)
                {
                    validReceipt = false;
                    Debug.LogWarning("Receipt failed local validation for " + productId);
                }
            }

            if (validReceipt)
            {
                // Consumable Notes packs
                foreach (var (sku, notes, _) in NotesPacks)
                {
                    if (sku == productId)
                    {
                        PlayerProgress.Instance.CreditNotes(notes);
                        return PurchaseProcessingResult.Complete;
                    }
                }

                // Non-consumables
                if (productId == RemoveAdsSku)
                {
                    PlayerProgress.Instance.SetAdsRemoved(true);
                }
                else if (productId == FoundersPackSku)
                {
                    // One-time new-player bundle: a chunk of Notes, an exclusive
                    // cosmetic, and ads removed — priced well below buying the
                    // equivalent separately, and gated so it can't be bought twice
                    // (NonConsumable products are automatically deduped by Unity IAP).
                    PlayerProgress.Instance.CreditNotes(400);
                    PlayerProgress.Instance.GrantStoreItem("cosmetic_founders_mask_gold_trim");
                    PlayerProgress.Instance.SetAdsRemoved(true);
                }
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError("IAP initialization failed: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"IAP initialization failed: {error} — {message}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"Purchase failed for {product.definition.id}: {reason}");
        }
    }
}
