namespace MindAeonDermaAI.Web.Services;

/// <summary>
/// Curated, non-diagnostic scientific background for each protein biomarker. All entries are
/// educational summaries of well-established skin-biology mechanisms; references are representative
/// literature-style pointers (no invented DOIs or page numbers) and must never be read as clinical claims.
/// </summary>
public static class BiomarkerEvidenceCatalog
{
    /// <summary>One curated evidence record per biomarker code.</summary>
    public record Evidence(string Code, string Title, string WhyItMatters, string[] References);

    /// <summary>Curated entries keyed by biomarker code (matches the seeded ProteinBiomarker codes).</summary>
    public static readonly IReadOnlyDictionary<string, Evidence> All = new Dictionary<string, Evidence>
    {
        // ---- Collagen / fibroblast remodeling domain ----
        ["COL1A1"] = new("COL1A1", "Collagen type I α1",
            "COL1A1 encodes the major chain of type I collagen, the dominant structural protein of the dermis that provides tensile strength and firmness. Expression generally declines with chronological and photo-aging as fibroblast activity falls, which is associated in the literature with thinning and reduced mechanical resilience. In this research context a comparatively lower signal is read as greater biological need for collagen support, never as a diagnosis.",
            new[]
            {
                "Fisher GJ et al. — Mechanisms of photoaging and chronological skin aging — Arch Dermatol",
                "Varani J et al. — Decreased collagen production in chronologically aged skin — Am J Pathol",
                "Representative reviews on dermal collagen type I and skin structural aging"
            }),
        ["COL3A1"] = new("COL3A1", "Collagen type III α1",
            "COL3A1 encodes type III collagen, which forms finer reticular fibers that co-assemble with type I collagen and is proportionally higher in younger and repairing skin. Shifts in the type III to type I balance are commonly discussed in the context of dermal remodeling and scar-versus-normal architecture. A lower relative signal is interpreted here as a marker of reduced remodeling reserve, in cautious non-diagnostic terms.",
            new[]
            {
                "Representative reviews on collagen type I/III ratio in skin remodeling and repair — J Invest Dermatol",
                "Weber L et al. — Collagen types in normal and aging human dermis — Eur J Cell Biol",
                "Representative literature on dermal extracellular-matrix composition"
            }),
        ["MMP1"] = new("MMP1", "Matrix metalloproteinase-1 (interstitial collagenase)",
            "MMP-1 is the principal enzyme that initiates cleavage of fibrillar type I and III collagen and is strongly induced by UV exposure through AP-1 signaling. Elevated activity is widely linked in the literature to accelerated collagen breakdown and matrix disorganization in photoaged skin. Because higher levels indicate more degradation, an elevated signal is treated here as increased biological need for protective and remodeling support.",
            new[]
            {
                "Fisher GJ et al. — UV induction of matrix metalloproteinases and collagen degradation — N Engl J Med",
                "Brennan M et al. — MMP-1 in photoaging and collagen turnover — Photochem Photobiol",
                "Representative reviews on matrix metalloproteinases in skin aging"
            }),
        ["TIMP1"] = new("TIMP1", "Tissue inhibitor of metalloproteinases-1",
            "TIMP-1 is an endogenous inhibitor that restrains MMP activity and helps preserve the collagen matrix, so it is best interpreted opposite to MMP-1 as part of a degradation-versus-protection balance. Reduced inhibitory capacity relative to metalloproteinase activity is associated in the literature with net matrix loss. A lower signal is read here as reduced protective reserve, strictly as a research indicator.",
            new[]
            {
                "Representative reviews on the MMP/TIMP balance in dermal matrix homeostasis — J Invest Dermatol",
                "Ashworth JL et al. — TIMP regulation of extracellular-matrix turnover — Matrix Biol",
                "Representative literature on metalloproteinase inhibition in skin"
            }),
        ["ELN"] = new("ELN", "Elastin",
            "ELN encodes elastin, the core protein of dermal elastic fibers that provides recoil and elasticity to skin. Chronic UV exposure drives disorganized elastin accumulation (solar elastosis) while functional, well-organized elastic fibers tend to decline, so both quantity and organization matter. A lower functional signal is interpreted here as reduced elastic support and greater biological need, in non-diagnostic language.",
            new[]
            {
                "Uitto J — Elastic fibers, elastin and solar elastosis in skin aging — J Invest Dermatol",
                "Naylor EC et al. — Molecular aspects of skin ageing and elastic fibers — Maturitas",
                "Representative reviews on dermal elastin and skin elasticity"
            }),
        ["TGFB1"] = new("TGFB1", "Transforming growth factor-β1",
            "TGF-β1 is a central signaling cytokine that promotes fibroblast collagen synthesis and matrix deposition, and its pathway activity is often reduced in aged skin. Both insufficient and excessive signaling are problematic — too little limits repair, while too much is linked to fibrosis — so dysregulation in either direction is the relevant read. This biomarker is therefore interpreted for balance rather than simple high-or-low, purely as a research signal.",
            new[]
            {
                "Massagué J — TGF-β signaling in tissue remodeling and fibrosis — Cell",
                "Quan T et al. — Reduced TGF-β signaling in aged human skin — Am J Pathol",
                "Representative reviews on TGF-β1 in dermal collagen regulation"
            }),

        // ---- Hydration / barrier repair domain ----
        ["CD44"] = new("CD44", "CD44 (principal hyaluronan receptor)",
            "CD44 is the main cell-surface receptor for hyaluronan and helps coordinate hyaluronan turnover, keratinocyte proliferation and epidermal hydration signaling. Reduced CD44-mediated activity is associated in the literature with impaired hyaluronan homeostasis and diminished barrier and hydration responses. A lower signal is read here as reduced hydration-axis capacity and greater biological need, in cautious terms.",
            new[]
            {
                "Kaya G et al. — CD44 in epidermal hyaluronan metabolism and hydration — Genes Dev",
                "Representative reviews on the CD44–hyaluronan axis in skin — J Invest Dermatol",
                "Representative literature on hyaluronan receptor signaling in the epidermis"
            }),
        ["AQP3"] = new("AQP3", "Aquaporin-3",
            "AQP3 is a water- and glycerol-transporting channel abundant in the epidermis that supports stratum corneum hydration and keratinocyte water balance. Lower AQP3 expression is linked in the literature to reduced skin hydration and slower barrier recovery. A comparatively low signal is interpreted here as diminished hydration-transport capacity and greater biological need, and never as a clinical finding.",
            new[]
            {
                "Hara-Chikuma M, Verkman AS — Aquaporin-3 in epidermal water and glycerol transport — J Mol Med",
                "Boury-Jamot M et al. — AQP3 expression and skin hydration — Handb Exp Pharmacol",
                "Representative reviews on aquaporins in skin barrier and hydration"
            }),
        ["FLG"] = new("FLG", "Filaggrin",
            "Filaggrin aggregates keratin filaments during cornification and is broken down into natural moisturizing factors that maintain hydration and a low, protective surface pH. Reduced filaggrin is strongly associated in the literature with impaired barrier function and dryness, most notably in barrier-compromised skin. A lower signal is read here as compromised barrier reserve and increased biological need, in non-diagnostic language.",
            new[]
            {
                "Irvine AD, McLean WHI, Leung DYM — Filaggrin mutations and skin barrier — N Engl J Med",
                "Sandilands A et al. — Filaggrin in the skin barrier and natural moisturizing factor — J Cell Sci",
                "Representative reviews on filaggrin and epidermal barrier integrity"
            }),
        ["LOR"] = new("LOR", "Loricrin",
            "Loricrin is the most abundant protein of the cornified envelope, the crosslinked structure that gives the outermost epidermis its mechanical and permeability barrier. Reduced loricrin is associated in the literature with a weaker cornified envelope and impaired barrier resilience. A lower signal is interpreted here as reduced barrier-structure reserve and greater biological need, strictly for research purposes.",
            new[]
            {
                "Representative reviews on the cornified envelope and loricrin in barrier formation — J Invest Dermatol",
                "Nithya S et al. — Cornified envelope proteins in epidermal differentiation — Int J Dermatol",
                "Representative literature on terminal keratinocyte differentiation"
            }),
        ["IL1A"] = new("IL1A", "Interleukin-1α",
            "IL-1α is a pro-inflammatory cytokine stored in keratinocytes and released on barrier disruption, acting as an early alarm signal of epidermal stress. Persistently elevated IL-1α is discussed in the literature under the concept of inflammaging and chronic low-grade barrier inflammation. Because higher levels indicate more inflammatory stress, an elevated signal is treated here as increased biological need, not as a diagnosis.",
            new[]
            {
                "Barland CO et al. — Epidermal barrier disruption and IL-1α release — J Invest Dermatol",
                "Franceschi C et al. — Inflammaging as a driver of tissue aging — Ann N Y Acad Sci",
                "Representative reviews on IL-1α in cutaneous inflammation and aging"
            }),
        ["HAS2"] = new("HAS2", "Hyaluronan synthase-2",
            "HAS2 is the dominant enzyme producing high-molecular-weight hyaluronan in skin, the glycosaminoglycan largely responsible for dermal and epidermal water retention and volume. Reduced HAS2 activity is associated in the literature with lower hyaluronan content, decreased hydration and loss of plumpness with age. A lower signal is read here as reduced hydration-synthesis capacity and greater biological need, in cautious non-diagnostic terms.",
            new[]
            {
                "Representative reviews on hyaluronan synthases and skin hydration — J Invest Dermatol",
                "Papakonstantinou E et al. — Hyaluronic acid and skin aging — Dermatoendocrinol",
                "Representative literature on HAS2 and dermal glycosaminoglycans"
            }),

        // ---- Deep ECM / connective-tissue remodeling domain ----
        ["FN1"] = new("FN1", "Fibronectin",
            "FN1 encodes fibronectin, a scaffolding glycoprotein that guides fibroblast adhesion, migration and the assembly of the collagen and elastic networks during matrix remodeling. Its expression is highly context-dependent, rising during active remodeling and repair, so both low and abnormally high levels can reflect matrix imbalance. This biomarker is therefore interpreted for dysregulation in either direction, purely as a research indicator.",
            new[]
            {
                "Representative reviews on fibronectin in extracellular-matrix assembly and remodeling — Matrix Biol",
                "To WS, Midwood KS — Fibronectin in tissue repair and matrix organization — Fibrogenesis Tissue Repair",
                "Representative literature on fibronectin in dermal connective tissue"
            }),
        ["POSTN"] = new("POSTN", "Periostin",
            "Periostin is a matricellular protein that supports collagen crosslinking and fibroblast function and is markedly upregulated during active tissue remodeling, wound healing and fibrosis. Because it is elevated in both healthy remodeling and pathological fibrosis, deviation in either direction from a balanced state is the relevant read. It is interpreted here for dysregulation rather than simple high-or-low, in strictly non-diagnostic language.",
            new[]
            {
                "Representative reviews on periostin in matrix remodeling and fibrosis — J Cell Commun Signal",
                "Nishiyama T et al. — Periostin in collagen fibrillogenesis and skin — J Invest Dermatol",
                "Representative literature on matricellular proteins in connective tissue"
            }),
        ["DCN"] = new("DCN", "Decorin",
            "Decorin is a small leucine-rich proteoglycan that binds collagen fibrils and regulates their orderly spacing and diameter, contributing to matrix organization and mechanical quality. Reduced decorin is associated in the literature with disorganized collagen architecture and altered tissue integrity with age. A lower signal is interpreted here as diminished matrix-organization reserve and greater biological need, and never as a clinical determination.",
            new[]
            {
                "Iozzo RV, Schaefer L — Decorin and small leucine-rich proteoglycans in matrix organization — FEBS J",
                "Li Y et al. — Decorin in collagen fibrillogenesis and skin — J Invest Dermatol",
                "Representative reviews on proteoglycans in dermal connective tissue"
            }),
        ["MMP2"] = new("MMP2", "Matrix metalloproteinase-2 (gelatinase A)",
            "MMP-2 degrades denatured collagen and basement-membrane components and participates in deep extracellular-matrix turnover and tissue remodeling. Elevated activity is associated in the literature with increased matrix breakdown at the dermal–epidermal junction and within the deep dermis in aging. Because higher levels indicate more degradation, an elevated signal is treated here as increased biological need for structural support, in non-diagnostic terms.",
            new[]
            {
                "Representative reviews on gelatinases (MMP-2/MMP-9) in extracellular-matrix turnover — Matrix Biol",
                "Pittayapruek P et al. — Matrix metalloproteinases in photoaging and remodeling — Int J Mol Sci",
                "Representative literature on MMP-2 in deep dermal matrix biology"
            }),
        ["VEGFA"] = new("VEGFA", "Vascular endothelial growth factor-A",
            "VEGF-A is the principal driver of angiogenesis and microvascular support, influencing dermal perfusion, nutrient delivery and repair capacity. Both insufficient signaling, linked to poor perfusion, and excessive signaling, linked to abnormal vascularity and redness, are discussed in the literature, so deviation in either direction is informative. This biomarker is therefore interpreted for dysregulation rather than simple high-or-low, strictly as a research signal.",
            new[]
            {
                "Ferrara N — VEGF-A biology and angiogenesis — Endocr Rev",
                "Representative reviews on cutaneous microvasculature and VEGF in skin — J Invest Dermatol",
                "Representative literature on angiogenic signaling in dermal repair"
            }),
    };

    /// <summary>Returns the curated evidence for a code, or null if not covered.</summary>
    public static Evidence? Get(string code) =>
        code is not null && All.TryGetValue(code, out var e) ? e : null;
}
