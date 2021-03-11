open System
open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics
open System.Linq
open Silk.NET.Input
open System.Collections.Generic
open Cube
open GalanteMath
open Galante

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

// List of cube copies that will be created, expressed as transformation of one 
// thats standing on the origin.
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

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "13_Camera_Walk_Around_With_Inputs"
            Size = new Size (800, 600) }

    let (window, ctx) = GlWin.create glOpts

    let mutable cubeVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
    let mutable timer = 0.0f
    
    let mutable camera = CameraState.Default

    let cameraSpeed = 2.5f
    let mutable cameraPosition = new Vector3(0.0f, 0.0f, 3.0f)
    let mutable cameraTarget = new Vector3(0.0f, 0.0f, -1.0f)
    let mutable cameraUp = new Vector3(0.0f, 1.0f, 0.0f)

    ////  To make sure the camera points towards the negative z-axis by default 
    //// we can give the yaw a default value of a 90 degree clockwise rotation. 
    //// Positive degrees rotate counter-clockwise so we set the default yaw value to:
    //let mutable cameraYaw = -90.0f
    //let mutable cameraPitch = 0.0f

    let mutable isMovingForward = false
    let mutable isMovingLeft = false
    let mutable isMovingRight = false
    let mutable isMovingBack = false
    let mutable isMovingUp = false
    let mutable isMovingDown = false

    let mouseSensitivity = 0.1f
    let zoomSpeed = 3.0f
    let mutable isAbsoluteFirstMouseInput = true
    let mutable lastMouseX = single (glOpts.Size.Width / 2)
    let mutable lastMouseY = single (glOpts.Size.Height / 2)

    let mutable fov = 45.0f
    let aspectRatio = 800.0f/600.0f
    
    let onKeyDown keyboard key id = 
        if key = Key.Escape then window.Close()
        if key = Key.W then isMovingForward <- true
        if key = Key.A then isMovingLeft <- true
        if key = Key.S then isMovingBack <- true
        if key = Key.D then isMovingRight <- true
        if key = Key.Space then isMovingUp <- true
        if key = Key.ShiftLeft then isMovingDown <- true
            
    let onKeyUp keyboard key id = 
        if key = Key.Escape then window.Close()
        if key = Key.W then isMovingForward <- false
        if key = Key.A then isMovingLeft <- false
        if key = Key.S then isMovingBack <- false
        if key = Key.D then isMovingRight <- false
        if key = Key.Space then isMovingUp <- false
        if key = Key.ShiftLeft then isMovingDown <- false

    let onMouseMove mouse (position: Vector2) = 
        // Without this block, you'll notice the camera makes a large sudden jump 
        // whenever the window first receives focus of your mouse cursor. The cause 
        // for this sudden jump is that as soon as your cursor enters the window the 
        // mouse callback function is called with an xpos and ypos position equal to 
        // the location your mouse entered the screen from. This is often a position 
        // that is significantly far away from the center of the screen, resulting in 
        // large offsets and thus a large movement jump. We can circumvent this issue 
        // by defining a global bool variable to check if this is the first time we 
        // receive mouse input. If it is the first time, we update the initial mouse 
        // positions to the new xpos and ypos values. The resulting mouse movements 
        // will then use the newly entered mouse's position coordinates to calculate 
        // the offsets.
        if isAbsoluteFirstMouseInput then
            lastMouseX <- position.X
            lastMouseY <- position.Y
            isAbsoluteFirstMouseInput <- false

        let mouseXoffset = position.X - lastMouseX

        // Reversed since y-coordinates range from bottom to top
        let mouseYoffset = lastMouseY - position.Y

        lastMouseX <- position.X
        lastMouseY <- position.Y

        camera <- 
            { camera with 
                Yaw = 
                    camera.Yaw
                    |> Degrees.value
                    |> fun yaw -> yaw + (mouseXoffset * mouseSensitivity)
                    |> Degrees.make
                Pitch = 
                    camera.Pitch
                    |> Degrees.value
                    |> fun oldPitch -> 
                        oldPitch + (mouseYoffset * mouseSensitivity)
                        |> fun newPitch -> 
                            if newPitch > 89.0f then 
                                89.0f
                            else if newPitch < -89.0f then 
                                -89.0f
                            else newPitch
                    |> Degrees.make }

    let onMouseWheelScroll mouse (wheel: ScrollWheel) = 
        fov <- fov - wheel.Y * zoomSpeed
        if fov < 20.0f then fov <- 20.0f
        if fov > 45.0f then fov <- 45.0f

    let onLoad () = 
        let inputs = window.CreateInput()

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
            |> GlVbo.withAttrDefinitions cubeVertexPositionsAndTexturePositions
            |> GlVbo.build (cubeVao, ctx)
            
        texture1 <- GlTex.create2D @"wall.jpg" (cubeVao, ctx)
        texture2 <- GlTex.create2D @"awesomeface.png" (cubeVao, ctx)

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
                
        let mainKeyboard = inputs.Keyboards.FirstOrDefault()

        if not (mainKeyboard = null) then
            mainKeyboard.add_KeyDown (new Action<IKeyboard, Key, int>(onKeyDown))
            mainKeyboard.add_KeyUp (new Action<IKeyboard, Key, int>(onKeyUp))

        let rec setMouseEventHandlers (mouseList: IMouse list) idx =
            match mouseList with
            | [] -> ()
            | h::t ->
                h.Cursor.CursorMode <- CursorMode.Disabled
                h.add_MouseMove (new Action<IMouse, Vector2>(onMouseMove))
                h.add_Scroll (new Action<IMouse, ScrollWheel>(onMouseWheelScroll))
                setMouseEventHandlers t (idx + 1)

        setMouseEventHandlers (List.ofSeq inputs.Mice) 0
        ()

    let normalizeCross (v1, v2) = Vector3.Normalize <| Vector3.Cross(v1, v2)

    let onUpdate dt =
        timer <- timer + single(dt)
        let dynCamSpeed = cameraSpeed * single(dt)

        if isMovingForward then 
            cameraPosition <- cameraPosition + (cameraTarget * dynCamSpeed)

        if isMovingBack then
            cameraPosition <- cameraPosition - (cameraTarget * dynCamSpeed)

        if isMovingLeft then 
            cameraPosition <- cameraPosition - normalizeCross(cameraTarget, cameraUp) * dynCamSpeed

        if isMovingRight then
            cameraPosition <- cameraPosition + normalizeCross(cameraTarget, cameraUp) * dynCamSpeed

        if isMovingUp then
            cameraPosition <- cameraPosition + (new Vector3(0.0f, 1.0f, 0.0f) * dynCamSpeed)

        if isMovingDown then
            cameraPosition <- cameraPosition + (new Vector3(0.0f, -1.0f, 0.0f) * dynCamSpeed)
            
        let newCamTargetX = MathF.Cos(toRadF cameraYaw) * MathF.Cos(toRadF cameraPitch)
        let newCamTargetY = MathF.Sin(toRadF cameraPitch)
        let newCamTargetZ = MathF.Sin(toRadF cameraYaw) * MathF.Cos(toRadF cameraPitch)

        cameraTarget <- 
            Vector3.Normalize(new Vector3(newCamTargetX, newCamTargetY, newCamTargetZ))

    let onRender dt =
        ctx.Gl.Enable GLEnum.DepthTest
        ctx.Gl.Clear <| uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)

        (cubeVao, ctx)
        |> GlTex.bind GLEnum.Texture0 texture1
        |> GlTex.bind GLEnum.Texture1 texture2
        |> ignore

        let viewMatrix = 
            Matrix4x4.CreateLookAt(cameraPosition, cameraPosition + cameraTarget, cameraUp)

        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(toRadF fov, aspectRatio, 0.1f, 100.0f)
       
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
                    Matrix4x4.CreateRotationX (toRadF rotationX)
                    * Matrix4x4.CreateRotationY (toRadF rotationY)
                    * Matrix4x4.CreateRotationZ (toRadF rotationZ)
                    * Matrix4x4.CreateTranslation cubeTransformations.[idx].Translation

                GlProg.setUniformM4x4 "uModel" modelMatrix (shader, ctx) |> ignore    
                ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

                drawEachTranslation t (idx + 1)

        drawEachTranslation cubeTransformations 0
        ()
    
    window.add_Update (new Action<float>(onUpdate))
    window.add_Load (new Action(onLoad))
    window.add_Render (new Action<float>(onRender))
    window.Run ()
    0
