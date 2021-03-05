open System
open Silk.NET.Windowing
open Silk.NET.OpenGL
open Galante
open System.Drawing
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Logging.Abstractions
open System.IO

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "02_Triangle"
            Size = new Size (600, 600) }

    let (window, gl) = GlWin.create glOpts

    let ctx =  
        { GlContext.Gl = gl
        ; Logger = new NullLogger<obj>()
        ; FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory())
        ;}

    let mutable shaderBasic = Unchecked.defaultof<_>
    let mutable triangleVao = Unchecked.defaultof<_>

    let load () =
        shaderBasic <-
            GlProg.emptyBuilder
            |> GlProg.withName "Basic"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Basic.vert"
                ; ShaderType.FragmentShader, @"Basic.frag" 
                ;]
            |> GlProg.withUniforms ["myColor"]
            |> GlProg.build ctx

        triangleVao <- 
            GlVao.create ctx
            |> GlVao.bind 
            |> fun (vao, _) -> vao

        let triangleVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames [ "Positions"]
            |> GlVbo.withAttrDefinitions [
                [[-0.5f;  -0.5f;  0.0f;]]
                [[0.5f; -0.5f;  0.0f;]]
                [[0.00f; 0.5f;   0.0f;]] ]
            |> GlVbo.build (triangleVao, ctx)
        ()

    let render (dt: float) =
        gl.Clear <| uint32 GLEnum.ColorBufferBit
        
        let greenValue = single <| Math.Sin(window.Time);

        (shaderBasic, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformV4 "myColor" (0.8f, greenValue, 0.0f, 1.0f)
        |> ignore
        gl.BindVertexArray triangleVao.GlVaoHandle
        gl.DrawArrays (GLEnum.Triangles, 0, 3u) 

    window.add_Load (new Action(load))
    window.add_Render (new Action<_>(render))
    window.Run ()
    0