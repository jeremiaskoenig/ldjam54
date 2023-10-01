using Godot;
using System;
using System.Collections.Generic;

public class ResourceManager
{
    private Main main;
    public Dictionary<ResourceType, int> Resources { get; } = new();

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
        GD.Print($"{resource} is now at {Resources[resource]}");
    }
}