local Godot = {}

---@class _Node
_Node = {}
_Node.INVALID_UID = 0

---@param name string
---@return integer
function _Node.get_node(name) return _Node.INVALID_UID end

---@param uid integer
---@param method string
---@param ... any
---@return ...
function _Node.call(uid, method, ...)
  return "<err>"
end

---@param uid integer
---@return boolean
function _Node.is_bad_node(uid) return false end

---@param uid integer
---@return string
function _Node.name(uid)
  return "<err>"
end


---@param type string
---@param name string?
---@return integer
function _Node.spawn(type, name)
  return _Node.INVALID_UID
end

---adds nodes
---@param uid integer
---@param ... any
---@return integer
function _Node.add_children(uid, ...)
  return 0
end

---@class Godot.Node
---@private uid integer
Godot.Node = {}
Godot.Node.__index = Godot.Node
Godot.Node.INVALID_UID = _Node.INVALID_UID

---spawns a new node, returning its UID.
---@param type string
---@param name string?
---@return integer
---@nodiscard
function Godot.Node.spawn(type, name)
  return _Node.spawn(type, name)
end

---@param uid? integer
---@return Godot.Node
function Godot.Node:new(uid)
  local o = setmetatable({}, self)
  o.uid = uid or _Node.INVALID_UID
  return o
end

---@param name string
---@return Godot.Node
function Godot.Node:get_node(name)
  -- call into backend to resolve uid
  local uid = _Node.get_node(name)
  return Godot.Node:new(uid)
end

--- Check if this Node's UID is valid
---@return boolean
function Godot.Node:is_bad_node()
  return _Node.is_bad_node(self.uid)
end

--- Call a method on this Node
---@param method string
---@param ... any
---@return ...
function Godot.Node:call(method, ...)
  return _Node.call(self.uid, method, ...)
end

function Godot.Node:add_children(...)
  return _Node.add_children(self.uid, ...)
end

---returns node's name
---@return string
function Godot.Node:name()
  return _Node.name(self.uid)
end

---@return integer
function Godot.Node:get()
  return self.uid
end

---@param uid integer
function Godot.Node:set(uid)
  self.uid = uid
end

function Godot.Node:_ready() end
---@param delta number
function Godot.Node:_process(delta) end
---@param delta number
function Godot.Node:_physics_process(delta) end
function Godot.Node:_enter_tree() end
function Godot.Node:_exit_tree() end
---@param what integer
function Godot.Node:_notification(what) end

---@class Godot.Node
---@field get_node fun(self: Godot.Node, name: string): Godot.Node
---@field is_bad_node fun(self: Godot.Node): boolean
---@field call fun(self: Godot.Node, mename: string, ...): any
---@field name fun(self: Godot.Node): string
---@field new fun(uid?: integer): Godot.Node
---@field set fun(uid: integer)
---@field get fun(): integer
---@field add_children fun(...): integer

---prints given arguments to Godot's output
function Godot.print(...) end
---pushes warning
function Godot.warn(...) end
---pushes error
function Godot.error(...) end
---prints non-formated arguments to Godot's output
function Godot.wraw(...) end

_G.Godot = Godot
return Godot