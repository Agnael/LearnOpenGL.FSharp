namespace Galante.OpenGL

open Galante.OpenGL

type GlVertexBufferObject =
    { GlVboHandle: uint32
    ; Data: single array
    ; DataByteSize: uint32
    ; DataPtr: voidptr
    ; StrideSize: int
    ; StrideByteSize: uint32
    ; VertexAttributes: GlVboAttribute array
    ;}