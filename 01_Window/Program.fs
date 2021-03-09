open Galante
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "01_Window"
            Size = new Size (600, 600) }

    let (window, ctx) = GlWin.create glOpts
    window.add_Render 
        (fun dt -> 
            ctx.Gl.ClearColor Color.DarkKhaki
            ctx.Gl.Clear <| uint32 GLEnum.ColorBufferBit)

    window.Run ()
    0