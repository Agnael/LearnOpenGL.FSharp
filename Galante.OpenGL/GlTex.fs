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
open Gl

// Texture builder ============================================================
// Generics allow for the type inference system to alert us when some of the
// builder´s "parameters" was not provided (the compiler will infer the default
// UNIT type of the emptyGlTexture instead of the actually needed type, and
// compilation will stop)
type GlTextureBuilder<'tt,'f,'inF,'wmS,'wmT,'fmMin,'fmMag, 'img> = 
   internal {
      TextureTarget: 'tt
      Format: 'f
      InternalFormat: 'inF
      WrapModeS: 'wmS
      WrapModeT: 'wmT
      FilterModeMin: 'fmMin
      FilterModeMag: 'fmMag
      Image: 'img
   }

let emptyGlTexture = {
   TextureTarget = ()
   Format = ()
   InternalFormat = ()
   WrapModeS = ()
   WrapModeT = ()
   FilterModeMin = ()
   FilterModeMag = ()
   Image = ()
}

let withTextureTarget 
   v
   (b:  GlTextureBuilder<unit,'f,'inF,'wmS,'wmT,'fmMin,'fmMag,'img>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = v
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Image = b.Image
   }

let withFormat
   v
   (b:  GlTextureBuilder<'tt,unit,'inF,'wmS,'wmT,'fmMin,'fmMag,'img>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = v
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Image = b.Image
   }

let withInternalFormat
   v
   (b:  GlTextureBuilder<'tt,'f,unit,'wmS,'wmT,'fmMin,'fmMag,'img>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = v
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Image = b.Image
   }

let withWrapModeS
   v
   (b:  GlTextureBuilder<'tt,'f,'inF,unit,'wmT,'fmMin,'fmMag,'img>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = v
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Image = b.Image
   }

let withWrapModeT
   v
   (b:  GlTextureBuilder<'tt,'f,'inF,'wmS,unit,'fmMin,'fmMag,'img>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = v
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Image = b.Image
   }

let withTextureMinFilter
   v
   (b:  GlTextureBuilder<'tt,'f,'inF,'wmS,'wmT,unit,'fmMag,'img>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = v
      FilterModeMag = b.FilterModeMag
      Image = b.Image
   }

let withTextureMagFilter
   v
   (b:  GlTextureBuilder<'tt,'f,'inF,'wmS,'wmT,'fmMin,unit,'img>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = v
      Image = b.Image
   }

let withImage
   v
   (b:  GlTextureBuilder<'tt,'f,'inF,'wmS,'wmT,'fmMin,'fmMag,unit>)
   : GlTextureBuilder<_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Image = v
   }

let buildGlTexture 
   ctx 
   (b: GlTextureBuilder<
      TextureTarget,
      PixelFormat,
      PixelFormat,
      GLEnum,
      GLEnum,
      GLEnum,
      GLEnum,
      Image<Rgba32>
   >): GlTexture = 
   let imgPtr = &&MemoryMarshal.GetReference(b.Image.GetPixelRowSpan(0))
   let imgVoidPtr = NativePtr.toVoidPtr imgPtr

   let texture = {
      GlTexHandle = ctx.Gl.GenTexture ()
      TextureTarget = b.TextureTarget
      Width = b.Image.Width
      Height = b.Image.Height
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      TextureFilterModeMin = b.FilterModeMin
      TextureFilterModeMag = b.FilterModeMag
   }
   
   ctx
   |> glBindTexture b.TextureTarget texture.GlTexHandle 
   |> glTexParameterI2d TextureParameterName.TextureWrapS b.WrapModeS
   |> glTexParameterI2d TextureParameterName.TextureWrapT b.WrapModeT
   |> glTexParameterI2d TextureParameterName.TextureMinFilter b.FilterModeMin
   |> glTexParameterI2d TextureParameterName.TextureMagFilter b.FilterModeMag
   |> glTexImage2d
         texture.TextureTarget
         0
         texture.InternalFormat
         texture.Width
         texture.Height
         0
         texture.Format
         PixelType.UnsignedByte
         imgVoidPtr
   |> glGenerateMipmap texture.TextureTarget
   |> glEnable EnableCap.Texture2D
   |> ignore
   texture

// ----------------------------------------------------------------------------

let buildCubemapGlTexture (cubemapImg: CubeMapImage) ctx: GlTexture =
   let olis = "olixx"

   let rec same = function
   | x::y::_ when x <> y -> false
   | _::xs -> same xs
   | [] -> true

   if not (same [
      cubemapImg.Back.Width
      cubemapImg.Bottom.Width
      cubemapImg.Front.Width
      cubemapImg.Left.Width
      cubemapImg.Right.Width
      cubemapImg.Top.Width
   ]) then failwith "All images in a cubemap must have equal widths"

   if not (same [
      cubemapImg.Back.Height
      cubemapImg.Bottom.Height
      cubemapImg.Front.Height
      cubemapImg.Left.Height
      cubemapImg.Right.Height
      cubemapImg.Top.Height
   ]) then failwith "All images in a cubemap must have equal heights"

   let width = cubemapImg.Back.Width
   let height = cubemapImg.Back.Height

   // A loot of repetitive, non looped, code ahead.
   // The execution will break when it enters this specific function if all of
   // this is done in a helper function (some pointer problems when fsharp 
   // doesn´t like the scope in which they are used).
   let imgBackIntPtr = 
      &&MemoryMarshal.GetReference(cubemapImg.Back.GetPixelRowSpan(0))
   let imgBackPtr = NativePtr.toVoidPtr imgBackIntPtr

   let imgBottomIntPtr = 
      &&MemoryMarshal.GetReference(cubemapImg.Bottom.GetPixelRowSpan(0))
   let imgBottomPtr = NativePtr.toVoidPtr imgBottomIntPtr
   
   let imgFrontIntPtr = 
      &&MemoryMarshal.GetReference(cubemapImg.Front.GetPixelRowSpan(0))
   let imgFrontPtr = NativePtr.toVoidPtr imgFrontIntPtr
   
   let imgLeftIntPtr = 
      &&MemoryMarshal.GetReference(cubemapImg.Left.GetPixelRowSpan(0))
   let imgLeftPtr = NativePtr.toVoidPtr imgLeftIntPtr
   
   let imgRightIntPtr = 
      &&MemoryMarshal.GetReference(cubemapImg.Right.GetPixelRowSpan(0))
   let imgRightPtr = NativePtr.toVoidPtr imgRightIntPtr
   
   let imgTopIntPtr = 
      &&MemoryMarshal.GetReference(cubemapImg.Top.GetPixelRowSpan(0))
   let imgTopPtr = NativePtr.toVoidPtr imgTopIntPtr

   let texture = {
      GlTexHandle = ctx.Gl.GenTexture ()
      TextureTarget = TextureTarget.TextureCubeMap
      Width = width
      Height = height
      Format = PixelFormat.Rgba
      InternalFormat = PixelFormat.Rgb
      WrapModeS = GLEnum.ClampToEdge
      WrapModeT = GLEnum.ClampToEdge
      TextureFilterModeMin = GLEnum.Linear
      TextureFilterModeMag = GLEnum.Linear
   }

   // Texture target	-> Orientation
   ctx
   |> glBindTexture texture.TextureTarget texture.GlTexHandle 
   // GL_TEXTURE_CUBE_MAP_POSITIVE_X -> Right
   |> glTexImage2d
         TextureTarget.TextureCubeMapPositiveX
         0
         texture.InternalFormat
         width
         height
         0
         texture.Format
         PixelType.UnsignedByte
         imgRightPtr
   // GL_TEXTURE_CUBE_MAP_NEGATIVE_X -> Left
   |> glTexImage2d
         TextureTarget.TextureCubeMapNegativeX
         0
         texture.InternalFormat
         width
         height
         0
         texture.Format
         PixelType.UnsignedByte
         imgLeftPtr
   // GL_TEXTURE_CUBE_MAP_POSITIVE_Y -> Top
   |> glTexImage2d
         TextureTarget.TextureCubeMapPositiveY
         0
         texture.InternalFormat
         width
         height
         0
         texture.Format
         PixelType.UnsignedByte
         imgTopPtr
   // GL_TEXTURE_CUBE_MAP_NEGATIVE_Y -> Bottom
   |> glTexImage2d
         TextureTarget.TextureCubeMapNegativeY
         0
         texture.InternalFormat
         width
         height
         0
         texture.Format
         PixelType.UnsignedByte
         imgBottomPtr
   // GL_TEXTURE_CUBE_MAP_POSITIVE_Z -> Back
   |> glTexImage2d
         TextureTarget.TextureCubeMapPositiveZ
         0
         texture.InternalFormat
         width
         height
         0
         texture.Format
         PixelType.UnsignedByte
         imgBackPtr
   // GL_TEXTURE_CUBE_MAP_NEGATIVE_Z -> Front
   |> glTexImage2d
         TextureTarget.TextureCubeMapNegativeZ
         0
         texture.InternalFormat
         width
         height
         0
         texture.Format
         PixelType.UnsignedByte
         imgFrontPtr
   |> glTexParameterIcubeMap TextureParameterName.TextureWrapS texture.WrapModeS
   |> glTexParameterIcubeMap TextureParameterName.TextureWrapT texture.WrapModeT
   |> glTexParameterIcubeMap TextureParameterName.TextureWrapR GLEnum.ClampToEdge
   |> glTexParameterIcubeMap 
         TextureParameterName.TextureMinFilter 
         texture.TextureFilterModeMag
   |> glTexParameterIcubeMap 
         TextureParameterName.TextureMagFilter 
         texture.TextureFilterModeMag
   |> ignore
   texture

// ----------------------------------------------------------------------------

let setActive (glTextureSlotId: GLEnum) texture (vao, ctx) =    
   GlVao.bind (vao, ctx) 
   |> snd
   |> glActiveTexture glTextureSlotId
   |> glBindTexture TextureTarget.Texture2D texture.GlTexHandle
   |> fun ctx -> (vao, ctx)

let setActiveEmptyTexture (slot: GLEnum) (tex: GlEmptyTexture) (vao, ctx) =    
   GlVao.bind (vao, ctx)
   |> snd
   |> glActiveTexture slot
   |> glBindTexture TextureTarget.Texture2D tex.GlTexHandle
   |> fun ctx -> (vao, ctx)

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

let loadCubemap dirPath fileExtension (ctx: GlWindowCtx) = 
   let getFullPath cubemapPartName =
      Path.Combine (dirPath, $"{cubemapPartName}{fileExtension}")

   {
      Back = loadImage (getFullPath "back") ctx
      Front = loadImage (getFullPath "front") ctx
      Right = loadImage (getFullPath "right") ctx
      Left = loadImage (getFullPath "left") ctx
      Top = loadImage (getFullPath "top") ctx
      Bottom = loadImage (getFullPath "bottom") ctx
   }

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
      
let texCreateDefault textureTarget image ctx =
   emptyGlTexture
   |> withTextureTarget textureTarget
   |> withFormat PixelFormat.Rgba
   |> withInternalFormat PixelFormat.Rgb
   |> withWrapModeS GLEnum.Repeat
   |> withWrapModeT GLEnum.Repeat
   |> withTextureMinFilter GLEnum.LinearMipmapLinear
   |> withTextureMagFilter GLEnum.Filter
   |> withImage image
   |> buildGlTexture ctx

let create2d img ctx = 
   texCreateDefault TextureTarget.Texture2D img ctx

let create2dTransparent image ctx =
   emptyGlTexture
   |> withTextureTarget TextureTarget.Texture2D
   |> withFormat PixelFormat.Rgba
   |> withInternalFormat PixelFormat.Rgba
   |> withWrapModeS GLEnum.ClampToEdge
   |> withWrapModeT GLEnum.ClampToEdge
   |> withTextureMinFilter GLEnum.LinearMipmapLinear
   |> withTextureMagFilter GLEnum.Filter
   |> withImage image
   |> buildGlTexture ctx

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


   let mutable linearEnumAsValue = 
      LanguagePrimitives.EnumToValue GLEnum.Linear

   let linearEnumAsValuePtr: nativeint = 
      NativePtr.toNativeInt &&linearEnumAsValue

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
   
//let createCubeMap (imgs: CubeMapImage) (ctx: GlWindowCtx) =    
//   // Fixed configs
//   let mainTextureTarget = TextureTarget.TextureCubeMap
//   let format = GLEnum.Rgba
//   let wrapModeS = GLEnum.ClampToEdge
//   let wrapModeT = GLEnum.ClampToEdge
//   let wrapModeR = GLEnum.ClampToEdge
//   let filterModeMin = GLEnum.Linear
//   let filterModeMag = GLEnum.Linear

//   let getPixelRowSpan (img: Image<Rgba32>) = img.GetPixelRowSpan(0)

//   let imgPtr = &&MemoryMarshal.GetReference(getPixelRowSpan imgs.Front)
//   let imgVoidPtr = NativePtr.toVoidPtr imgPtr

//   let  mainTexture =
//      { GlTexture.GlTexHandle = ctx.Gl.GenTexture ()
//      ; TextureTarget = mainTextureTarget
//      ; Width = 
//         imgs.Left.Width +
//         imgs.Front.Width +
//         imgs.Right.Width +
//         imgs.Back.Width
//      ; Height = 
//         imgs.Top.Width +
//         imgs.Front.Width +
//         imgs.Bottom.Width
//      ; Format = format
//      ; InternalFormat = LanguagePrimitives.EnumToValue format
//      ; WrapModeS = wrapModeS
//      ; WrapModeT = wrapModeT
//      ; TextureFilterModeMin = filterModeMin
//      ; TextureFilterModeMag = filterModeMag
//      ;}
    
//   ctx.Gl.BindTexture (mainTextureTarget,  mainTexture.GlTexHandle)



//   // Wrap mode for S
//   let mutable textureWrapS = LanguagePrimitives.EnumToValue wrapModeS
//   let textureWrapSIntPtr = NativePtr.toNativeInt<int> &&textureWrapS

//   let textureWrapSNativePtr: nativeptr<int> = 
//      NativePtr.ofNativeInt textureWrapSIntPtr

//   ctx.Gl.TexParameterI (
//      GLEnum.Texture2D, 
//      GLEnum.TextureWrapS, 
//      textureWrapSNativePtr)
    
//   // Wrap mode for T
//   let mutable textureWrapT = wrapModeT |> LanguagePrimitives.EnumToValue
//   let textureWrapTIntPtr = NativePtr.toNativeInt<int> &&textureWrapT

//   let textureWrapTNativePtr: nativeptr<int> = 
//      NativePtr.ofNativeInt textureWrapTIntPtr

//   ctx.Gl.TexParameterI (
//      GLEnum.Texture2D, 
//      GLEnum.TextureWrapT, 
//      textureWrapTNativePtr)
       
//   // Fixed R wrapping mode: Texture coordinates that are exactly 
//   // between two faces may not hit an exact face (due to some hardware
//   // limitations) so by using GL_CLAMP_TO_EDGE OpenGL always returns 
//   // their edge values whenever we sample between faces.
//   let mutable textureWrapR = 
//      LanguagePrimitives.EnumToValue GLEnum.ClampToEdge

//   let textureWrapRIntPtr = 
//      NativePtr.toNativeInt<int> &&textureWrapR

//   let textureWrapRNativePtr: nativeptr<int> = 
//      NativePtr.ofNativeInt textureWrapRIntPtr

//   ctx.Gl.TexParameterI (
//      GLEnum.Texture2D, 
//      GLEnum.TextureWrapR, 
//      textureWrapRNativePtr)

//   // Texture filtering modes
//   let mutable textureFilterParams = 
//      LanguagePrimitives.EnumToValue GLEnum.Linear

//   let textureFilterParamsIntPtr = 
//      NativePtr.toNativeInt<int> &&textureFilterParams

//   let textureFilterNativePtr: nativeptr<int> = 
//      NativePtr.ofNativeInt textureFilterParamsIntPtr

//   ctx.Gl.TexParameterI (
//      GLEnum.Texture2D, 
//      GLEnum.TextureMinFilter, 
//      textureFilterNativePtr)

//   ctx.Gl.TexParameterI (
//      GLEnum.Texture2D, 
//      GLEnum.TextureMagFilter, 
//      textureFilterNativePtr)
       
//   ctx.Gl.TexImage2D 
//      (  mainTexture.TextureTargetGl
//      , 0
//      ,  mainTexture.InternalFormat
//      , uint32  mainTexture.Width
//      , uint32  mainTexture.Height
//      , 0
//      ,  mainTexture.Format
//      , GLEnum.UnsignedByte
//      , imgVoidPtr)

//   let glError = ctx.Gl.GetError()
//   ctx.Gl.GenerateMipmap  mainTexture.TextureTarget
//   let isEnabled = ctx.Gl.IsEnabled (GLEnum.Texture2D)
//   ctx.Gl.Enable (GLEnum.Texture2D)
//    mainTexture