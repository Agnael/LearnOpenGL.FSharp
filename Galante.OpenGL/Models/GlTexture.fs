namespace Galante.OpenGL

open Silk.NET.OpenGL

[<Struct>]
type GlTexture = {
   GlTexHandle: uint32 
   Width: int
   Height: int
   TextureTarget: TextureTarget
   Format: PixelFormat
   InternalFormat: PixelFormat
   WrapModeS: GLEnum
   WrapModeT: GLEnum
   TextureFilterModeMin: GLEnum
   TextureFilterModeMag: GLEnum
}