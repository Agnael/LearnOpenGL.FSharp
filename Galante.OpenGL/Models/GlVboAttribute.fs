namespace Galante.OpenGL

open Galante.OpenGL

type GlVboAttribute = { 
    AttrIdx: uint32 
    Name: string
    DataLength: int
    StrideByteSize: uint32
    OffsetInStridePtr: voidptr }