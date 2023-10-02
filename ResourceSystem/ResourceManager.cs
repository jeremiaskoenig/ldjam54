using Godot;
using System;
using System.Collections.Generic;

public class ResourceManager
{
    private Main main;
    public Dictionary<ResourceType, int> Resources { get; } = new();

    public ResourceBucket ResourceBucket { get; } = new();

    public ResourceManager(Main main)
    {
        this.main = main;
        foreach (var resource in Enum.GetValues<ResourceType>())
        {
            Resources[resource] = 0;
        }
    }

    public void AddResource(ResourceType resource, int amount)
    {
        Resources[resource] += amount;
    }

    public void AddFromResourceBucket()
    {
        var (resource, amount) = ResourceBucket.GetNext();
        AddResource(resource, amount);
    }

    public bool CanAfford(IEnumerable<(ResourceType type, int amount)> cost)
    {
        bool canAfford = true;
        foreach (var costEntry in cost)
        {
            canAfford &= main.ResourceManager.Resources[costEntry.type] >= costEntry.amount;
        }
        return canAfford;
    }

    internal void Pay(IEnumerable<(ResourceType type, int amount)> cost)
    {
        foreach (var costEntry in cost)
        {
            Resources[costEntry.type] -= costEntry.amount;
        }
    }
}