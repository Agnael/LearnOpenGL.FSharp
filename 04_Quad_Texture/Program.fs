open System
open Galante
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "04_Quad_Texture"
            Size = new Size (600, 600) }

    let (window, ctx) = GlWin.create glOpts

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

        let quadTex = GlTex.create2D "wall.jpg" (quadVao, ctx)

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onRender dt =
        ctx.Gl.Clear <| uint32 GLEnum.ColorBufferBit
        
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> ignore

        GlVao.bind (quadVao, ctx) |> ignore
        ctx.Gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
        ()

    window.add_Load (new Action(onLoad))
    window.add_Render (new Action<float>(onRender))
    window.Run ()
    0
