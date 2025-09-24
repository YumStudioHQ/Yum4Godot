using System;
using Godot;

namespace Yum4Godot.RuntimeLibrary.Yum4GodotAPI;

public partial class GlobalYGManager : Node
{
  #region Static class
  private static long _uidBase = (long)Time.GetUnixTimeFromSystem();
  public static YumEngineAPI.YumSubsystem Subsystem { get; private set; }
  private static int refCount;

  public static YumEngineAPI.YumSubsystem Acquire()
  {
    if (refCount == 0 && Subsystem == null)
      Subsystem = new YumEngineAPI.YumSubsystem();

    refCount++;
    return Subsystem;
  }

  public static void Release()
  {
    if (refCount > 0)
      refCount--;

    if (refCount == 0 && Subsystem != null)
    {
      Subsystem.Dispose();
      Subsystem = null;
    }
  }

  public static bool IsInitialized => Subsystem != null;

  public static ulong AskLuaState()
  {
    if (!IsInitialized) Subsystem = new();
    return Subsystem.NewState();
  }

  public static long GetUID()
  {
    return _uidBase++;
  }

  #endregion

  public override void _EnterTree() => Acquire();
  public override void _ExitTree() => Release();

  
}