using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// Vitals captured at the start of a visit. 1:1 with <see cref="Visit"/>. All measurements optional — clinics
/// rarely capture every field. BMI is server-computed when height + weight are both present.
/// </summary>
public sealed class VisitVitals : BaseEntity
{
    public Guid VisitId { get; set; }
    public Visit? Visit { get; set; }

    /// <summary>Systolic blood pressure in mmHg.</summary>
    public int? BloodPressureSystolic { get; set; }

    /// <summary>Diastolic blood pressure in mmHg.</summary>
    public int? BloodPressureDiastolic { get; set; }

    /// <summary>Pulse in beats per minute.</summary>
    public int? PulseBpm { get; set; }

    /// <summary>Body temperature in degrees Celsius.</summary>
    public decimal? TemperatureCelsius { get; set; }

    /// <summary>Height in centimeters.</summary>
    public decimal? HeightCm { get; set; }

    /// <summary>Weight in kilograms.</summary>
    public decimal? WeightKg { get; set; }

    /// <summary>Body Mass Index — server-computed when height and weight are both present.</summary>
    public decimal? Bmi { get; set; }

    /// <summary>Peripheral oxygen saturation percentage.</summary>
    public int? SpO2Percent { get; set; }

    /// <summary>Respiratory rate in breaths per minute.</summary>
    public int? RespirationRate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
