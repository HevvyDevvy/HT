using System.Collections.Generic;

namespace HustleThrough.IAP
{
    public enum StoreItemCategory { Cosmetic, Convenience, TimeSaver }

    public class StoreItem
    {
        public string Sku;
        public StoreItemCategory Category;
        public string DisplayName;
        public string Description;
        public long PriceNotes; // cost in hard currency, if buyable that way
        public string TierUnlock; // "Gold" | "Platinum" | "Diamond-Enhanced" | null
        public bool NotesPurchasable; // false for items only obtainable via a bundle (e.g. Founder's Pack)

        public StoreItem(string sku, StoreItemCategory category, string name, string description, long priceNotes, string tierUnlock = null, bool notesPurchasable = true)
        {
            Sku = sku;
            Category = category;
            DisplayName = name;
            Description = description;
            PriceNotes = priceNotes;
            TierUnlock = tierUnlock;
            NotesPurchasable = notesPurchasable;
        }
    }

    /// <summary>
    /// Full launch catalog of everything Notes (hard currency) can buy.
    /// Deliberately contains no rank-skip or level-skip entries — see the
    /// architecture doc's "What NOT to sell" section. Cosmetics are grouped
    /// by the tier progression from the Game Bible (desaturated Rookie/Gold
    /// -> neon Ganger/Platinum -> cold Gangster-plus/Diamond-Enhanced), so
    /// the catalog visibly grows richer as a player progresses rather than
    /// dumping everything on a day-one player who has no context for it yet.
    ///
    /// Pricing philosophy: every item here is priced so a single small Notes
    /// pack purchase (see IAPManager.NotesPacks) buys at least one real thing
    /// outright — no item requires stacking multiple purchases to afford,
    /// which is the most common freemium complaint ("I bought Notes and
    /// still couldn't afford anything").
    /// </summary>
    public static class StoreCatalog
    {
        public static readonly StoreItem[] Items =
        {
            // --- Gold tier (Rookie through Player, ranks 1-4) ---
            new StoreItem("cosmetic_rack_scuffed_chrome", StoreItemCategory.Cosmetic,
                "Scuffed Chrome Rack", "An early flex — a chrome-edged card rack for a Rookie who wants to stand out.", 80, "Gold"),
            new StoreItem("cosmetic_mask_variant_denim", StoreItemCategory.Cosmetic,
                "Denim Patch Mask", "A patched-together mask variant, street-level style.", 70, "Gold"),
            new StoreItem("convenience_retry_token", StoreItemCategory.Convenience,
                "Minigame Retry Token", "Instantly retry a failed hacking/tower-defence sequence.", 40, null),
            new StoreItem("time_saver_rack_refresh", StoreItemCategory.TimeSaver,
                "Instant Rack Refresh", "Skip the cooldown before new jobs appear on your rack.", 25, null),

            // --- Platinum tier (Ganger through Hustler, ranks 5-10) ---
            new StoreItem("cosmetic_rack_obsidian", StoreItemCategory.Cosmetic,
                "Obsidian Job Rack", "A sleek obsidian skin for your business-card rack.", 150, "Platinum"),
            new StoreItem("cosmetic_mask_variant_chrome", StoreItemCategory.Cosmetic,
                "Chrome Mask Variant", "An alternate finish for your street mask.", 120, "Platinum"),
            new StoreItem("cosmetic_car_neon_trim", StoreItemCategory.Cosmetic,
                "Neon Trim Package", "Underglow and neon trim for your ride, matching the Ganger-era palette.", 140, "Platinum"),
            new StoreItem("cosmetic_crew_colours_alt", StoreItemCategory.Cosmetic,
                "Alternate Crew Colours", "A second colourway for your crew's tag and gear.", 100, "Platinum"),

            // --- Diamond-Enhanced tier (Gangster through Head Hustler, ranks 11-14) ---
            new StoreItem("cosmetic_rack_diamond_plate", StoreItemCategory.Cosmetic,
                "Diamond Plate Rack", "Cold, industrial finish for the rack — Gangster-plus era.", 220, "Diamond-Enhanced"),
            new StoreItem("cosmetic_mask_variant_engraved", StoreItemCategory.Cosmetic,
                "Engraved Head Hustler Mask", "A purely cosmetic alternate finish echoing the finale mask — does not require having reached Head Hustler to buy, but is designed to feel earned either way.", 200, "Diamond-Enhanced"),
            new StoreItem("cosmetic_car_diamond_wrap", StoreItemCategory.Cosmetic,
                "Diamond Wrap", "A cold, reflective full wrap for your car.", 210, "Diamond-Enhanced"),
            new StoreItem("cosmetic_empire_hq_skin", StoreItemCategory.Cosmetic,
                "Empire HQ Reskin", "Redecorate your home base for the Empire Era.", 180, "Diamond-Enhanced"),

            // --- Universal convenience/time-savers (available at any tier) ---
            new StoreItem("convenience_retry_token_pack5", StoreItemCategory.Convenience,
                "Retry Token 5-Pack", "Five minigame retry tokens at a bulk discount.", 150, null),
            new StoreItem("time_saver_rack_refresh_pack5", StoreItemCategory.TimeSaver,
                "Rack Refresh 5-Pack", "Five instant rack refreshes at a bulk discount.", 100, null),

            // --- Bundle-exclusive cosmetic, granted only via the Founder's Pack ---
            new StoreItem("cosmetic_founders_mask_gold_trim", StoreItemCategory.Cosmetic,
                "Founder's Gold Trim Mask", "Exclusive mask finish for players who backed the game at launch. Not sold separately.", 0, null, notesPurchasable: false),
        };

        public static StoreItem Find(string sku)
        {
            foreach (var item in Items)
            {
                if (item.Sku == sku) return item;
            }
            return null;
        }

        public static IEnumerable<StoreItem> ForTier(string tier)
        {
            foreach (var item in Items)
            {
                if (item.TierUnlock == null || item.TierUnlock == tier) yield return item;
            }
        }
    }
}

