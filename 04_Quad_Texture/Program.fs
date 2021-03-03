// Learn more about F# at http://fsharp.org

open System
open Galante
open Silk.NET.Windowing
open Silk.NET.OpenGL
open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Extensions.FileProviders
open System.IO
open System.Drawing

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "04_Quad_Texture"
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
            |> GlProg.withName "Textured"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Textured.vert"
                ; ShaderType.FragmentShader, @"Textured.frag" 
                ;]
            |> GlProg.withUniforms []
            |> GlProg.build ctx

        quadVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let quadVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
            |> GlVbo.withAttrDefinitions [
                [[-0.5f; 0.5f; 0.0f]; [0.0f; 0.0f]]
                [[0.5f; 0.5f; 0.0f]; [1.0f; 0.0f]]
                [[-0.5f; -0.5f; 0.0f]; [0.0f; 1.0f]]
                [[0.5f; -0.5f; 0.0f]; [1.0f; 1.0f]]
            ]
            |> GlVbo.build (quadVao, ctx)

        let quadTex =
            (quadVao, ctx)
            |> GlTex.create 
                ( TextureTarget.Texture2D
                , GLEnum.Texture2D
                , @"wall.jpg"
                , GLEnum.Rgba
                , GLEnum.Rgba
                , GLEnum.Repeat
                , GLEnum.Repeat
                , GLEnum.Linear
                , GLEnum.Linear
                )
            |> fun tex ->
                GlTex.bind tex (quadVao, ctx) |> ignore
                tex

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onRender dt =
        gl.Clear <| uint32 GLEnum.ColorBufferBit
        
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> ignore

        GlVao.bind (quadVao, ctx) |> ignore
        ctx.Gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
        ()

    window.add_Load (new Action(onLoad))
    window.add_Render (new Action<float>(onRender))
    window.Run ()
    0 // return an integer exit code
