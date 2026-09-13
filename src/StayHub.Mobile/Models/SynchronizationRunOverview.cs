using System.ComponentModel;
using System.Runtime.CompilerServices;
using StayHub.Contracts.Synchronization;

namespace StayHub.Mobile.Models;

public sealed class SynchronizationRunOverview : INotifyPropertyChanged
{
    private bool _isExpanded;

    private SynchronizationRunOverview(SynchronizationRunResponse response)
    {
        Id = response.Id;
        SourceName = response.SourceName;
        Status = response.Status;
        StartedAtUtc = response.StartedAtUtc;
        CompletedAtUtc = response.CompletedAtUtc;
        CreatedCount = response.CreatedCount;
        UpdatedCount = response.UpdatedCount;
        UnchangedCount = response.UnchangedCount;
        ConflictCount = response.ConflictCount;
        FailedCount = response.FailedCount;
        ErrorMessage = response.ErrorMessage;
        Errors = string.IsNullOrWhiteSpace(response.ErrorMessage)
            ? []
            : response.ErrorMessage
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public int Id { get; }
    public string SourceName { get; }
    public string Status { get; }
    public DateTime StartedAtUtc { get; }
    public DateTime? CompletedAtUtc { get; }
    public int CreatedCount { get; }
    public int UpdatedCount { get; }
    public int UnchangedCount { get; }
    public int ConflictCount { get; }
    public int FailedCount { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyList<string> Errors { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
                return;

            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpansionIndicator));
        }
    }

    public string StatusDisplayName => Status switch
    {
        "CompletedWithErrors" => "Completed with errors",
        "Running" => "In progress",
        _ => Status
    };

    public string StartedLabel => StartedAtUtc.ToLocalTime().ToString("dd MMM yyyy, HH:mm");
    public string CompletedLabel => CompletedAtUtc is null
        ? "Not completed"
        : $"Finished {CompletedAtUtc.Value.ToLocalTime():dd MMM yyyy, HH:mm}";
    public string DurationLabel => CompletedAtUtc is null
        ? "Synchronization is still running"
        : $"Duration: {FormatDuration(CompletedAtUtc.Value - StartedAtUtc)}";
    public string CreatedLabel => CreatedCount.ToString();
    public string UpdatedLabel => UpdatedCount.ToString();
    public string UnchangedLabel => UnchangedCount.ToString();
    public string ConflictLabel => ConflictCount.ToString();
    public string FailedLabel => FailedCount.ToString();
    public string ExpansionIndicator => IsExpanded ? "⌃" : "⌄";
    public bool HasConflicts => ConflictCount > 0;
    public bool HasErrors => Errors.Count > 0;

    public string ConflictDescription => ConflictCount == 1
        ? "1 imported booking overlaps another booking for the same property."
        : $"{ConflictCount} imported bookings overlap other bookings for the same property.";

    public string StatusColor => Status switch
    {
        "Completed" => "#356859",
        "CompletedWithErrors" => "#C77800",
        "Failed" => "#B3261E",
        "Running" => "#19788C",
        _ => "#5F6368"
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    public static SynchronizationRunOverview FromResponse(SynchronizationRunResponse response) => new(response);

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 1)
            return "less than a second";
        if (duration.TotalMinutes < 1)
            return $"{Math.Ceiling(duration.TotalSeconds):0} sec";

        return $"{Math.Ceiling(duration.TotalMinutes):0} min";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
