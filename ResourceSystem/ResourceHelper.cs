
public static class ResourceHelper
{
    public static string GetName(this ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.BuildingMaterials => "Building Materials",
            ResourceType.DuctTape => "Duct Tape",
            ResourceType.Chemicals => "Chemicals",
            ResourceType.Tools => "Tools",
            ResourceType.Circuitry => "Circurity",
            _ => "",
        };
    }
}