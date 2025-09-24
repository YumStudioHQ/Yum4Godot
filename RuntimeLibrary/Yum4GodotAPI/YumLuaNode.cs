using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Godot;
using Yum4Godot.RuntimeLibrary.YumEngineAPI;

namespace Yum4Godot.RuntimeLibrary.Yum4GodotAPI;

public class InternalLuaState
{
  public ulong LuaUID { get; private set; }

  public InternalLuaState()
  {
    LuaUID = GlobalYGManager.AskLuaState();
  }

  ~InternalLuaState()
  {
    GlobalYGManager.Subsystem.DeleteState(LuaUID);
  }

  public YumVector Call(string name, YumVector argv)
   => GlobalYGManager.Subsystem.Call(LuaUID, name, argv);

  public int Load(string content, bool isFile = false)
    => GlobalYGManager.Subsystem.Load(LuaUID, content, isFile);

  public void PushCallback(string name, Func<YumVector, YumVector> func, string ns = "")
    => GlobalYGManager.Subsystem.PushCallback(LuaUID, name, func, ns);

  public bool HasMethod(string path)
    => GlobalYGManager.Subsystem.HasMethod(LuaUID, path);
}

[GlobalClass]
public partial class YumLuaNode : Node
{
  private GlobalYGManager _YumManagerInstance;
  private readonly InternalLuaState localLuaState = new();

  [ExportGroup("Source code")]
  [Export] private string CodePath = "res://relative/path/to/your/lua/file";
  [Export] private string ClassName = "ClassName";
  [Export] private bool UseLuaStdLibrary = true; // TODO
  [Export] private bool UseYumEngineLibrary = true; // TODO

  [ExportGroup("Runtime Configuration")]
  [Export] private Vector3I MinimumVersion = new(1, 7, 0);
  [Export] private Vector3I MaximumVersion = new(2, 0, 0);
  [Export] private Vector3I RecommendedVersion = new(1, 6, 0);
  [Export] private Godot.Collections.Array<Vector3I> ExcludedVersions = [new(1, 5, 0)];
  [Export] private bool CanSpawnStates = false;
  [Export] private bool PrintErrors = true;
  [Export] private bool PrintWarnings = true;
  [Export] private bool MakeErrorsAsFatal = true;
  [Export] private bool MakeWarningsAsFatal = false;
  [Export] private bool MakeWarningsAsErrors = false;

  private readonly Dictionary<long, Node> NodeHandles = [];
  private readonly Dictionary<string, Type> NodeReflection = [];

  public new Variant Call(StringName name, Variant[] args)
  {
    if (localLuaState.HasMethod(name))
      return VariantConverter.ToVariant(localLuaState.Call(name, VariantConverter.ToYumVector(args)));

    return base.Call(name, args);
  }

  public override void _Ready()
  {
    if (!YumEngineRuntimeInfo.Require(MinimumVersion, MaximumVersion, [.. ExcludedVersions]))
    {
      GD.PushWarning(
        new NotSupportedException(
          $"Current version: {YumEngineRuntimeInfo.VersionString()}, required (at least): {MinimumVersion}, recommended: {RecommendedVersion}"
        )
      );
    }

    var apiFunctions = Assembly.GetExecutingAssembly()
    .GetTypes()
    .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
    .Select(m => (Method: m, Attr: m.GetCustomAttribute<LuaApiAttribute>()))
    .Where(x => x.Attr != null)
    .ToList();

    var nodeTypes = typeof(Node).Assembly
    .GetTypes()
    .Where(t => t.IsSubclassOf(typeof(Node)))
    .ToList();

    foreach (var node in nodeTypes) NodeReflection[node.Name] = node;
    
    int code = localLuaState.Load(CodePath, true);

    if (code != 0)
    {
      GD.PrintErr($"Cannot load Lua code, exit code: {code}");
    }

    foreach (var (method, attr) in apiFunctions)
    {
      var parameters = method.GetParameters();
      bool correctSignature = parameters.Length == 1 &&
                              parameters[0].ParameterType == typeof(YumVector) &&
                              method.ReturnType == typeof(YumVector);

      if (!correctSignature)
      {
        GD.PushWarning($"Skipping method {method.Name}: must have signature YumVector -> YumVector");
        continue;
      }

      Func<YumVector, YumVector> del;
      if (method.IsStatic)
      {
        del = (Func<YumVector, YumVector>)Delegate.CreateDelegate(typeof(Func<YumVector, YumVector>), method);
      }
      else
      {
        del = (Func<YumVector, YumVector>)Delegate.CreateDelegate(typeof(Func<YumVector, YumVector>), this, method);
      }

      localLuaState.PushCallback(attr.Name, del, attr.Namespace);
    }

    var uidOfThis = GlobalYGManager.GetUID();
    NodeHandles[uidOfThis] = this;

    localLuaState.Load($"{ClassName}:set({uidOfThis})\n{ClassName}:_ready()");
  }

  public override void _EnterTree()
  {
    localLuaState.Load($"{ClassName}:_enter_tree()");
  }

  public override void _ExitTree()
  {
    localLuaState.Load($"{ClassName}:_exit_tree()");
  }

  public override void _Process(double delta)
  {
    localLuaState.Load($"{ClassName}:_process({delta})");
  }

  public override void _PhysicsProcess(double delta)
  {
    localLuaState.Load($"{ClassName}:_physics_process({delta})");
  }

  public override void _Notification(int what)
  {
    localLuaState.Load($"{ClassName}:_notification({what})");
  }


  private void WriteE(string from, string what)
  {
    var s = $"err: {from}: {what}";
    if (MakeErrorsAsFatal)
      throw new Exception(s) {};
    if (PrintWarnings)
      GD.PushError(s);
  }

  private void WriteW(string from, string what)
  {
    var s = $"warn: {from}: {what}";
    if (MakeWarningsAsFatal)
      throw new Exception(s) {};
    if (PrintWarnings)
      if (MakeWarningsAsErrors) GD.PushError(s);
      else GD.PushWarning(s);
  }

  #region Lua Callbacks

  public const long NullUID = 0;

  [LuaApi("Godot", "print")]
  private static YumVector CBL_print(YumVector args)
  {
    GD.Print(args.Format(""));
    return [];
  }

  [LuaApi("Godot", "error")]
  private static YumVector CBL_error(YumVector args)
  {
    GD.PushError(args.Format(""));
    return [];
  }

  [LuaApi("Godot", "warn")]
  private static YumVector CBL_warn(YumVector args)
  {
    GD.PushWarning(args.Format(""));
    return [];
  }


  [LuaApi("Godot", "wraw")]
  private static YumVector CBL_wraw(YumVector args)
  {
    GD.PrintRaw(args.Format(""));
    return [];
  }

  [LuaApi("_Node", "get_node")]
  private YumVector CBL_get_node(YumVector args)
  {
    if (args.Count < 1)
    {
      WriteW(nameof(CBL_get_node), "not enough arguments, returning 0");
      return [NullUID];
    }

    if (!args[0].IsString)
    {
      WriteE(nameof(CBL_get_node), "invalid argument, expected (string, string)");
      return [NullUID];
    }

    var uid = GlobalYGManager.GetUID();

    try
    {
      NodeHandles[uid] = GetNode<Node>(args[0].AsString());
    }
    catch (Exception e)
    {
      WriteE(nameof(CBL_get_node), $"Runtime Exception {e}");
      NodeHandles[uid] = new BadNode();
    }

    return [uid];
  }

  [LuaApi("_Node", "is_bad_node")]
  private YumVector CBL_is_bad_node(YumVector args)
  {
    if (args.Count > 0)
      if (args[0].IsInt)
      {
        if (NodeHandles[args[0].AsInt()] is BadNode bad)
        {
          return [false];
        }
        return [true];
      }

    return [false];
  }

  [LuaApi("_Node", "call")]
  private YumVector CBL_call(YumVector args)
  {
    if (args.Count >= 2 && args[0].IsInt && args[1].IsString)
    {
      var uid = args[0].AsInt();
      var methodName = args[1].AsString();
      var methodArgs = args.Skip(2).ToArray();

      if (NodeHandles.TryGetValue(uid, out var node) && node != null)
      {
        var result = node.Call(methodName, VariantConverter.ToVariantArray(methodArgs));
        return VariantConverter.ToYumVector(result.AsGodotArray());
      }
      else
      {
        WriteE(nameof(CBL_call), $"Node with UID {uid} not found.");
      }
    }

    return [];
  }

  [LuaApi("_Node", "spawn")]
  private YumVector CBL_spawn(YumVector args)
  {
    if (args.Count >= 1)
    {
      if (args[0].IsString)
      {
        var key = args[0].AsString();
        if (NodeReflection.TryGetValue(key, out Type value))
        {
          var name = (args.Count >= 2 && args[1].IsString ) ? args[1].AsString() : $"{value.Name}_{GlobalYGManager.GetUID()}";
          var instance = (Node)Activator.CreateInstance(value)!;
          var uid = GlobalYGManager.GetUID();
          instance.Name = name;
          NodeHandles[uid] = instance;
          return [uid];
        }
        else
          WriteE(nameof(CBL_spawn), "type {key} isn't present in current reflection --unknown type");
      }
      else WriteE(nameof(CBL_spawn), "expected string as argument #1");
    }
    else WriteW(nameof(CBL_spawn), "not enough arguments");

    return [NullUID];
  }

  [LuaApi("_Node", "add_children")]
  private YumVector CBL_add_children(YumVector args)
  {
    if (args.Count < 1 || !args[0].IsInt)
    {
      WriteE(nameof(CBL_add_children), "First argument must be an integer");
      return [];
    }

    long uid = args[0].AsInt();
    long pushed = 0;

    if (NodeHandles.TryGetValue(uid, out Node targ))
    {
      var rest = args.Skip(1);

      foreach (var arg in rest)
      {
        if (arg.IsInt && NodeHandles.TryGetValue(arg.AsInt(), out Node node))
        {
          targ.AddChild(node);
          pushed++;
        }
      }
    }
    else
    {
      WriteE(nameof(CBL_add_children), $"Node with UID {uid} not found.");
    }

    return [pushed];
  }

  [LuaApi("_Node", "name")]
  private YumVector CBL_name(YumVector args)
  {
    if (args.Count >= 1 && args[0].IsInt)
    {
      var uid = args[0].AsInt();
      if (NodeHandles.TryGetValue(uid, out Node value))
      {
        var n = value.Name.ToString();
        return [n];
      }

      WriteE(nameof(CBL_name), $"Invalid UID: {uid}");
      return ["<null>"];
    }

    WriteW(nameof(CBL_name), "Not enough arguments, returning '<null>' string");

    return ["<null>"];
  }

  #endregion
}