namespace Core;
public class WoCutoffOptions
{
    public WoCutoffOptions()
    {
    }

    public WoCutoffOptions(string? enabledPlants)
    {
        EnabledPlants = enabledPlants;
    }

    public string? EnabledPlants { get; set; }
}