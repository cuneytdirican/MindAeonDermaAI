namespace MindAeonDermaAI.Core.Enums;

/// <summary>Expected direction of a biomarker change that signals increased biological need.</summary>
public enum NeedDirection
{
    /// <summary>Higher values indicate more need (e.g. MMP-1, IL-1α).</summary>
    HighIsNeed = 1,
    /// <summary>Lower values indicate more need (e.g. COL1A1, filaggrin).</summary>
    LowIsNeed = 2,
    /// <summary>Deviation in either direction (dysregulation) indicates need (e.g. TGF-β1).</summary>
    DysregulationIsNeed = 3
}

public enum TimePointType
{
    Baseline = 1,
    FollowUp = 2
}

public enum AiProviderType
{
    OpenAiCompatible = 1,
    Anthropic = 2,
    Custom = 3
}

[Flags]
public enum ModelCapability
{
    None = 0,
    Chat = 1,
    Vision = 2
}

public enum AiTaskKind
{
    Chat = 1,
    Vision = 2
}

/// <summary>Confidence level assigned by data-quality control.</summary>
public enum ConfidenceLevel
{
    High = 1,
    Moderate = 2,
    Reduced = 3,
    Low = 4
}

public enum AssessmentStatus
{
    Draft = 1,
    DataEntry = 2,
    Analyzed = 3,
    Reported = 4
}
