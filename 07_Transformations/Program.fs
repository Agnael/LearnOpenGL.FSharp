open System
open Galante
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Extensions.FileProviders
open System.IO

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "06_Quad_Texture_Double"
            Size = new Size (600, 600) }

    let (window, gl) = GlWin.create glOpts

    let ctx =  
        { GlContext.Gl = gl
        ; Logger = new NullLogger<obj>()
        ; FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory())
        ;}

    let mutable quadVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
    
    let onLoad () = 
        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "DoubleTexture"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"DoubleTextureWithTransformer.vert"
                ; ShaderType.FragmentShader, @"DoubleTexture.frag" 
                ;]
            |> GlProg.withUniforms ["texture1"; "texture2"]
            |> GlProg.build ctx

        quadVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let quadVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
            |> GlVbo.withAttrDefinitions [
                [[-0.5f; 0.5f; 0.0f]; [0.0f; 1.0f]]
                [[0.5f; 0.5f; 0.0f]; [1.0f; 1.0f]]
                [[-0.5f; -0.5f; 0.0f]; [0.0f; 0.0f]]
                [[0.5f; -0.5f; 0.0f]; [1.0f; 0.0f]]
            ]
            |> GlVbo.build (quadVao, ctx)
            
        texture1 <- GlTex.create2D @"wall.jpg" (quadVao, ctx)
        texture2 <- GlTex.create2D @"awesomeface.png" (quadVao, ctx)

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onRender dt =
        gl.Clear <| uint32 GLEnum.ColorBufferBit
        
        ctx.Gl.ActiveTexture (GLEnum.Texture0)
        GlTex.bind texture1 (quadVao, ctx) |> ignore

        let maxTextureSlot = ctx.Gl.GetInteger(GLEnum.MaxCombinedTextureImageUnits);        

        ctx.Gl.ActiveTexture (GLEnum.Texture1)
        GlTex.bind texture2 (quadVao, ctx) |> ignore
        
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformI "texture1" 0
        |> GlProg.setUniformI "texture2" 1
        |> ignore

        GlVao.bind (quadVao, ctx) |> ignore
        ctx.Gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
        ()

    window.add_Load (new Action(onLoad))
    window.add_Render (new Action<float>(onRender))
    window.Run ()
    0