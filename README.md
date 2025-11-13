# Yum4Godot

## Important — This is a public archive
Yum4Godot is now an archive. You can use it of course, but highly deprecated since the YumEngine-v2.x release.

---

# YumEngine Runtime API Documentation

This document provides an overview of the `YumEngineRuntimeInfo`, `YumVariant`, `YumVector`, and `YumSubsystem` classes (C#), as well as the Lua-side `Godot.Node` bindings.
The goal is to expose YumEngine’s runtime and integration layer for Godot.

---

## C# Runtime Library (`Yum4Godot.RuntimeLibrary.YumEngineAPI`)

### YumEngineRuntimeInfo

Provides access to engine metadata such as versioning and studio information.

* **Name()** → `string`
* **StudioName()** → `string`
* **StudioBranch()** → `string`
* **Major()** / **Minor()** / **Patch()** → `int`
* **VersionString()** → `string`

#### Version Checks

* **RequireMin(int maj, int min, int patch)**
* **RequireMax(int maj, int min, int patch)**
* **IsSameVersion(Vector3I v)**
* **Require(Vector3I min, Vector3I max, Vector3I\[] excludes)**

---

### YumVariant

Wrapper around a variant value (int, float, bool, string). Supports implicit conversions.

* **Constructors**: `YumVariant(long|double|bool|string)`
* **Set(...)** / **As...()** methods for each type
* **IsInt / IsFloat / IsBool / IsString** → `bool`
* Implicit conversions both ways
* **ToString()** returns stringified value
* Implements **IDisposable** (releases native handle)

---

### YumVector

Managed wrapper over a native vector of `YumVariant`.

* **Append(YumVariant v)** / **Add(...)** overloads
* **Pop()**, **Clear()**
* **Count** → `long`
* Indexer: `this[long index]`
* Implements `IEnumerable<YumVariant>`
* **Format(string delimiter)** for string joining
* Implements **IDisposable**

---

### YumSubsystem

Represents a YumEngine subsystem, capable of managing script states and interop.

* **NewState(bool loadStdLibs = true)** → `ulong`
* **DeleteState(ulong uid)**
* **IsValidUID(ulong uid)** → `bool`
* **Load(ulong uid, string source, bool isFile)** → `int`
* **Good(ulong uid)** → `bool`
* **Call(ulong uid, string name, YumVector args)** → `YumVector`
* **PushCallback(ulong uid, string name, Func\<YumVector,YumVector> func, string ns = "")**
* **HasMethod(ulong uid, string path)** → `bool`
* Implements **IDisposable** with finalizer

---

## Lua Runtime Bindings (`Godot`)

The Lua layer provides lightweight bindings to Godot nodes.

### `_Node` (internal helpers)

* **INVALID\_UID** → `0`
* **get\_node(name: string) → integer**
* **call(uid: integer, method: string, ...) → any**
* **is\_bad\_node(uid: integer) → boolean**
* **name(uid: integer) → string**
* **spawn(type: string, name?: string) → integer**
* **add\_children(uid: integer, ...) → integer**

---

### `Godot.Node`

A thin object wrapper over `_Node`.

* **INVALID\_UID** → `0`
* **new(uid?: integer) → Godot.Node**
* **spawn(type: string, name?: string) → integer**
* **get\_node(name: string) → Godot.Node**
* **is\_bad\_node() → boolean**
* **call(method: string, ...) → any**
* **add\_children(...) → integer**
* **name() → string**
* **get() → integer**
* **set(uid: integer)**

**NOTE**: For the moment, signals are **not** supported. We're working on their support.

---

### Output Functions

* **Godot.print(...)**: Prints into Godot's console. New line is added at the end of the call.
* **Godot.warn(...)**: Pushes a warning
* **Godot.error(...)**: Pushes an error
* **Godot.wraw(...)**: Writes raw string to Godot's console.
