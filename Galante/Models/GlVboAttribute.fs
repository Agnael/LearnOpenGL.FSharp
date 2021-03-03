namespace Galante

open Galante

type GlVboAttribute = { 
    AttrIdx: uint32 
    Name: string
    DataLength: int
    StrideByteSize: uint32
    OffsetInStridePtr: voidptr }