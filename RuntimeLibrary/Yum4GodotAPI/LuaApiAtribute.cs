using System;

namespace Yum4Godot.RuntimeLibrary.Yum4GodotAPI;

[AttributeUsage(AttributeTargets.Method)]
public class LuaApiAttribute : Attribute
{
  public string Namespace { get; }
  public string Name { get; }

  public LuaApiAttribute(string ns, string name)
  {
    Namespace = ns;
    Name = name;
  }
}
