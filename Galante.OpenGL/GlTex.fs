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

    //img.Mutate 
    //    (fun x -> 
    //        x.Rotate(RotateMode.Rotate180) 
    //        |> ignore
    //    )
    img

let create (texTarget, texGlTarget, img: Image<Rgba32>, format, internalFormat: GLEnum, wrapModeS, wrapModeT, filterModeMin, filterModeMag) (ctx: GlWindowCtx) =    
    
    let imgPtr = &&MemoryMarshal.GetReference(img.GetPixelRowSpan(0))
    let imgVoidPtr = NativePtr.toVoidPtr imgPtr

    // TEST START
    // Generate raw bytes
    //let pixelArray = img.GetPixelRowSpan(0).ToArray()
    
    //// Cast to Rgba32 array
    //use imgBytesPtr: Span<Rgba32> = imgBytes
    //let imgVoidPtr = NativePtr.toVoidPtr imgBytesPtr
    // TEST END

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
    let mutable wrapParams = GLEnum.Repeat |> LanguagePrimitives.EnumToValue
    let wrapParamsIntPtr = NativePtr.toNativeInt<int> &&wrapParams
    let wrapParamsNativePtr: nativeptr<int> = NativePtr.ofNativeInt wrapParamsIntPtr
    ctx.Gl.TexParameterI (GLEnum.Texture2D, GLEnum.TextureWrapS, wrapParamsNativePtr)
    ctx.Gl.TexParameterI (GLEnum.Texture2D, GLEnum.TextureWrapT, wrapParamsNativePtr)
       
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