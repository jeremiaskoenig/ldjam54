using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	public WorldGenerator WorldGenerator  { get; private set; }

	private readonly Dictionary<string, object> configuration = new();

	public T GetConfig<T>(string key)
	{
		return (T)Convert.ChangeType(configuration[key], typeof(T));
	}

	public override void _Ready()
	{
		foreach (string key in GetMetaList())
		{
			if (key.StartsWith("config_"))
			{
				var meta = GetMeta(key);
				switch (meta.VariantType)
				{
					case Variant.Type.Bool:
						configuration[key[7..]] = (bool)meta;
						break;
					case Variant.Type.Int:
						configuration[key[7..]] = (int)meta;
						break;
					case Variant.Type.Float:
						configuration[key[7..]] = (float)meta;
						break;
					case Variant.Type.String:
					case Variant.Type.StringName:
						configuration[key[7..]] = (string)meta;
						break;
					default:
						break;
				}
			}
		}

		var roomTemplate = GetNode<TileMap>("RoomTemplate");
		var worldMap = GetNode<TileMap>("World");
		WorldGenerator = new WorldGenerator(this, worldMap, roomTemplate);
		WorldGenerator.Generate();
	}
}
