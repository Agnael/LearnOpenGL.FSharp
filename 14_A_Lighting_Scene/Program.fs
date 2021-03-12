open System
open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics
open System.Linq
open Silk.NET.Input
open System.Collections.Generic
open CameraSlice
open MouseSlice
open GalanteMath
open Aether
open Sodium.Frp

type GameAction =
    | Camera of CameraAction
    | Mouse of MouseAction
    | Quit

type GameState =
    { Camera: CameraState 
    ; Mouse: MouseState
    ; ShouldQuit: bool
    ;}
    static member Default = 
        { Camera = CameraState.Default 
        ; Mouse = MouseState.Default
        ; ShouldQuit = false
        ;}

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
    
    let actionSink = sinkS<GameAction>()
    let game =
        loopWithNoCapturesC 
            (fun game ->
                actionSink
                |> snapshotC game 
                    (fun gameAction game -> 
                        match gameAction with
                        | Camera action ->
                            { game with Camera = CameraSlice.reduce action game.Camera }
                        | Mouse action ->
                            { game with Mouse = MouseSlice.reduce action game.Mouse }
                        | Quit -> 
                            { game with ShouldQuit = true }
                    )
                |> Stream.hold GameState.Default
            )

    let zoomSpeed = 3.0f

    let toRadians degrees = degrees * MathF.PI / 180.0f
    let mutable fov = 45.0f
    let aspectRatio = 800.0f/600.0f
    
    let sendMovement cameraAction = 
        sendS (Camera cameraAction) actionSink

    let onKeyDown keyboard key id =         
        match key with
        | Key.W         -> sendMovement MoveForwardStart
        | Key.A         -> sendMovement MoveLeftStart
        | Key.S         -> sendMovement MoveBackStart
        | Key.D         -> sendMovement MoveRightStart
        | Key.Space     -> sendMovement MoveUpStart
        | Key.ShiftLeft -> sendMovement MoveDownStart

        | Key.Escape    -> sendS Quit actionSink
        | _ -> ()
            
    let onKeyUp keyboard key id = 
        match key with
        | Key.W         -> sendMovement MoveForwardStop
        | Key.A         -> sendMovement MoveLeftStop
        | Key.S         -> sendMovement MoveBackStop
        | Key.D         -> sendMovement MoveRightStop
        | Key.Space     -> sendMovement MoveUpStop
        | Key.ShiftLeft -> sendMovement MoveDownStop
        | _ -> ()

    let onMouseMove mouse newPos = 
        let game = sampleC game
        
        sendS (Mouse (NewPosition newPos)) actionSink

        if not game.Mouse.IsFirstMoveReceived then
            sendS (Mouse FirstMoveReceived) actionSink
        else
            // Reversed  y-coordinates since they range from bottom to top
            let cameraOffset =
                { CameraOffset.X = newPos.X - game.Mouse.X 
                ; CameraOffset.Y = game.Mouse.Y - newPos.Y
                ;}
            sendS (Camera (AngularChange cameraOffset)) actionSink

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
            |> GlVbo.withAttrDefinitions Cube.vertexPositionsAndTexturePositions
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

    let onUpdate dt =
        let game = sampleC game

        if game.ShouldQuit then window.Close()
        
        timer <- timer + single(dt)
        let dynCamSpeed = CameraSpeed.make <| game.Camera.Speed * single(dt)

        sendS (Camera (UpdatePosition dynCamSpeed)) actionSink

    let onRender dt =
        let game = sampleC game

        ctx.Gl.Enable GLEnum.DepthTest
        ctx.Gl.Clear <| uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)

        (cubeVao, ctx)
        |> GlTex.bind GLEnum.Texture0 texture1
        |> GlTex.bind GLEnum.Texture1 texture2
        |> ignore

        let viewMatrix = CameraSlice.createViewMatrix game.Camera

        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(toRadians fov, aspectRatio, 0.1f, 100.0f)
       
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
                let rotationX = Cube.transformations.[idx].RotationX
                let rotationY = Cube.transformations.[idx].RotationY
                let rotationZ = Cube.transformations.[idx].RotationZ

                let modelMatrix =
                    Matrix4x4.CreateRotationX (toRadians rotationX)
                    * Matrix4x4.CreateRotationY (toRadians rotationY)
                    * Matrix4x4.CreateRotationZ (toRadians rotationZ)
                    * Matrix4x4.CreateTranslation Cube.transformations.[idx].Translation

                GlProg.setUniformM4x4 "uModel" modelMatrix (shader, ctx) |> ignore    
                ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

                drawEachTranslation t (idx + 1)

        drawEachTranslation Cube.transformations 0
        ()
    
    window.add_Update (new Action<float>(onUpdate))
    window.add_Load (new Action(onLoad))
    window.add_Render (new Action<float>(onRender))
    window.Run ()
    0
