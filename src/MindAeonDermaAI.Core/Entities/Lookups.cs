namespace MindAeonDermaAI.Core.Entities;

// Simple categorical lookups. All inherit the shared Smart-Select behaviour from LookupBase.
// Each is its own table so relationships stay strongly typed and seed data is isolated.

public class Sex : LookupBase { }
public class SkinType : LookupBase { }
public class FitzpatrickType : LookupBase { }
public class SmokingStatus : LookupBase { }
public class AlcoholUse : LookupBase { }
public class SleepQuality : LookupBase { }
public class StressLevel : LookupBase { }
public class UVExposureLevel : LookupBase { }
public class MenopauseStatus : LookupBase { }
public class AestheticProcedure : LookupBase { }
public class InterventionCategory : LookupBase { }
public class MeasurementUnit : LookupBase { }
public class PhenotypeType : LookupBase { }
public class FacialZone : LookupBase { }        // regional facial mapping (heatmap)
public class InstrumentType : LookupBase { }     // multi-omics / instrumental modalities (TEWL, corneometry, …)
public class WearableSource : LookupBase { }     // Apple Watch, Garmin, Fitbit, Oura, Whoop, Manual
public class WearableMetricType : LookupBase { } // Sleep, Stress, HRV, Recovery, Steps, Resting HR
public class OmicsType : LookupBase { }          // Gene variant, SNP, Expression, Methylation
