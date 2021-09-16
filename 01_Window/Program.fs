open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing

[<EntryPoint>]
let main argv =
   let glOpts = 
      { GlWindowOptions.Default with
         Title = "01_Window"
         Size = new Size (600, 600) }

   let ctx = GlWin.create glOpts

   ctx.Window.add_Render 
      (fun dt -> 
         ctx.Gl.ClearColor Color.DarkKhaki
         ctx.Gl.Clear <| uint32 GLEnum.ColorBufferBit)

   ctx.Window.Run ()
   0