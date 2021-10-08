module Gl

#nowarn "9"
#nowarn "51"
open Galante.OpenGL
open Silk.NET.OpenGL
open Microsoft.FSharp.NativeInterop
open System

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

let glGetUniformBlockIndex shader (uniformBlockName: string) ctx =
   ctx.Gl.GetUniformBlockIndex (shader.GlProgramHandle, uniformBlockName)

let glUniformBlockBinding shader uniformIndex targetUniformBlockBinding ctx =
   ctx.Gl.UniformBlockBinding (
      shader.GlProgramHandle, 
      uniformIndex, 
      targetUniformBlockBinding)
   ctx

let glBindBuffer (target: BufferTargetARB) ubo ctx =
   ctx.Gl.BindBuffer (target, ubo.GlUboHandle)
   ctx

let glBufferData 
   (target: BufferTargetARB)
   size
   (dataPtr: voidptr)
   (usageType: BufferUsageARB)
   ctx =
      ctx.Gl.BufferData (target, size, dataPtr, usageType)
      ctx

let glBufferDataEmpty target size usageType ctx =
   glBufferData target size (IntPtr.Zero.ToPointer()) usageType ctx

let glBindBufferDefault (target: BufferTargetARB) ctx =
   ctx.Gl.BindBuffer (target, 0ul)
   ctx

let glBindBufferRange 
   (target: BufferTargetARB)
   targetUniformBlockBindingIndex
   bufferHandle
   offset
   size
   ctx =
      ctx.Gl.BindBufferRange (
         target,
         targetUniformBlockBindingIndex,
         bufferHandle,
         offset,
         size)

let glGetUniformIndices
   (shader: GlProgram) 
   (uniformBlock: GlUniformBlockDefinition)
   ctx =
      let mutable indices: uint32 array = 
         Array.zeroCreate uniformBlock.UniformNames.Length

      ctx.Gl.GetUniformIndices(
         shader.GlProgramHandle, 
         uint32 uniformBlock.UniformNames.Length,
         List.toArray uniformBlock.UniformNames,
         &indices.[0])

      let mutable offsetValues: int array = Array.zeroCreate 1

      ctx.Gl.GetActiveUniforms(
         shader.GlProgramHandle,
         1ul,
         &indices.[0],
         UniformPName.UniformOffset,
         &offsetValues.[0])

      let uniformBlockIndexWithinShader = 
         ctx.Gl.GetUniformBlockIndex(shader.GlProgramHandle, uniformBlock.Name)

      let mutable uniformBlockSize = 0

      ctx.Gl.GetActiveUniformBlock(
         shader.GlProgramHandle,
         uniformBlockIndexWithinShader,
         UniformBlockPName.UniformBlockDataSize,
         &uniformBlockSize)

      indices

let glVertexAttribDivisor index divisor ctx =
   ctx.Gl.VertexAttribDivisor(index, divisor)
   ctx

let glVertexAttribPointer 
   index 
   size 
   (vapType: VertexAttribPointerType) 
   normalized 
   stride
   ptr
   ctx =
      ctx.Gl.VertexAttribPointer(index, size, vapType, normalized, stride, ptr)
      ctx