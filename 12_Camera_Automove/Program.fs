open System
open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics

type CubeTransformation = 
    { Translation: Vector3
    ; RotationX: single
    ; RotationY: single
    ; RotationZ: single
    ;}

    static member create (translation, rotationX, rotationY, rotationZ) =
        { Translation = translation
        ; RotationX = rotationX
        ; RotationY = rotationY
        ; RotationZ = rotationZ 
        ;}

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "12_Camera_Automove"
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
            |> GlVbo.withAttrDefinitions [|     
                [| [| -0.5f; -0.5f; -0.5f |]; [| 0.0f; 0.0f |] |]
                [| [|  0.5f; -0.5f; -0.5f |]; [| 1.0f; 0.0f |] |]
                [| [|  0.5f;  0.5f; -0.5f |]; [| 1.0f; 1.0f |] |]
                [| [|  0.5f;  0.5f; -0.5f |]; [| 1.0f; 1.0f |] |]
                [| [| -0.5f;  0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                [| [| -0.5f; -0.5f; -0.5f |]; [| 0.0f; 0.0f |] |]
                
                [| [| -0.5f; -0.5f;  0.5f |]; [| 0.0f; 0.0f |] |]
                [| [|  0.5f; -0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                [| [|  0.5f;  0.5f;  0.5f |]; [| 1.0f; 1.0f |] |]
                [| [|  0.5f;  0.5f;  0.5f |]; [|  1.0f; 1.0f |] |]
                [| [| -0.5f;  0.5f;  0.5f |]; [| 0.0f; 1.0f |] |]
                [| [| -0.5f; -0.5f;  0.5f |]; [| 0.0f; 0.0f |] |]
                
                [| [| -0.5f;  0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                [| [| -0.5f;  0.5f; -0.5f |]; [| 1.0f; 1.0f |] |]
                [| [| -0.5f; -0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                [| [| -0.5f; -0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                [| [| -0.5f; -0.5f;  0.5f |]; [| 0.0f; 0.0f |] |]
                [| [| -0.5f;  0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                
                [| [|  0.5f;  0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                [| [|  0.5f;  0.5f; -0.5f |]; [| 1.0f; 1.0f |] |]
                [| [|  0.5f; -0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                [| [|  0.5f; -0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                [| [|  0.5f; -0.5f;  0.5f |]; [| 0.0f; 0.0f |] |]
                [| [|  0.5f;  0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                
                [| [| -0.5f; -0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                [| [|  0.5f; -0.5f; -0.5f |]; [| 1.0f; 1.0f |] |]
                [| [|  0.5f; -0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                [| [|  0.5f; -0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                [| [| -0.5f; -0.5f;  0.5f |]; [| 0.0f; 0.0f |] |]
                [| [| -0.5f; -0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                
                [| [| -0.5f;  0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
                [| [|  0.5f;  0.5f; -0.5f |]; [| 1.0f; 1.0f |] |]
                [| [|  0.5f;  0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                [| [|  0.5f;  0.5f;  0.5f |]; [| 1.0f; 0.0f |] |]
                [| [| -0.5f;  0.5f;  0.5f |]; [| 0.0f; 0.0f |] |]
                [| [| -0.5f;  0.5f; -0.5f |]; [| 0.0f; 1.0f |] |]
             |]
            |> GlVbo.build (cubeVao, ctx)
            
        texture1 <- GlTex.create2D @"wall.jpg" ctx
        texture2 <- GlTex.create2D @"awesomeface.png" ctx
        
        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onUpdate dt = ()
        
    let toRadians degrees = degrees * MathF.PI / 180.0f
    let fov = toRadians 45.0f
    let aspectRatio = 800.0f/600.0f
    
    // List of cube copies that will be created, expressed as transformation of one 
    // thats standing on the origin.
    // 
    // NOTE: This is just to add some dynamism without being TOO distracting from 
    // the actual spotlight of the example, which is the camera and it's movement.
    let cubeTransformations = [
        CubeTransformation.create (new Vector3(0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f)
        CubeTransformation.create (new Vector3(2.0f, 5.0f, -15.0f), 43.0f, 12.0f, 0.0f)
        CubeTransformation.create (new Vector3(-1.5f, -2.2f, -2.5f), 12.0f, 98.0f, 40.0f)
        CubeTransformation.create (new Vector3(-3.8f, -2.0f, -12.3f), 45.0f, 32.0f, 0.0f)
        CubeTransformation.create (new Vector3(2.4f, -0.4f, -3.5f), 0.0f, 0.0f, 43.0f)
        CubeTransformation.create (new Vector3(-1.7f, 3.0f, -7.5f), 0.0f, 54.0f, 0.0f)
        CubeTransformation.create (new Vector3(1.3f, -2.0f, -2.5f), 14.0f, 54.0f, 12.0f)
        CubeTransformation.create (new Vector3(1.5f, 2.0f, -2.5f), 76.5f, 0.56f, 12.0f)
        CubeTransformation.create (new Vector3(1.5f, 0.2f, -1.5f), 54.0f, 0.0f, 125.0f)
        CubeTransformation.create (new Vector3(-1.3f, 1.0f, -1.5f), 246.0f, 122.0f, 243.0f)
    ] 

    let onRender dt =
        ctx.Gl.Enable GLEnum.DepthTest
        ctx.Gl.Clear <| uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)

        (cubeVao, ctx)
        |> GlTex.setActive GLEnum.Texture0 texture1
        |> GlTex.setActive GLEnum.Texture1 texture2
        |> ignore

        // Creates the current position of the camera on each frame so it looks 
        // like the camera is moving around the scene.
        let cameraPathRadius = 10.0
        let cameraPathCurrentX = sin(ctx.Window.Time) * cameraPathRadius
        let cameraPathCurrentZ = cos(ctx.Window.Time) * cameraPathRadius

        // 1. Camera position
        let cameraPosition = new Vector3 (single cameraPathCurrentX, 0.0f, single cameraPathCurrentZ)

        // 2. Camera direction
        let cameraTarget = new Vector3(0.0f, 0.0f, 0.0f)

        // The next vector required is the camera's direction e.g. at what direction 
        // it is pointing at. For now we let the camera point to the origin of our 
        // scene: (0,0,0). Remember that if we subtract two vectors from each other 
        // we get a vector that's the difference of these two vectors? Subtracting the 
        // camera position vector from the scene's origin vector thus results in the 
        // direction vector we want. For the view matrix's coordinate system we want 
        // its z-axis to be positive and because by convention (in OpenGL) the camera 
        // points towards the negative z-axis we want to negate the direction vector. 
        // If we switch the subtraction order around we now get a vector pointing 
        // towards the camera's positive z-axis.
        let cameraReverseDirection = Vector3.Normalize (cameraPosition - cameraTarget)

        // 3. Camera right axis
        // The next vector that we need is a right vector that represents the 
        // positive x-axis of the camera space. To get the right vector we use a 
        // little trick by first specifying an up vector that points upwards 
        // (in world space). Then we do a cross product on the up vector and the 
        // direction vector from step 2. Since the result of a cross product is a 
        // vector perpendicular to both vectors, we will get a vector that points 
        // in the positive x-axis's direction (if we would switch the cross product 
        // order we'd get a vector that points in the negative x-axis).
        let absoluteUp = new Vector3(0.0f, 1.0f, 0.0f)
        let cameraRight = 
            Vector3.Normalize (Vector3.Cross(absoluteUp, cameraReverseDirection))

        // 4. Camera up axis
        // Now that we have both the x-axis vector and the z-axis vector, retrieving 
        // the vector that points to the camera's positive y-axis is relatively 
        // easy: we take the cross product of the right and direction vector.
        let cameraUp = Vector3.Cross (cameraReverseDirection, cameraRight)

        // Note that we're translating the scene in the reverse 
        // direction of where we want to move.
        let viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUp)

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
                let rotationX = cubeTransformations.[idx].RotationX
                let rotationY = cubeTransformations.[idx].RotationY
                let rotationZ = cubeTransformations.[idx].RotationZ

                let modelMatrix =
                    Matrix4x4.CreateRotationX (toRadians rotationX)
                    * Matrix4x4.CreateRotationY (toRadians rotationY)
                    * Matrix4x4.CreateRotationZ (toRadians rotationZ)
                    * Matrix4x4.CreateTranslation cubeTransformations.[idx].Translation

                GlProg.setUniformM4x4 "uModel" modelMatrix (shader, ctx) |> ignore    
                ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

                drawEachTranslation t (idx + 1)

        drawEachTranslation cubeTransformations 0
        ()
    
    ctx.Window.add_Update (new Action<float>(onUpdate))
    ctx.Window.add_Load (new Action(onLoad))
    ctx.Window.add_Render (new Action<float>(onRender))
    ctx.Window.Run ()
    0
