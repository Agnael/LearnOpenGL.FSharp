// Learn more about F# at http://fsharp.org

open System
open Galante
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open Silk.NET.Maths

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "RawSilk"
            Size = new Size (600, 300) }

    let (window, gl) = GlWin.create glOpts
    window.add_Render 
        (fun dt -> 
            gl.ClearColor Color.DarkKhaki
            gl.Clear <| uint32 GLEnum.ColorBufferBit)

    window.Run ()
    0