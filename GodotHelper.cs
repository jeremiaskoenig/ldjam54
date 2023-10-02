using Godot;

public static class GodotHelper
{
    public static Variant SafeGetMeta(this GodotObject obj, string key, Variant defaultValue) => obj.HasMeta(key) ? obj.GetMeta(key) : defaultValue;
}