module Gl

open System.Runtime.InteropServices
open System.Runtime.CompilerServices
open System.Drawing
open System.Numerics

#nowarn "9"
#nowarn "51"
open Galante.OpenGL
open Silk.NET.OpenGL
open Microsoft.FSharp.NativeInterop
open System
open Microsoft.Extensions.Logging

let glGetError ctx = ctx.Gl.GetError()

type GlErrorManager =
   static member 
      LogIfError 
      (
         ctx: GlWindowCtx, 
         [<CallerMemberName>]? methodNameMaybe: string
      ) =
      match methodNameMaybe with
      | Some callingMethodName ->
         let rec logNextGlError glError =
            if glError = GLEnum.NoError then
               ()
            else
               ctx.Logger.LogError 
                  $"[{callingMethodName}] '{glError.ToString()}' | "
               logNextGlError (glGetError ctx)

         logNextGlError (glGetError ctx)
      | _ -> 
         invalidOp 
            "The GL error catching function was called but the calling 
            method name was somehow not available at runtime."
      ctx

let glActiveTexture (glTextureSlotId: GLEnum) ctx =
   ctx.Gl.ActiveTexture (glTextureSlotId)
   GlErrorManager.LogIfError ctx

let glBindTexture (textureTarget: TextureTarget) (handle: uint32) ctx =
   ctx.Gl.BindTexture (textureTarget, handle)
   GlErrorManager.LogIfError ctx

let glTexParameterI 
   (target: TextureTarget) 
   (paramName: TextureParameterName) 
   (valueEnum: GLEnum) 
   ctx = 
      let mutable value = LanguagePrimitives.EnumToValue valueEnum
      let valueIntPtr = NativePtr.toNativeInt<int> &&value
      let valueNativePtr: nativeptr<int> = NativePtr.ofNativeInt valueIntPtr

      ctx.Gl.TexParameterI (target, paramName, valueNativePtr)
      GlErrorManager.LogIfError ctx
      
let glTexParameterBorderColor
   ctx 
   (target: TextureTarget) 
   (paramName: TextureParameterName) 
   (color: Vector4) = 
      let mutable value = color
      let valueIntPtr = NativePtr.toNativeInt<Vector4> &&value
      let valueNativePtr: nativeptr<single> = NativePtr.ofNativeInt valueIntPtr

      ctx.Gl.TexParameter (target, paramName, valueNativePtr)

let glTexParameterIcubeMap = glTexParameterI TextureTarget.TextureCubeMap
let glTexParameterI2d = glTexParameterI TextureTarget.Texture2D

// Indentation hell, want to keep code under 80 columns, but this looks so bad
let glTexImage2d 
   (textureTarget: TextureTarget)
   (lvl: int )
   (internalFormat: int)
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
      , internalFormat
      , uint32 w
      , uint32 h
      , border
      , pxFormat
      , pxType
      , pixels)

   GlErrorManager.LogIfError ctx

let glGenBuffer ctx = 
   let handle = ctx.Gl.GenBuffer ()

   GlErrorManager.LogIfError ctx
   |> ignore

   handle

let glGenerateMipmap (textureTarget: TextureTarget) ctx = 
   ctx.Gl.GenerateMipmap textureTarget
   GlErrorManager.LogIfError ctx

let glGenTexture ctx = ctx.Gl.GenTexture ()

let glEnable (flag: EnableCap) ctx = 
   ctx.Gl.Enable (flag)

   GlErrorManager.LogIfError ctx

let glGetUniformBlockIndex shader (uniformBlockName: string) ctx =
   let resultIndex =
      ctx.Gl.GetUniformBlockIndex (shader.GlProgramHandle, uniformBlockName)

   GlErrorManager.LogIfError ctx
   |> ignore

   resultIndex

let glUniformBlockBinding shader uniformIndex targetUniformBlockBinding ctx =
   ctx.Gl.UniformBlockBinding (
      shader.GlProgramHandle, 
      uniformIndex, 
      targetUniformBlockBinding)
   GlErrorManager.LogIfError ctx

let glBindBuffer (target: BufferTargetARB) bufferHandle ctx =
   ctx.Gl.BindBuffer (target, bufferHandle)
   GlErrorManager.LogIfError ctx

let glBufferData 
   (target: BufferTargetARB)
   size
   (dataPtr: voidptr)
   (usageType: BufferUsageARB)
   ctx =
      ctx.Gl.BufferData (target, size, dataPtr, usageType)
      GlErrorManager.LogIfError ctx

let glBufferDataEmpty target size usageType ctx =
   ctx
   |> glBufferData target size (IntPtr.Zero.ToPointer()) usageType
   |> GlErrorManager.LogIfError

let glBindBufferDefault (target: BufferTargetARB) ctx =
   ctx.Gl.BindBuffer (target, 0ul)
   GlErrorManager.LogIfError ctx

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

      GlErrorManager.LogIfError ctx

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
         
      GlErrorManager.LogIfError ctx |> ignore

      let mutable offsetValues: int array = Array.zeroCreate 1

      ctx.Gl.GetActiveUniforms(
         shader.GlProgramHandle,
         1ul,
         &indices.[0],
         UniformPName.UniformOffset,
         &offsetValues.[0])

      GlErrorManager.LogIfError ctx |> ignore

      let uniformBlockIndexWithinShader = 
         ctx.Gl.GetUniformBlockIndex(shader.GlProgramHandle, uniformBlock.Name)
      
      GlErrorManager.LogIfError ctx |> ignore

      let mutable uniformBlockSize = 0

      ctx.Gl.GetActiveUniformBlock(
         shader.GlProgramHandle,
         uniformBlockIndexWithinShader,
         UniformBlockPName.UniformBlockDataSize,
         &uniformBlockSize)
      
      GlErrorManager.LogIfError ctx |> ignore

      indices

let glVertexAttribDivisor index divisor ctx =
   ctx.Gl.VertexAttribDivisor(index, divisor)
   GlErrorManager.LogIfError ctx

let glVertexAttribPointer 
   (index: uint32) 
   (size: int)
   (vapType: VertexAttribPointerType) 
   (normalized: bool)
   strideBytesSize
   offsetByteSizePtr
   ctx =
      ctx.Gl.VertexAttribPointer(
         index, 
         size, 
         vapType, 
         normalized, 
         strideBytesSize, 
         offsetByteSizePtr
      )
      GlErrorManager.LogIfError ctx

let glEnableVertexAttribArray attrIdx ctx = 
   ctx.Gl.EnableVertexAttribArray attrIdx
   GlErrorManager.LogIfError ctx

let glBindVertexArray handle ctx = ctx.Gl.BindVertexArray handle

let glDeleteTexture (texture: GlTexture) ctx =
   ctx.Gl.DeleteTexture texture.GlTexHandle
   ctx

let glFramebufferTexture
   (fbTarget: FramebufferTarget)
   (attachment: FramebufferAttachment)
   (texHandle: uint32)
   (lvl: int) 
   ctx =
   ctx.Gl.FramebufferTexture (
      fbTarget, 
      attachment, 
      texHandle, 
      lvl)
   ctx

let glFramebufferTexture2d 
   (fbTarget: FramebufferTarget)
   (attachment: FramebufferAttachment)
   (texTarget: TextureTarget)
   (texHandle: uint32)
   (lvl: int) 
   ctx =
   ctx.Gl.FramebufferTexture2D (
      fbTarget, 
      attachment, 
      texTarget, 
      texHandle, 
      lvl)
   ctx

let glDrawBuffer (drawBufferMode: DrawBufferMode) ctx =
   ctx.Gl.DrawBuffer drawBufferMode
   ctx

let glReadBuffer (readBufferMode: ReadBufferMode) ctx =
   ctx.Gl.ReadBuffer readBufferMode
   ctx

let glBindFramebuffer (target: FramebufferTarget) handle ctx =
   ctx.Gl.BindFramebuffer (target, handle)
   ctx

let glViewport x y (w: int) (h: int) ctx = 
   ctx.Gl.Viewport (x, y, uint32 w, uint32 h)

let glClear ctx (glEnumValue: GLEnum) =
   ctx.Gl.Clear (uint32 glEnumValue)

let glClearColor ctx x y z a = ctx.Gl.ClearColor (x, y, z, a)

let glDrawArrays ctx (primitiveType: PrimitiveType) first count =
   ctx.Gl.DrawArrays (primitiveType, first, count)

let glDepthFunc ctx (func: DepthFunction) =
   ctx.Gl.DepthFunc func

let glCullFace ctx (mode: CullFaceMode) =
   ctx.Gl.CullFace mode

let glDisable ctx (flag: EnableCap) =
   ctx.Gl.Disable flag