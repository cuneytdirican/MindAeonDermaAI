using MindAeonDermaAI.Analysis.Models;

namespace MindAeonDermaAI.Analysis.Engine;

/// <summary>Deterministic, reproducible scoring engine implementing the report methodology.</summary>
public interface IAnalysisEngine
{
    AnalysisResult Analyze(AnalysisInput input);
}

/// <summary>Stable panel / phenotype codes shared with the seeder.</summary>
public static class PanelCodes
{
    public const string Collagen = "PANEL_COLLAGEN";
    public const string Barrier = "PANEL_BARRIER";
    public const string DeepEcm = "PANEL_DEEPECM";
}

public static class PhenotypeCodes
{
    public const string BarrierDeficient = "BARRIER_DEF";
    public const string CollagenDepleted = "COLLAGEN_DEP";
    public const string Inflammaging = "INFLAMMAGING";
    public const string DeepEcm = "DEEP_ECM";
    public const string Fibrosis = "FIBROSIS";
    public const string MechStress = "MECH_STRESS";
    public const string YoungBalanced = "YOUNG_BALANCED";
    public const string Mixed = "MIXED";
}
