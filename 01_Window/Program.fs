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
            Title = "01_Window"
            Size = new Size (600, 600) }

    let (window, gl) = GlWin.create glOpts
    window.add_Render 
        (fun dt -> 
            gl.ClearColor Color.DarkKhaki
            gl.Clear <| uint32 GLEnum.ColorBufferBit)

    window.Run ()
    0