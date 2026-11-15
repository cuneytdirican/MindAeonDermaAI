using MindAeonDermaAI.Core.Entities;

namespace MindAeonDermaAI.Web.Services;

/// <summary>
/// Curated, rule-based mapping from a deterministic <see cref="ScoreResult"/> to cosmetic-ingredient (INCI)
/// suggestions, plus a lightweight suitability check for a pasted product INCI list.
/// Everything here is NON-DIAGNOSTIC, cosmetic-education only, and intended for research/prototyping use.
/// No medical claims are made; wording is deliberately cautious ("may support…").
/// </summary>
public static class RecommendationEngine
{
    // A single suggested active, the cosmetic concern it addresses, and a short "why".
    public record IngredientRec(string Ingredient, string Concern, string Rationale);

    // Thresholds used to decide when a need-score is "high" or an index is "low" (illustrative).
    private const double NeedHigh = 55;
    private const double IndexLow = 45;

    /// <summary>
    /// Build a de-duplicated list of cosmetic-ingredient suggestions from the score. Rules are curated and
    /// cautious; daily broad-spectrum SPF is always included as a baseline.
    /// </summary>
    public static List<IngredientRec> Recommend(ScoreResult s)
    {
        var recs = new List<IngredientRec>();
        var phenotypes = (s.PhenotypeCodesCsv ?? string.Empty).ToUpperInvariant();

        bool barrierNeed = s.BarrierHydrationNeedScore >= NeedHigh
            || (s.BarrierResilienceIndex.HasValue && s.BarrierResilienceIndex.Value <= IndexLow);
        bool collagenNeed = s.CollagenRemodelingNeedScore >= NeedHigh
            || (s.RemodelingBalanceIndex.HasValue && s.RemodelingBalanceIndex.Value <= IndexLow);
        bool laxityNeed = s.DeepEcmLaxityNeedScore >= NeedHigh;
        bool hydrationAxisLow = s.HyaluronanAxisIndex.HasValue && s.HyaluronanAxisIndex.Value <= IndexLow;
        bool inflammaging = phenotypes.Contains("INFLAMMAGING");
        bool exposomeHigh = s.ExposomeBurden.HasValue && s.ExposomeBurden.Value >= NeedHigh;
        bool pigmentConcern = phenotypes.Contains("PIGMENT") || phenotypes.Contains("PHOTO");

        // Barrier / hydration support.
        if (barrierNeed || hydrationAxisLow)
        {
            const string concern = "Barrier & hydration support";
            recs.Add(new("Niacinamide", concern, "May support barrier proteins and help reduce trans-epidermal water loss."));
            recs.Add(new("Ceramides", concern, "Replenish key barrier lipids that may support a more resilient stratum corneum."));
            recs.Add(new("Panthenol (pro-vitamin B5)", concern, "Humectant/soothing agent that may support comfort and hydration."));
            recs.Add(new("Hyaluronic acid", concern, "Humectant that may support surface hydration and a plumper look."));
            if (hydrationAxisLow)
                recs.Add(new("Glycerin", concern, "Classic humectant that may support lasting hydration of the outer skin."));
        }

        // Collagen / remodeling support.
        if (collagenNeed)
        {
            const string concern = "Collagen & firmness support";
            recs.Add(new("Retinoid (e.g. retinol)", concern, "Well-studied cosmetic active that may support the look of firmness and fine lines."));
            recs.Add(new("Matrixyl / signal peptides", concern, "Peptides that may support the appearance of skin firmness over time."));
            recs.Add(new("Vitamin C (L-ascorbic acid)", concern, "Antioxidant that may support a brighter, firmer-looking complexion."));
        }

        // Deep ECM laxity (firmness).
        if (laxityNeed)
        {
            const string concern = "Deep firmness support";
            recs.Add(new("Peptides", concern, "May support the appearance of firmness where deeper laxity is a concern."));
            recs.Add(new("Retinoid (e.g. retinol)", concern, "May support skin renewal and the look of firmness."));
        }

        // Inflammaging / soothing.
        if (inflammaging)
        {
            const string concern = "Soothing & calming";
            recs.Add(new("Niacinamide", concern, "May help support a calmer, more even-looking complexion."));
            recs.Add(new("Centella asiatica / Madecassoside", concern, "Botanical that may support soothing of visibly stressed skin."));
            recs.Add(new("Green tea (Camellia sinensis)", concern, "Polyphenol antioxidant that may support a calmer skin appearance."));
        }

        // Exposome / pigment.
        if (exposomeHigh || pigmentConcern)
        {
            const string concern = "Environmental & tone support";
            recs.Add(new("Vitamin C (L-ascorbic acid)", concern, "Antioxidant that may support defense against visible environmental dullness."));
            recs.Add(new("Alpha-arbutin", concern, "May support a more even-looking skin tone."));
        }

        // Baseline for everyone.
        recs.Add(new("Daily broad-spectrum SPF", "Photoprotection (baseline)",
            "Daily sunscreen may support all of the above and is the foundation of any cosmetic routine."));

        // De-duplicate by ingredient (keep first concern/rationale seen).
        return recs
            .GroupBy(r => r.Ingredient, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    // ---- INCI suitability check --------------------------------------------------------------

    public record IngredientVerdict(string Name, string Verdict, string Note);

    public record InciResult(List<IngredientVerdict> Items, int SuitabilityPercent, string Summary);

    // Curated ingredient tags. An ingredient may carry several tags.
    [Flags]
    private enum Tag
    {
        None = 0,
        BarrierGood = 1 << 0,
        Soothing = 1 << 1,
        Collagen = 1 << 2,
        Hydration = 1 << 3,
        Exfoliant = 1 << 4,
        Occlusive = 1 << 5,
        Fragrance = 1 << 6,
        DryingAlcohol = 1 << 7,
        Comedogenic = 1 << 8,
    }

    // ~40 common INCI names mapped to cosmetic tags. Keys are normalized (lower-case, single-spaced).
    private static readonly Dictionary<string, Tag> Dictionary = new(StringComparer.OrdinalIgnoreCase)
    {
        // Barrier / hydration good actors
        ["niacinamide"] = Tag.BarrierGood | Tag.Soothing,
        ["ceramide np"] = Tag.BarrierGood,
        ["ceramide"] = Tag.BarrierGood,
        ["panthenol"] = Tag.BarrierGood | Tag.Soothing | Tag.Hydration,
        ["glycerin"] = Tag.Hydration,
        ["sodium hyaluronate"] = Tag.Hydration,
        ["hyaluronic acid"] = Tag.Hydration,
        ["squalane"] = Tag.BarrierGood | Tag.Hydration,
        ["cholesterol"] = Tag.BarrierGood,
        ["allantoin"] = Tag.Soothing,
        ["butylene glycol"] = Tag.Hydration,
        ["propylene glycol"] = Tag.Hydration,
        ["sodium pca"] = Tag.Hydration,
        ["urea"] = Tag.Hydration,
        ["betaine"] = Tag.Hydration,
        // Soothing botanicals
        ["centella asiatica extract"] = Tag.Soothing,
        ["madecassoside"] = Tag.Soothing,
        ["bisabolol"] = Tag.Soothing,
        ["camellia sinensis leaf extract"] = Tag.Soothing,
        ["aloe barbadensis leaf juice"] = Tag.Soothing | Tag.Hydration,
        ["oat kernel extract"] = Tag.Soothing | Tag.BarrierGood,
        ["colloidal oatmeal"] = Tag.Soothing | Tag.BarrierGood,
        // Collagen / actives
        ["retinol"] = Tag.Collagen,
        ["retinal"] = Tag.Collagen,
        ["ascorbic acid"] = Tag.Collagen,
        ["ascorbyl glucoside"] = Tag.Collagen,
        ["tetrahexyldecyl ascorbate"] = Tag.Collagen,
        ["palmitoyl pentapeptide-4"] = Tag.Collagen,
        ["palmitoyl tripeptide-1"] = Tag.Collagen,
        ["copper tripeptide-1"] = Tag.Collagen,
        // Exfoliants
        ["glycolic acid"] = Tag.Exfoliant,
        ["lactic acid"] = Tag.Exfoliant | Tag.Hydration,
        ["salicylic acid"] = Tag.Exfoliant,
        ["mandelic acid"] = Tag.Exfoliant,
        // Occlusives / comedogenic
        ["petrolatum"] = Tag.Occlusive | Tag.BarrierGood,
        ["dimethicone"] = Tag.Occlusive,
        ["shea butter"] = Tag.Occlusive | Tag.BarrierGood,
        ["coconut oil"] = Tag.Occlusive | Tag.Comedogenic,
        ["isopropyl myristate"] = Tag.Comedogenic,
        ["cocoa butter"] = Tag.Occlusive | Tag.Comedogenic,
        // Cautions
        ["fragrance"] = Tag.Fragrance,
        ["parfum"] = Tag.Fragrance,
        ["limonene"] = Tag.Fragrance,
        ["linalool"] = Tag.Fragrance,
        ["citronellol"] = Tag.Fragrance,
        ["geraniol"] = Tag.Fragrance,
        ["alcohol denat"] = Tag.DryingAlcohol,
        ["denatured alcohol"] = Tag.DryingAlcohol,
        ["sd alcohol 40"] = Tag.DryingAlcohol,
        ["ethanol"] = Tag.DryingAlcohol,
    };

    /// <summary>
    /// Parse a pasted INCI list, match each token against the curated dictionary, assign a cosmetic verdict,
    /// and compute an overall suitability blend. Barrier-deficient / sensitive subjects weight cautions higher.
    /// Non-diagnostic.
    /// </summary>
    public static InciResult AnalyzeInci(string inciText, ScoreResult s)
    {
        var items = new List<IngredientVerdict>();
        if (string.IsNullOrWhiteSpace(inciText))
            return new InciResult(items, 0, "No ingredients were provided. Paste an INCI list to analyze.");

        bool barrierDeficient = s.BarrierHydrationNeedScore >= NeedHigh
            || (s.BarrierResilienceIndex.HasValue && s.BarrierResilienceIndex.Value <= IndexLow)
            || (s.PhenotypeCodesCsv ?? string.Empty).ToUpperInvariant().Contains("INFLAMMAGING");
        bool collagenNeed = s.CollagenRemodelingNeedScore >= NeedHigh;

        // Split on commas and newlines; normalize whitespace and strip trailing percentages/parentheses.
        var tokens = inciText
            .Split(new[] { ',', '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Normalize)
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        int beneficial = 0, caution = 0;
        foreach (var token in tokens)
        {
            var tag = Lookup(token);

            string verdict, note;
            if (tag.HasFlag(Tag.Fragrance))
            {
                verdict = "Caution";
                note = barrierDeficient
                    ? "Fragrance component — may be worth avoiding for a barrier-deficient / sensitive profile."
                    : "Fragrance component — a common source of irritation for reactive skin.";
                caution++;
            }
            else if (tag.HasFlag(Tag.DryingAlcohol))
            {
                verdict = "Caution";
                note = barrierDeficient
                    ? "Drying alcohol — may compromise an already-stressed barrier."
                    : "Drying alcohol high on the list may feel dehydrating for some skin.";
                caution++;
            }
            else if (tag.HasFlag(Tag.BarrierGood) || tag.HasFlag(Tag.Hydration) || tag.HasFlag(Tag.Soothing))
            {
                verdict = "Beneficial";
                note = "May support barrier, hydration or soothing — a good fit for this profile.";
                beneficial++;
            }
            else if (tag.HasFlag(Tag.Collagen))
            {
                verdict = "Beneficial";
                note = collagenNeed
                    ? "Cosmetic active that may support the look of firmness — relevant to this profile."
                    : "Cosmetic active that may support the look of firmness.";
                beneficial++;
            }
            else if (tag.HasFlag(Tag.Comedogenic))
            {
                verdict = "Caution";
                note = "May be pore-clogging for some skin types depending on formulation position.";
                caution++;
            }
            else
            {
                verdict = "Neutral";
                note = tag.HasFlag(Tag.Occlusive)
                    ? "Occlusive/texture agent — generally well tolerated."
                    : "Not in the curated reference; treated as neutral.";
            }

            items.Add(new IngredientVerdict(Prettify(token), verdict, note));
        }

        // Weighted blend: cautions weigh more when the subject is barrier-deficient / sensitive.
        double cautionWeight = barrierDeficient ? 2.0 : 1.3;
        double weighted = beneficial + caution * cautionWeight;
        int suitability = weighted <= 0
            ? 60 // all-neutral list: mildly suitable by default
            : (int)Math.Round(Math.Clamp(100.0 * beneficial / weighted, 0, 100));
        // Blend toward a neutral midpoint so all-neutral or sparse lists aren't scored at extremes.
        int neutralCount = items.Count - beneficial - caution;
        if (items.Count > 0)
            suitability = (int)Math.Round((suitability * (beneficial + caution) + 60.0 * neutralCount) / items.Count);
        suitability = Math.Clamp(suitability, 0, 100);

        string summary = items.Count == 0
            ? "None of the pasted terms matched the curated reference."
            : $"{beneficial} beneficial, {caution} caution and {neutralCount} neutral ingredient(s) recognized. " +
              (suitability >= 70
                  ? "This list looks broadly suitable for the profile."
                  : suitability >= 45
                      ? "This list is a mixed fit — review the caution items below."
                      : "This list carries several caution items for this profile — review carefully.") +
              " Cosmetic education only, non-diagnostic.";

        return new InciResult(items, suitability, summary);
    }

    // Normalize a raw token: lower-case, strip percentages/parentheses, collapse whitespace.
    private static string Normalize(string raw)
    {
        var t = raw.ToLowerInvariant().Trim();
        int paren = t.IndexOf('(');
        if (paren >= 0) t = t[..paren];
        // Drop stray percentage/number suffixes.
        t = new string(t.Where(c => !char.IsDigit(c) && c != '%').ToArray());
        t = string.Join(' ', t.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        return t.Trim().TrimEnd('.', '-', '_');
    }

    // Exact then partial (contains) match against the dictionary.
    private static Tag Lookup(string token)
    {
        if (Dictionary.TryGetValue(token, out var exact)) return exact;
        foreach (var kv in Dictionary)
            if (token.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        return Tag.None;
    }

    // Title-case a normalized token for display.
    private static string Prettify(string token) =>
        string.Join(' ', token.Split(' ')
            .Select(w => w.Length == 0 ? w : char.ToUpperInvariant(w[0]) + w[1..]));
}
