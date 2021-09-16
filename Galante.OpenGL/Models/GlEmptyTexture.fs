namespace Galante.OpenGL

open Silk.NET.OpenGL

[<Struct>]
type GlEmptyTexture =
    { GlTexHandle: uint32 
    ; Width: int
    ; Height: int
    ;}