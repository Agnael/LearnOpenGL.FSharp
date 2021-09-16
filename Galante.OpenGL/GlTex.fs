module GlTex

open SixLabors.ImageSharp.Formats
open SixLabors.ImageSharp.Processing

#nowarn "9"
#nowarn "51"
open Silk.NET.OpenGL
open System
open SixLabors.ImageSharp.PixelFormats
open System.Runtime.InteropServices
open Galante.OpenGL
open SixLabors.ImageSharp
open System.IO
open Microsoft.FSharp.NativeInterop
open Microsoft.Extensions.Logging

   let setActive (glTextureSlotId: GLEnum) texture (vao, ctx) =    
      GlVao.bind (vao, ctx) |> ignore
      ctx.Gl.ActiveTexture glTextureSlotId
      ctx.Gl.BindTexture (GLEnum.Texture2D, texture.GlTexHandle)
      (vao, ctx)

   let setActiveEmptyTexture (slot: GLEnum) (tex: GlEmptyTexture) (vao, ctx) =    
      GlVao.bind (vao, ctx) |> ignore
      ctx.Gl.ActiveTexture slot
      ctx.Gl.BindTexture (GLEnum.Texture2D, tex.GlTexHandle)
      (vao, ctx)

   let loadImage filePath (ctx: GlWindowCtx) =
      let (img: Image<Rgba32>, imgSharpFormat) =
         ctx.FileProvider.GetFileInfo(filePath)
         |> fun fileInfo ->
            let format = Unchecked.defaultof<_>
            let image: Image<Rgba32> =
                  if fileInfo = null
                  then invalidOp "El archivo no existe"
                  else Image.Load (fileInfo.PhysicalPath, ref format)
            (image, format)
      img

   /// <summary>
   /// Loads and flips the image vertically.
   /// </summary>
   let loadImageF filePath (ctx: GlWindowCtx) =
      let (img: Image<Rgba32>, imgSharpFormat) =
         ctx.FileProvider.GetFileInfo(filePath)
         |> fun fileInfo ->
            let format = Unchecked.defaultof<_>
            let image: Image<Rgba32> =
                  if fileInfo = null
                  then invalidOp "El archivo no existe"
                  else Image.Load (fileInfo.PhysicalPath, ref format)
            (image, format)

      img.Mutate(
         new Action<IImageProcessingContext>(
            fun x -> x.Flip(FlipMode.Vertical) |> ignore
         )
      )

      img

   let create 
      ( texTarget
      , texGlTarget
      , img: Image<Rgba32>
      , format
      , internalFormat: GLEnum
      , wrapModeS
      , wrapModeT
      , filterModeMin
      , filterModeMag
      ) 
      (ctx: GlWindowCtx) =    
    
      let imgPtr = &&MemoryMarshal.GetReference(img.GetPixelRowSpan(0))
      let imgVoidPtr = NativePtr.toVoidPtr imgPtr

      let texture =
         { GlTexture.GlTexHandle = ctx.Gl.GenTexture ()
         //; FilePath = image
         ; TextureTarget = texTarget
         ; TextureTargetGl = texGlTarget
         ; Width = img.Width
         ; Height = img.Height
         ; Format = format
         ; InternalFormat = LanguagePrimitives.EnumToValue internalFormat
         ; WrapModeS = wrapModeS
         ; WrapModeT = wrapModeT
         ; TextureFilterModeMin = filterModeMin
         ; TextureFilterModeMag = filterModeMag
         ;}
    
      ctx.Gl.BindTexture (GLEnum.Texture2D, texture.GlTexHandle)

      // 1 the texture wrapping/filtering options (on the currently bound texture object)
      let mutable textureWrapS = wrapModeS |> LanguagePrimitives.EnumToValue
      let textureWrapSIntPtr = NativePtr.toNativeInt<int> &&textureWrapS
      let textureWrapSNativePtr: nativeptr<int> = NativePtr.ofNativeInt textureWrapSIntPtr
      ctx.Gl.TexParameterI (GLEnum.Texture2D, GLEnum.TextureWrapS, textureWrapSNativePtr)
    
      let mutable textureWrapT = wrapModeS |> LanguagePrimitives.EnumToValue
      let textureWrapTIntPtr = NativePtr.toNativeInt<int> &&textureWrapT
      let textureWrapTNativePtr: nativeptr<int> = NativePtr.ofNativeInt textureWrapTIntPtr
      ctx.Gl.TexParameterI (GLEnum.Texture2D, GLEnum.TextureWrapT, textureWrapTNativePtr)
       
      let mutable textureFilterParams = GLEnum.Linear |> LanguagePrimitives.EnumToValue
      let textureFilterParamsIntPtr = NativePtr.toNativeInt<int> &&textureFilterParams
      let textureFilterNativePtr: nativeptr<int> = NativePtr.ofNativeInt textureFilterParamsIntPtr
      ctx.Gl.TexParameterI (GLEnum.Texture2D, GLEnum.TextureMinFilter, textureFilterNativePtr)
      ctx.Gl.TexParameterI (GLEnum.Texture2D, GLEnum.TextureMagFilter, textureFilterNativePtr)
       
      ctx.Gl.TexImage2D 
         ( texture.TextureTargetGl
         , 0
         , texture.InternalFormat
         , uint32 texture.Width
         , uint32 texture.Height
         , 0
         , texture.Format
         , GLEnum.UnsignedByte
         , imgVoidPtr)

      let glError = ctx.Gl.GetError()
      ctx.Gl.GenerateMipmap texture.TextureTarget
      let isEnabled = ctx.Gl.IsEnabled (GLEnum.Texture2D)
      ctx.Gl.Enable (GLEnum.Texture2D)
      texture


   let create2D image ctx =
      ctx
      |> create 
         ( TextureTarget.Texture2D
         , GLEnum.Texture2D
         , image
         , GLEnum.Rgba
         , GLEnum.Rgba
         , GLEnum.Repeat
         , GLEnum.Repeat
         // Changed when troubleshooting texture bug in model loading
         //, GLEnum.Linear
         , GLEnum.LinearMipmapLinear
         , GLEnum.Linear
         )
        
   let create2Dtransparent image ctx =
      ctx
      |> create 
         ( TextureTarget.Texture2D
         , GLEnum.Texture2D
         , image
         , GLEnum.Rgba
         , GLEnum.Rgba
         , GLEnum.ClampToEdge
         , GLEnum.ClampToEdge
         // Changed when troubleshooting texture bug in model loading
         //, GLEnum.Linear
         , GLEnum.LinearMipmapLinear
         , GLEnum.Linear
         )

   let texCreateEmpty2d width height ctx =
      let texture =
            { GlEmptyTexture.GlTexHandle = ctx.Gl.GenTexture ()
            ; Width = width
            ; Height = height
            ;}
   
      ctx.Gl.BindTexture (GLEnum.Texture2D, texture.GlTexHandle)

      ctx.Gl.TexImage2D (
         TextureTarget.Texture2D,
         0,
         LanguagePrimitives.EnumToValue GLEnum.Rgb,
         uint32 width,
         uint32 height,
         0,
         GLEnum.Rgb,
         GLEnum.UnsignedByte,
         IntPtr.Zero.ToPointer())


      let mutable linearEnumAsValue = LanguagePrimitives.EnumToValue GLEnum.Linear
      let linearEnumAsValuePtr: nativeint = NativePtr.toNativeInt &&linearEnumAsValue
      let linearEnumAsValueNativePtr: nativeptr<int> =
         NativePtr.ofNativeInt linearEnumAsValuePtr

      ctx.Gl.TexParameterI (
         TextureTarget.Texture2D, 
         TextureParameterName.TextureMinFilter,
         linearEnumAsValueNativePtr)

      ctx.Gl.TexParameterI (
         TextureTarget.Texture2D, 
         TextureParameterName.TextureMagFilter,
         linearEnumAsValueNativePtr)

      texture