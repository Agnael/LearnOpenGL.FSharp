namespace Galante.OpenGL

open Silk.NET.OpenGL

type GlVboAttrId = string

// Implementación, junto con los métodos que lo complementan 
// (que están en GL) robados de: https://stackoverflow.com/a/2267467/6466245
type GlVboBuilder =
    internal { AttrNames: string list; AttrDefinitions: single array array array; }