module GlTex

open SixLabors.ImageSharp.Formats
open SixLabors.ImageSharp.Processing

#nowarn "9"
#nowarn "51"
open Silk.NET.OpenGL
open System
open SixLabors.ImageSharp.PixelFormats
open System.Runtime.InteropServices
open Galante
open SixLabors.ImageSharp
open System.IO
open Microsoft.FSharp.NativeInterop
open Microsoft.Extensions.Logging

let bind (texture: GlTexture) (vao, ctx) =
    GlVao.bind (vao, ctx) |> ignore
    ctx.Gl.BindTexture (GLEnum.Texture2D, texture.GlTexHandle)
    (vao, ctx)

let create (texTarget, texGlTarget, filePath, format, internalFormat: GLEnum, wrapModeS, wrapModeT, filterModeMin, filterModeMag) (vao, ctx) =
    GlVao.bind (vao, ctx) |> ignore
    
    let (img: Image<Rgba32>, imgSharpFormat) =
        ctx.FileProvider.GetFileInfo(filePath)
        |> fun fileInfo ->
            let format = Unchecked.defaultof<_>
            let image: Image<Rgba32> =
                if fileInfo = null
                then invalidOp "El archivo no existe"
                else Image.Load (fileInfo.PhysicalPath, ref format)
            (image, format)

    img.Mutate (fun x -> 
        x.Rotate(RotateMode.Rotate180) 
        |> ignore)

    let imgBytes =
        use memStream = new MemoryStream ()
        img.SaveAsJpegAsync memStream
        |> Async.AwaitTask
        |> Async.RunSynchronously
        memStream.ToArray()
    
    let imgPtr = &&MemoryMarshal.GetReference(img.GetPixelRowSpan(0))
    let imgVoidPtr = NativePtr.toVoidPtr imgPtr

    let tex =
        { GlTexture.GlTexHandle = ctx.Gl.GenTexture ()
        ; FilePath = filePath
        ; TextureTarget = texTarget
        ; TextureTargetGl = texGlTarget
        ; Width = img.Width
        ; Height = img.Height
        ; DataVoidPtr = imgVoidPtr
        ; Format = format
        ; InternalFormat = LanguagePrimitives.EnumToValue internalFormat
        ; WrapModeS = wrapModeS
        ; WrapModeT = wrapModeT
        ; TextureFilterModeMin = filterModeMin
        ; TextureFilterModeMag = filterModeMag
        ;}

    ctx.Gl.BindTexture (GLEnum.Texture2D, tex.GlTexHandle)

    // set the texture wrapping/filtering options (on the currently bound texture object)
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
        ( tex.TextureTargetGl
        , 0
        , tex.InternalFormat
        , uint32 tex.Width
        , uint32 tex.Height
        , 0
        , tex.Format
        , GLEnum.UnsignedByte
        , tex.DataVoidPtr)

    let glError = ctx.Gl.GetError()
    ctx.Gl.GenerateMipmap tex.TextureTarget
    let isEnabled = ctx.Gl.IsEnabled (GLEnum.Texture2D)
    ctx.Gl.Enable (GLEnum.Texture2D)
    ctx.Logger.LogInformation <| "olaaaaa"
    tex

let create2D filePath (vao, ctx) =
    (vao, ctx)
    |> create 
        ( TextureTarget.Texture2D
        , GLEnum.Texture2D
        , filePath
        , GLEnum.Rgba
        , GLEnum.Rgba
        , GLEnum.Repeat
        , GLEnum.Repeat
        , GLEnum.Linear
        , GLEnum.Linear
        )