// Learn more about F# at http://fsharp.org

open System
open Sodium.Frp
open Galante
open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Extensions.FileProviders
open System.IO
open System.Drawing
open Silk.NET.Windowing
open Silk.NET.OpenGL

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "03_Quad"
            Size = new Size (600, 300) }

    let (window, gl) = GlWin.create glOpts

    let ctx =  
        { GlContext.Gl = gl
        ; Logger = new NullLogger<obj>()
        ; FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory())
        ;}

    let mutable quadVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>

    let onLoad () = 
        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "Basic"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Basic.vert"
                ; ShaderType.FragmentShader, @"Basic.frag" 
                ;]
            |> GlProg.withUniforms ["myColor"]
            |> GlProg.build ctx

        quadVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let quadVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"]
            |> GlVbo.withAttrDefinitions [
                [[-0.5f; 0.5f; 0.0f]]
                [[0.5f; 0.5f; 0.0f]]
                [[-0.5f; -0.5f; 0.0f]]
                [[0.5f; -0.5f; 0.0f]]
            ]
            |> GlVbo.build (quadVao, ctx)

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onRender dt =
        gl.Clear <| uint32 GLEnum.ColorBufferBit
        
        let greenValue = single <| Math.Sin(window.Time);

        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformV4 "myColor" (0.8f, greenValue, 0.0f, 1.0f)
        |> ignore

        gl.BindVertexArray quadVao.GlVaoHandle
        gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
        ()

    window.add_Load (new Action(onLoad))
    window.add_Render (new Action<float>(onRender))
    window.Run ()
    0 // return an integer exit code
