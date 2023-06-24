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
   (v: int)
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
      int,
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



// Texture builder ============================================================
// Version of the already existing version of the GlTextureBuilder, but that
// does not require an image to be provided. I know it´s gross, stop staring at
// me.
type GlImagelessTextureBuilder<'tt,'f,'inF,'wmS,'wmT,'fmMin,'fmMag, 'w, 'h> = 
   internal {
      TextureTarget: 'tt
      Format: 'f
      InternalFormat: 'inF
      WrapModeS: 'wmS
      WrapModeT: 'wmT
      FilterModeMin: 'fmMin
      FilterModeMag: 'fmMag
      Width: 'w
      Height: 'h
   }

let emptyImagelessGlTexture = {
   TextureTarget = ()
   Format = ()
   InternalFormat = ()
   WrapModeS = ()
   WrapModeT = ()
   FilterModeMin = ()
   FilterModeMag = ()
   Width = ()
   Height = ()
}

let imagelessWithTextureTarget 
   v
   (b:  GlImagelessTextureBuilder<unit,'f,'inF,'wmS,'wmT,'fmMin,'fmMag,'w,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = v
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Width = b.Width
      Height = b.Height
   }

let imagelessWithFormat
   v
   (b:  GlImagelessTextureBuilder<'tt,unit,'inF,'wmS,'wmT,'fmMin,'fmMag,'w,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = v
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Width = b.Width
      Height = b.Height
   }

let imagelessWithInternalFormat
   (v: int)
   (b:  GlImagelessTextureBuilder<'tt,'f,unit,'wmS,'wmT,'fmMin,'fmMag,'w,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = v
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Width = b.Width
      Height = b.Height
   }

let imagelessWithWrapModeS
   v
   (b:  GlImagelessTextureBuilder<'tt,'f,'inF,unit,'wmT,'fmMin,'fmMag,'w,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = v
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Width = b.Width
      Height = b.Height
   }

let imagelessWithWrapModeT
   v
   (b:  GlImagelessTextureBuilder<'tt,'f,'inF,'wmS,unit,'fmMin,'fmMag,'w,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = v
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Width = b.Width
      Height = b.Height
   }

let imagelessWithTextureMinFilter
   v
   (b:  GlImagelessTextureBuilder<'tt,'f,'inF,'wmS,'wmT,unit,'fmMag,'w,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = v
      FilterModeMag = b.FilterModeMag
      Width = b.Width
      Height = b.Height
   }

let imagelessWithTextureMagFilter
   v
   (b:  GlImagelessTextureBuilder<'tt,'f,'inF,'wmS,'wmT,'fmMin,unit,'w,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = v
      Width = b.Width
      Height = b.Height
   }

let imagelessWithWidth
   v
   (b:  GlImagelessTextureBuilder<'tt,'f,'inF,'wmS,'wmT,'fmMin,'fmMag,unit,'h>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Width = v
      Height = b.Height
   }

let imagelessWithHeight
   v
   (b:  GlImagelessTextureBuilder<'tt,'f,'inF,'wmS,'wmT,'fmMin,'fmMag,'w,unit>)
   : GlImagelessTextureBuilder<_,_,_,_,_,_,_,_,_> = {
      TextureTarget = b.TextureTarget
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      FilterModeMin = b.FilterModeMin
      FilterModeMag = b.FilterModeMag
      Width = b.Width
      Height = v
   }

let buildImagelessGlTexture 
   ctx 
   (b: GlImagelessTextureBuilder<
      TextureTarget,
      PixelFormat,
      int,
      GLEnum,
      GLEnum,
      GLEnum,
      GLEnum,
      int,
      int
   >): GlTexture = 
   let texture = {
      GlTexHandle = ctx.Gl.GenTexture ()
      TextureTarget = b.TextureTarget
      Width = b.Width
      Height = b.Height
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
         // Note this pixel type. This function is not ONLY creating an 
         // imageless texture, so this API design sucks and should be reworked.
         PixelType.Float
         (IntPtr.Zero.ToPointer())
   |> glGenerateMipmap texture.TextureTarget
   |> glEnable EnableCap.Texture2D
   |> ignore
   texture

// Dumb function copypasted from "buildImagelessGlTexture"
// It only exists so that creating a cubemap depth texture is not that difficult, but
// it's horrifying from an API standpoint.
// Note that the TextureFormat parameter is ignored.
let buildImagelessGlTextureCubemap
   ctx 
   (b: GlImagelessTextureBuilder<
      TextureTarget,
      PixelFormat,
      int,
      GLEnum,
      GLEnum,
      GLEnum,
      GLEnum,
      int,
      int
   >): GlTexture = 
   let texture = {
      GlTexHandle = ctx.Gl.GenTexture ()
      TextureTarget = b.TextureTarget
      Width = b.Width
      Height = b.Height
      Format = b.Format
      InternalFormat = b.InternalFormat
      WrapModeS = b.WrapModeS
      WrapModeT = b.WrapModeT
      TextureFilterModeMin = b.FilterModeMin
      TextureFilterModeMag = b.FilterModeMag
   }
   
   ctx
   |> glBindTexture TextureTarget.TextureCubeMap texture.GlTexHandle 
   |> glTexParameterIcubeMap TextureParameterName.TextureWrapS b.WrapModeS
   |> glTexParameterIcubeMap TextureParameterName.TextureWrapT b.WrapModeT
   // Note that this specific wrap config is donw for cubemaps only
   |> glTexParameterIcubeMap TextureParameterName.TextureWrapR GLEnum.ClampToEdge
   |> glTexParameterIcubeMap TextureParameterName.TextureMinFilter b.FilterModeMin
   |> glTexParameterIcubeMap TextureParameterName.TextureMagFilter b.FilterModeMag
   |> fun ctx ->
      let generateTextureImage targetOffset =
         glTexImage2d
            (enum<TextureTarget> ((int TextureTarget.TextureCubeMapPositiveX) + targetOffset))
            0
            texture.InternalFormat
            texture.Width
            texture.Height
            0
            texture.Format
            // Note this pixel type. This function is not ONLY creating an 
            // imageless texture, so this API design sucks and should be reworked.
            PixelType.Float
            (IntPtr.Zero.ToPointer())
            ctx

      // Executes it once per face
      [0..5]
      |> List.map (fun i -> generateTextureImage i)
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
      InternalFormat = LanguagePrimitives.EnumToValue PixelFormat.Rgb
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
   
let setActiveCubemap (glTextureSlotId: GLEnum) texture (vao, ctx) =    
   GlVao.bind (vao, ctx) 
   |> snd
   |> glActiveTexture glTextureSlotId
   |> glBindTexture TextureTarget.TextureCubeMap texture.GlTexHandle
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
      
let texCreateDefault textureTarget internalFormat image ctx =
   emptyGlTexture
   |> withTextureTarget textureTarget
   |> withFormat PixelFormat.Rgba
   |> withInternalFormat internalFormat
   |> withWrapModeS GLEnum.Repeat
   |> withWrapModeT GLEnum.Repeat
   |> withTextureMinFilter GLEnum.LinearMipmapLinear
   |> withTextureMagFilter GLEnum.Filter
   |> withImage image
   |> buildGlTexture ctx

let create2d img ctx = 
   texCreateDefault 
      TextureTarget.Texture2D 
      (LanguagePrimitives.EnumToValue PixelFormat.Rgb)
      img 
      ctx
   
let create2dSrgb img ctx = 
   texCreateDefault 
      TextureTarget.Texture2D 
      (LanguagePrimitives.EnumToValue GLEnum.Srgb)
      img 
      ctx

let create2dTransparent image ctx =
   emptyGlTexture
   |> withTextureTarget TextureTarget.Texture2D
   |> withFormat PixelFormat.Rgba
   |> withInternalFormat (LanguagePrimitives.EnumToValue PixelFormat.Rgba)
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