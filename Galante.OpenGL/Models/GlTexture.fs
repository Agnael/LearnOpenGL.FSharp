namespace Galante.OpenGL

open Silk.NET.OpenGL

[<Struct>]
type GlTexture =
    { GlTexHandle: uint32 
    //; FilePath: string
    ; Width: int
    ; Height: int
    //; DataVoidPtr: voidptr
    ; TextureTarget: TextureTarget
    ; TextureTargetGl: GLEnum
    ; Format: GLEnum
    ; InternalFormat: int
    ; WrapModeS: GLEnum
    ; WrapModeT: GLEnum
    ; TextureFilterModeMin: GLEnum
    ; TextureFilterModeMag: GLEnum
    ;}