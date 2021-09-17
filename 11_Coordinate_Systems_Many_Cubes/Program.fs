open System
open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "10_Coordinate_Systems_Many_Cubes"
            Size = new Size (800, 600) }

    let ctx = GlWin.create glOpts

    let mutable cubeVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
        
    let onLoad () = 
        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "3dShader"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Textured3d.vert"
                ; ShaderType.FragmentShader, @"DoubleTexture.frag" 
                ;]
            |> GlProg.withUniforms [
                "texture1"; 
                "texture2"; 
                "uModel"; 
                "uView"; 
                "uProjection"]
            |> GlProg.build ctx

        cubeVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let qubeVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
            |> GlVbo.withAttrDefinitions Cube.vertexPositionsAndTextureCoords
            |> GlVbo.build (cubeVao, ctx)
            
        texture1 <- 
            GlTex.loadImage @"wall.jpg" ctx
            |> fun img -> GlTex.create2d img ctx

        texture2 <- 
            GlTex.loadImage @"awesomeface.png" ctx
            |> fun img -> GlTex.create2d img ctx
        
        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onUpdate dt = ()
        
    let toRadians degrees = degrees * MathF.PI / 180.0f
    let fov = toRadians 45.0f
    let aspectRatio = 800.0f/600.0f

    // List of cube copies that will be created, expressed as translations.
    let cubeTranslations = [
        new Vector3(0.0f, 0.0f, 0.0f)
        new Vector3(2.0f, 5.0f, -15.0f)
        new Vector3(-1.5f, -2.2f, -2.5f)
        new Vector3(-3.8f, -2.0f, -12.3f)
        new Vector3(2.4f, -0.4f, -3.5f)
        new Vector3(-1.7f, 3.0f, -7.5f)
        new Vector3(1.3f, -2.0f, -2.5f)
        new Vector3(1.5f, 2.0f, -2.5f)
        new Vector3(1.5f, 0.2f, -1.5f)
        new Vector3(-1.3f, 1.0f, -1.5f)
    ]

    let onRender dt =
        ctx.Gl.Enable GLEnum.DepthTest
        ctx.Gl.Clear <| uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)

        (cubeVao, ctx)
        |> GlTex.setActive GLEnum.Texture0 texture1
        |> GlTex.setActive GLEnum.Texture1 texture2
        |> ignore
        
        // Note that we're translating the scene in the reverse 
        // direction of where we want to move.
        let viewMatrix = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, -3.0f))

        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, 0.1f, 100.0f)
       
        // Prepares the shader
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformI "texture1" 0
        |> GlProg.setUniformI "texture2" 1
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> ignore

        GlVao.bind (cubeVao, ctx) |> ignore
        
        // Draws a copy of the image per each transition registered, resulting in 
        // multiple cubes being rendered but always using the same VAO.
        let rec drawEachTranslation translations idx =
            match translations with
            | [] -> ()
            | h::t ->
                let modelMatrix =
                    Matrix4x4.CreateRotationX (toRadians(50.0f) * single(ctx.Window.Time))
                    * Matrix4x4.CreateRotationY (toRadians(50.0f) * single(ctx.Window.Time))
                    * Matrix4x4.CreateTranslation cubeTranslations.[idx]

                GlProg.setUniformM4x4 "uModel" modelMatrix (shader, ctx) |> ignore    
                ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

                drawEachTranslation t (idx + 1)

        drawEachTranslation cubeTranslations 0
        ()
    
    ctx.Window.add_Update (new Action<float>(onUpdate))
    ctx.Window.add_Load (new Action(onLoad))
    ctx.Window.add_Render (new Action<float>(onRender))
    ctx.Window.Run ()
    0
