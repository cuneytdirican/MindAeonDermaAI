namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// Landmark-derived facial geometry for an assessment (from in-browser MediaPipe FaceMesh): symmetry,
/// proportion balances, jawline angle, plus the capture quality gate. Illustrative, non-diagnostic.
/// </summary>
public class FacialGeometry
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    /// <summary>Left/right symmetry (0–100; higher = more symmetric).</summary>
    public double? SymmetryScore { get; set; }
    /// <summary>Jawline angle in degrees (lower = sharper/more defined).</summary>
    public double? JawlineAngle { get; set; }
    /// <summary>Balance of the facial thirds (0–100; higher = closer to canonical proportions).</summary>
    public double? ThirdsBalance { get; set; }
    /// <summary>Balance of the facial fifths (0–100).</summary>
    public double? FifthsBalance { get; set; }
    /// <summary>Head yaw in degrees (0 = frontal).</summary>
    public double? Yaw { get; set; }

    /// <summary>Aggregate capture quality (0–100) from blur/brightness/resolution/face/eyes checks.</summary>
    public double? ImageQualityPercent { get; set; }
    public bool EyesOpen { get; set; }
    public bool FacePresent { get; set; }
}
