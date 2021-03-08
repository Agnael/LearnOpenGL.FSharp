module GlEbo
#nowarn "9"

open Galante
open Silk.NET.OpenGL
open Microsoft.FSharp.NativeInterop

let create ctx (indices: uint32[]) =
    let squareEbo = ctx.Gl.GenBuffer () 
    use indicesDataIntPtr = fixed indices
    let indicesDataVoidPtr = NativePtr.toVoidPtr indicesDataIntPtr 
    let indicesDataBytesSize = unativeint <| indices.Length * sizeof<uint32>
    ctx.Gl.BindBuffer 
        ( BufferTargetARB.ElementArrayBuffer
        , squareEbo
        )
    ctx.Gl.BufferData 
        ( BufferTargetARB.ElementArrayBuffer
        , indicesDataBytesSize
        , indicesDataVoidPtr
        , BufferUsageARB.StaticDraw
        )