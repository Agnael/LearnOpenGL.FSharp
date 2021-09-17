module Gl

#nowarn "9"
#nowarn "51"
open Galante.OpenGL
open Silk.NET.OpenGL
open Microsoft.FSharp.NativeInterop

let glActiveTexture (glTextureSlotId: GLEnum) ctx =
   ctx.Gl.ActiveTexture (glTextureSlotId)
   ctx

let glBindTexture (textureTarget: TextureTarget) (handle: uint32) ctx =
   ctx.Gl.BindTexture (textureTarget, handle)
   ctx

let glTexParameterI 
   (target: TextureTarget) 
   (paramName: TextureParameterName) 
   (valueEnum: GLEnum) 
   ctx = 
      let mutable value = LanguagePrimitives.EnumToValue valueEnum
      let valueIntPtr = NativePtr.toNativeInt<int> &&value
      let valueNativePtr: nativeptr<int> = NativePtr.ofNativeInt valueIntPtr

      ctx.Gl.TexParameterI (target, paramName, valueNativePtr)
      ctx
      
let glTexParameterIcubeMap = glTexParameterI TextureTarget.TextureCubeMap
let glTexParameterI2d = glTexParameterI TextureTarget.Texture2D

// Indentation hell, want to keep code under 80 columns, but this looks so bad
let glTexImage2d 
   (textureTarget: TextureTarget)
   (lvl: int )
   (internalFormat: PixelFormat)
   (w: int )
   (h: int)
   (border: int)
   (pxFormat: PixelFormat)
   (pxType: PixelType)
   (pixels: voidptr)
   (ctx: GlWindowCtx) =
   ctx.Gl.TexImage2D 
      (textureTarget
      , lvl
      , LanguagePrimitives.EnumToValue internalFormat
      , uint32 w
      , uint32 h
      , border
      , pxFormat
      , pxType
      , pixels)
   ctx

let glGenerateMipmap (textureTarget: TextureTarget) ctx = 
   ctx.Gl.GenerateMipmap textureTarget
   ctx

let glEnable (enableCap: EnableCap) ctx = 
   ctx.Gl.Enable (enableCap)
   ctx
