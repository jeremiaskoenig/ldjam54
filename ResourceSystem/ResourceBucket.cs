using Godot;
using System.Collections.Generic;

public class ResourceBucket
{
    private List<BucketEntry> bucketContents = new();

    public (ResourceType resource, int amount) GetNext()
    {
        if (bucketContents.Count == 0)
            Refill();

        var result = bucketContents[GD.RandRange(0, bucketContents.Count - 1)];
        bucketContents.Remove(result);
        return result.ToTuple();
    }

    private void Refill()
    {
        bucketContents.Add(new(ResourceType.BuildingMaterials, 5));
        bucketContents.Add(new(ResourceType.BuildingMaterials, 4));
        bucketContents.Add(new(ResourceType.BuildingMaterials, 3));
        bucketContents.Add(new(ResourceType.BuildingMaterials, 2));
        bucketContents.Add(new(ResourceType.Tools, 5));
        bucketContents.Add(new(ResourceType.Tools, 4));
        bucketContents.Add(new(ResourceType.Tools, 3));
        bucketContents.Add(new(ResourceType.DuctTape, 4));
        bucketContents.Add(new(ResourceType.DuctTape, 4));
        bucketContents.Add(new(ResourceType.DuctTape, 3));
        bucketContents.Add(new(ResourceType.DuctTape, 3));
        bucketContents.Add(new(ResourceType.Chemicals, 5));
        bucketContents.Add(new(ResourceType.Chemicals, 4));
        bucketContents.Add(new(ResourceType.Chemicals, 3));
        bucketContents.Add(new(ResourceType.Circuitry, 5));
        bucketContents.Add(new(ResourceType.Circuitry, 4));
        bucketContents.Add(new(ResourceType.Circuitry, 3));
    }


    private class BucketEntry
    {
        private readonly ResourceType resource;
        private readonly int amount;

        public BucketEntry(ResourceType resource, int amount)
        {
            this.resource = resource;
            this.amount = amount;
        }

        public (ResourceType, int) ToTuple() => (resource, amount);
    }
}
