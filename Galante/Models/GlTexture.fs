namespace Galante

open Silk.NET.OpenGL

type GlTexture =
    { GlTexHandle: uint32 
    ; FilePath: string
    ; Width: int
    ; Height: int
    ; DataVoidPtr: voidptr
    ; TextureTarget: TextureTarget
    ; TextureTargetGl: GLEnum
    ; Format: GLEnum
    ; InternalFormat: int
    ; WrapModeS: GLEnum
    ; WrapModeT: GLEnum
    ; TextureFilterModeMin: GLEnum
    ; TextureFilterModeMag: GLEnum
    ;}