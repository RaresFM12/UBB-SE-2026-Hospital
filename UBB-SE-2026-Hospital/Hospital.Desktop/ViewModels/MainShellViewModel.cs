using CommunityToolkit.Mvvm.ComponentModel;

namespace Hospital.Desktop.ViewModels;

public partial class MainShellViewModel : ObservableObject
{
    public string Title { get; } = "UBB-SE-2026-Hospital";

    public string Subtitle { get; } = "Unified desktop shell for Staff, ER, and Patient workflows.";

    public IReadOnlyList<string> Workstreams { get; } =
    [
        "Port 923-2 staff and pharmacy pages into Views/",
        "Port House-MD ER screens into Views/",
        "Port MysteryInc patient and billing screens into Views/",
        "Move HTTP proxies into Proxy/ and wire them through DI",
    ];
}
