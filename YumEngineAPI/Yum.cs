using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

namespace Yum4Godot.YumEngineAPI
{
  public static class YumEngineRuntimeInfo
  {
    public static string Name() => Native.SafeIt(Native.YumEngineInfo_name());
    public static string StudioName() => Native.SafeIt(Native.YumEngineInfo_studioName());
    public static string StudioBranch() => Native.SafeIt(Native.YumEngineInfo_branch());
    public static int Major() => Native.YumEngineInfo_versionMajor();
    public static int Minor() => Native.YumEngineInfo_versionMinor();
    public static int Patch() => Native.YumEngineInfo_versionPatch();

    private static int CompareVersion(int aMaj, int aMin, int aPatch, int bMaj, int bMin, int bPatch)
    {
      if (aMaj != bMaj) return aMaj.CompareTo(bMaj);
      if (aMin != bMin) return aMin.CompareTo(bMin);
      return aPatch.CompareTo(bPatch);
    }

    public static bool RequireMin(int maj, int min, int patch)
    {
      var cmp = CompareVersion(Major(), Minor(), Patch(), maj, min, patch);
      return cmp >= 0;
    }

    public static bool RequireMin(Godot.Vector3I v) => RequireMin(v.X, v.Y, v.Z);

    public static bool RequireMax(int maj, int min, int patch)
    {
      var cmp = CompareVersion(Major(), Minor(), Patch(), maj, min, patch);
      return cmp <= 0;
    }

    public static bool RequireMax(Godot.Vector3I v) => RequireMax(v.X, v.Y, v.Z);

    public static bool IsSameVersion(Godot.Vector3I v)
    {
      return v.X == Major() && v.Y == Minor() && v.Z == Patch();
    }

    public static bool Require(Godot.Vector3I min, Godot.Vector3I max, Godot.Vector3I[] excludes)
    {
      if (!RequireMin(min)) return false;
      if (!RequireMax(max)) return false;

      if (excludes != null)
      {
        foreach (var exclude in excludes)
          if (IsSameVersion(exclude)) return false;
      }

      return true;
    }

    public static string VersionString()
    {
      return $"{Major()}.{Minor()}.{Patch()}";
    }

    public static string WellVersionString()
    {
      return $"{StudioName()}.{StudioBranch()}.{VersionString()}";
    }
  }

  public class YumVariant : IDisposable
  {
    internal IntPtr Handle { get; private set; }

    public YumVariant()
    {
      Handle = Native.YumVariant_new();
    }

    public YumVariant(long v) : this() => Set(v);
    public YumVariant(double v) : this() => Set(v);
    public YumVariant(bool v) : this() => Set(v);
    public YumVariant(string v) : this() => Set(v);

    internal YumVariant(IntPtr handle)
    {
      Handle = handle;
    }

    public void Set(long v) => Native.YumVariant_setInt(Handle, v);
    public void Set(double v) => Native.YumVariant_setFloat(Handle, v);
    public void Set(bool v) => Native.YumVariant_setBool(Handle, v ? 1 : 0);
    public void Set(string v) => Native.YumVariant_setString(Handle, v);

    public long AsInt() => Native.YumVariant_asInt(Handle);
    public double AsFloat() => Native.YumVariant_asFloat(Handle);
    public bool AsBool() => Native.YumVariant_asBool(Handle) != 0;
    public string AsString() => Native.YumVariant_asStringSafe(Handle);

    public bool IsInt => Native.YumVariant_isInt(Handle) != 0;
    public bool IsFloat => Native.YumVariant_isFloat(Handle) != 0;
    public bool IsBool => Native.YumVariant_isBool(Handle) != 0;
    public bool IsString => Native.YumVariant_isString(Handle) != 0;

    // --- Implicit conversions ---
    public static implicit operator YumVariant(long v) => new YumVariant(v);
    public static implicit operator YumVariant(double v) => new YumVariant(v);
    public static implicit operator YumVariant(bool v) => new YumVariant(v);
    public static implicit operator YumVariant(string v) => new YumVariant(v);

    public static implicit operator long(YumVariant v) => v.AsInt();
    public static implicit operator double(YumVariant v) => v.AsFloat();
    public static implicit operator bool(YumVariant v) => v.AsBool();
    public static implicit operator string(YumVariant v) => v.AsString();
    public override string ToString()
    {
      if (IsInt) return AsInt().ToString();
      if (IsFloat) return AsFloat().ToString();
      if (IsBool) return AsBool().ToString();
      if (IsString) return AsString();
      return "<nil>";
    }

    public string AsLiteralValue()
    {
      if (IsString) return $"\"{AsString()}\"";
      return ToString();
    }

    public void Dispose()
    {
      if (Handle != IntPtr.Zero)
      {
        Native.YumVariant_delete(Handle);
        Handle = IntPtr.Zero;
      }
    }
  }

  public class YumVector : IDisposable, IEnumerable<YumVariant>
  {
    internal IntPtr Handle { get; private set; }
    private readonly bool ownsHandle;

    public YumVector()
    {
      Handle = Native.YumVector_new();
      ownsHandle = true;
    }

    internal YumVector(IntPtr handle, bool ownsHandle = false)
    {
      Handle = handle;
      this.ownsHandle = ownsHandle;
    }

    public void Dispose()
    {
      if (ownsHandle && Handle != IntPtr.Zero)
      {
        Native.YumVector_delete(Handle);
        Handle = IntPtr.Zero;
      }
    }

    public void Append(YumVariant v) => Native.YumVector_append(Handle, v.Handle);

    public void Add(YumVariant v) => Append(v);

    // Sugar: collection initializer support
    public void Add(long v) => Append(new YumVariant(v));
    public void Add(double v) => Append(new YumVariant(v));
    public void Add(bool v) => Append(new YumVariant(v));
    public void Add(string v) => Append(new YumVariant(v));

    public void Pop() => Native.YumVector_pop(Handle);
    public void Clear() => Native.YumVector_clear(Handle);
    public long Count => Native.YumVector_size(Handle);

    public YumVariant this[long index]
    {
      get
      {
        var ptr = Native.YumVector_at(Handle, index);
        return new YumVariant(ptr);
      }
    }

    public IEnumerator<YumVariant> GetEnumerator()
    {
      for (long i = 0; i < Count; i++)
        yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
      var items = new List<string>();
      foreach (var v in this)
        items.Add(v.ToString());
      return string.Join(", ", items);
    }

    public string Format(string del)
    {
      var items = new List<string>();
      foreach (var v in this)
        items.Add(v.ToString());
      return string.Join(del, items);
    }
  }

  public class YumSubsystem : IDisposable
  {
    private IntPtr handle;
    private bool disposed;

    public YumSubsystem()
    {
      handle = Native.YumSubsystem_new();
      if (handle == IntPtr.Zero)
        throw new Exception("Failed to create subsystem");
    }

    public ulong NewState(bool loadStdLibs = true) =>
        Native.YumSubsystem_newState(handle, loadStdLibs ? 1 : 0);

    public void DeleteState(ulong uid) =>
        Native.YumSubsystem_deleteState(handle, uid);

    public bool IsValidUID(ulong uid) =>
        Native.YumSubsystem_isValidUID(handle, uid) != 0;

    public int Load(ulong uid, string source, bool isFile) =>
        Native.YumLuaSubsystem_load(handle, uid, source, isFile);

    public bool Good(ulong uid) =>
        Native.YumLuaSubsystem_good(handle, uid);

    public YumVector Call(ulong uid, string name, YumVector args)
    {
      var res = Native.YumLuaSubsystem_call(handle, uid, name, args.Handle);
      return new YumVector(res);
    }

    public void PushCallback(ulong uid, string name, Func<YumVector, YumVector> func, string ns = "")
    {
      Native.YumCallback cb = (IntPtr inVecPtr, IntPtr outVecPtr) =>
      {
        try
        {
          var input = new YumVector(inVecPtr, ownsHandle: false);

          // Borrowed output → does not own
          var outputVec = new YumVector(outVecPtr, ownsHandle: false);

          var result = func(input);

          foreach (var v in result) outputVec.Add(v);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine($"[YumSubsystem] Callback '{name}' threw: {ex}");
          // IMPORTANT: swallow exception, don’t let it escape to native
        }
      };

      _callbacks.Add(cb); // keep alive

      var result = Native.YumLuaSubsystem_pushCallback(handle, uid, name, cb, ns);
      if (result != 0)
        throw new Exception($"Failed to push callback {name}");
    }

    public bool HasMethod(ulong uid, string path)
    {
      return Native.YumLuaSubsystem_hasMethod(handle, uid, path) != 0;
    }

    // Store delegates to prevent GC
    private readonly List<Delegate> _callbacks = [];

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this); // don’t run finalizer if already disposed
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (handle != IntPtr.Zero)
        {
          Native.YumSubsystem_delete(handle);
          handle = IntPtr.Zero;
        }

        disposed = true;
      }
    }

    ~YumSubsystem()
    {
      Dispose(false);
    }
  }

  public static class IO
  {
    private static Native.YumRedirectionCallback _gOutCallback;
    private static Native.YumRedirectionCallback _gErrCallback;

    public static void RedirectGOut(Action<string> action)
    {
      _gOutCallback = msg =>
      {
        try
        {
          if (msg != null)
            action(msg);
        }
        catch (Exception ex)
        {
          GD.PrintErr($"[Yum IO] RedirectGOut error: {ex}");
        }
      };

      Native.Yum_redirect_G_out(_gOutCallback);
    }

    public static void RedirectGErr(Action<string> action)
    {
      _gErrCallback = msg =>
      {
        try
        {
          if (msg != null)
            action(msg);
        }
        catch (Exception ex)
        {
          GD.PrintErr($"[Yum IO] RedirectGOut error: {ex}");
        }
      };

      Native.Yum_redirect_G_err(_gOutCallback);
    }

    public static void OpenGOut(string path)
    {
      Native.Yum_open_G_out(path);
    }

    public static void OpenGErr(string path)
    {
      Native.Yum_open_G_err(path);
    }
    
    public static void OpenGIn(string path)
    {
      Native.Yum_open_G_in(path);
    }
  }
}
