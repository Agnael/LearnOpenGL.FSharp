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
            Title = "10_Coordinate_Systems_Cube_With_Depth_Testing"
            Size = new Size (800, 600) }

    let (window, ctx) = GlWin.create glOpts

    let mutable cubeVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
    let mutable timer = 0.0f
        
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
            |> GlVbo.withAttrDefinitions [    
                [[-0.5f; -0.5f; -0.5f]; [0.0f; 0.0f]]
                [[0.5f; -0.5f; -0.5f]; [1.0f; 0.0f]]
                [[0.5f;  0.5f; -0.5f]; [1.0f; 1.0f]]
                [[0.5f;  0.5f; -0.5f]; [1.0f; 1.0f]]
                [[-0.5f;  0.5f; -0.5f]; [0.0f; 1.0f]]
                [[-0.5f; -0.5f; -0.5f]; [0.0f; 0.0f]]
                
                [[ -0.5f; -0.5f;  0.5f]; [0.0f; 0.0f]]
                [[0.5f; -0.5f;  0.5f]; [1.0f; 0.0f]]
                [[0.5f;  0.5f;  0.5f]; [1.0f; 1.0f]]
                [[0.5f;  0.5f;  0.5f]; [ 1.0f; 1.0f]]
                [[-0.5f;  0.5f;  0.5f]; [0.0f; 1.0f]]
                [[-0.5f; -0.5f;  0.5f]; [0.0f; 0.0f]]
                
                [[-0.5f;  0.5f;  0.5f]; [1.0f; 0.0f]]
                [[-0.5f;  0.5f; -0.5f]; [1.0f; 1.0f]]
                [[-0.5f; -0.5f; -0.5f]; [0.0f; 1.0f]]
                [[-0.5f; -0.5f; -0.5f]; [0.0f; 1.0f]]
                [[-0.5f; -0.5f;  0.5f]; [0.0f; 0.0f]]
                [[-0.5f;  0.5f;  0.5f]; [1.0f; 0.0f]]
                
                [[0.5f;  0.5f;  0.5f]; [1.0f; 0.0f]]
                [[0.5f;  0.5f; -0.5f]; [1.0f; 1.0f]]
                [[0.5f; -0.5f; -0.5f]; [0.0f; 1.0f]]
                [[0.5f; -0.5f; -0.5f]; [0.0f; 1.0f]]
                [[0.5f; -0.5f;  0.5f]; [0.0f; 0.0f]]
                [[0.5f;  0.5f;  0.5f]; [1.0f; 0.0f]]
                
                [[-0.5f; -0.5f; -0.5f]; [0.0f; 1.0f]]
                [[0.5f; -0.5f; -0.5f]; [1.0f; 1.0f]]
                [[0.5f; -0.5f;  0.5f]; [1.0f; 0.0f]]
                [[0.5f; -0.5f;  0.5f]; [1.0f; 0.0f]]
                [[-0.5f; -0.5f;  0.5f]; [0.0f; 0.0f]]
                [[-0.5f; -0.5f; -0.5f]; [0.0f; 1.0f]]
                
                [[-0.5f;  0.5f; -0.5f]; [0.0f; 1.0f]]
                [[0.5f;  0.5f; -0.5f]; [1.0f; 1.0f]]
                [[0.5f;  0.5f;  0.5f]; [1.0f; 0.0f]]
                [[0.5f;  0.5f;  0.5f]; [1.0f; 0.0f]]
                [[-0.5f;  0.5f;  0.5f]; [0.0f; 0.0f]]
                [[-0.5f;  0.5f; -0.5f]; [0.0f; 1.0f]]
            ]
            |> GlVbo.build (cubeVao, ctx)
            
        texture1 <- GlTex.create2D @"wall.jpg" (cubeVao, ctx)
        texture2 <- GlTex.create2D @"awesomeface.png" (cubeVao, ctx)

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onUpdate dt =
        timer <- timer + float32(dt)
        ()
        
    let toRadians degrees = degrees * MathF.PI / 180.0f
    let fov = toRadians 45.0f
    let aspectRatio = 800.0f/600.0f

    let onRender dt =
        ctx.Gl.Enable GLEnum.DepthTest
        ctx.Gl.Clear <| uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)

        (cubeVao, ctx)
        |> GlTex.bind GLEnum.Texture0 texture1
        |> GlTex.bind GLEnum.Texture1 texture2
        |> ignore
        
        let rotationQuaternion = 
            new Quaternion(new Vector3(0.5f, 1.0f, 0.0f), toRadians(5.0f) * timer)

        let modelMatrix =
            Matrix4x4.CreateRotationX (toRadians(50.0f) * timer)
            * Matrix4x4.CreateRotationY (toRadians(50.0f) * timer)

        // Note that we're translating the scene in the reverse 
        // direction of where we want to move.
        let viewMatrix = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, -3.0f))

        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, 0.1f, 100.0f)
       
        // Prepares shader and draws the original image
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformI "texture1" 0
        |> GlProg.setUniformI "texture2" 1
        |> GlProg.setUniformM4x4 "uModel" modelMatrix
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> ignore
    
        GlVao.bind (cubeVao, ctx) |> ignore
        ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)
        ()
    
    window.add_Update (new Action<float>(onUpdate))
    window.add_Load (new Action(onLoad))
    window.add_Render (new Action<float>(onRender))
    window.Run ()
    0
