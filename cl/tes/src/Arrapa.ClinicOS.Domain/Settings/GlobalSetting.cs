using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Settings;

/// <summary>
/// Platform-wide configuration entry managed by super-admins at runtime.
/// </summary>
public sealed class GlobalSetting : BaseEntity, IAggregateRoot
{
    [MaxLength(128)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Category { get; set; } = "General";

    public SettingDataType DataType { get; set; } = SettingDataType.String;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsSecret { get; set; }
}
