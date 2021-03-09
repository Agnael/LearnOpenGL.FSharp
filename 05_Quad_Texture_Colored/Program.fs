open System
open Galante
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "05_Quad_Texture_Colored"
            Size = new Size (600, 600) }

    let (window, ctx) = GlWin.create glOpts

    let mutable quadVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    
    let onLoad () = 
        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "TextureColored"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"TextureColored.vert"
                ; ShaderType.FragmentShader, @"TextureColored.frag" 
                ;]
            |> GlProg.withUniforms []
            |> GlProg.build ctx

        quadVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let quadVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Color"; "Texture coords"]
            |> GlVbo.withAttrDefinitions [
                [[-0.5f; 0.5f; 0.0f]; [1.0f; 0.0f; 0.0f]; [0.0f; 0.0f]]
                [[0.5f; 0.5f; 0.0f]; [0.0f; 0.1f; 0.0f]; [1.0f; 0.0f]]
                [[-0.5f; -0.5f; 0.0f]; [0.0f; 0.0f; 0.1f]; [0.0f; 1.0f]]
                [[0.5f; -0.5f; 0.0f]; [1.0f; 1.0f; 0.0f]; [1.0f; 1.0f]]
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