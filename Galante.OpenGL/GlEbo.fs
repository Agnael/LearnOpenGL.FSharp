module GlEbo
#nowarn "9"

open Galante.OpenGL
open Silk.NET.OpenGL
open Microsoft.FSharp.NativeInterop

let create ctx (indices: uint32[]) =
    let ebo = ctx.Gl.GenBuffer () 
    use indicesDataIntPtr = fixed indices
    let indicesDataVoidPtr = NativePtr.toVoidPtr indicesDataIntPtr 
    let indicesDataBytesSize = unativeint <| indices.Length * sizeof<uint32>
    ctx.Gl.BindBuffer 
        ( BufferTargetARB.ElementArrayBuffer
        , ebo
        )
    ctx.Gl.BufferData 
        ( BufferTargetARB.ElementArrayBuffer
        , indicesDataBytesSize
        , indicesDataVoidPtr
        , BufferUsageARB.StaticDraw
        )

    { GlElementBufferObject.GlEboHandle = ebo
    //; GlElementBufferObject.indices = indices
    ;}