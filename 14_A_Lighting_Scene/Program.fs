open System
open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics
open System.Linq
open Silk.NET.Input
open CameraSlice
open MouseSlice
open GalanteMath
open Aether
open GameState
open Sodium.Frp
open FpsCounterSlice
open WindowSlice
open StoreFactory
open Game

let initialState = 
    GameState.createDefault(
        "13_Camera_Walk_Around_With_Inputs", 
        new Size(800, 600))

let initialRes = initialState.Window.Resolution

[<EntryPoint>]
let main argv =
    let (getState, dispatch) = 
        createStore(initialState, gameMainReducer)

    // No need to get the current state since this is executed
    // before starting the game loop, so using the initial state 
    // is just fine.
    let glOpts = 
        { GlWindowOptions.Default with
            IsVsync = false
            Title = initialState.Window.Title
            Size = initialRes }

    let ctx = GlWin.create glOpts

    let game =
        emptyGameBuilder glOpts initialState gameMainReducer
        |> buildAndRun

    let mutable cubeVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
    
    let toRadians degrees = degrees * MathF.PI / 180.0f
    
    let sendMovement cameraAction = dispatch (Camera cameraAction)
    let sendWindow windowAction = dispatch (Window windowAction)
    let sendResolutionUpdate w h = 
        dispatch (Window (ResolutionUpdate (new Size(w, h))))

    let onKeyDown keyboard key id =         
        match key with
        | Key.W         -> sendMovement MoveForwardStart
        | Key.A         -> sendMovement MoveLeftStart
        | Key.S         -> sendMovement MoveBackStart
        | Key.D         -> sendMovement MoveRightStart
        | Key.Space     -> sendMovement MoveUpStart
        | Key.ShiftLeft -> sendMovement MoveDownStart        
        | Key.F5        -> sendResolutionUpdate initialRes.Width initialRes.Height
        | Key.F6        -> sendResolutionUpdate 1280 720
        | Key.F1        -> dispatch (Window ToggleFullscreen)
        | Key.Escape    -> sendWindow Close
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
        let state = getState()
        
        dispatch (Mouse (NewPosition newPos))

        if not state.Mouse.IsFirstMoveReceived then
            dispatch (Mouse FirstMoveReceived)
        else
            // Reversed  y-coordinates since they range from bottom to top
            let cameraOffset =
                { CameraOffset.X = newPos.X - state.Mouse.X 
                ; CameraOffset.Y = state.Mouse.Y - newPos.Y
                ;}
            dispatch (Camera (AngularChange cameraOffset))

    let onMouseWheelScroll mouse (wheel: ScrollWheel) = 
        let state = getState()
        dispatch (Camera (ZoomChange (ZoomOffset(wheel.Y * state.Camera.ZoomSpeed))))

    let onLoad () = 
        let inputs = ctx.Window.CreateInput()

        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "3dShader"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Textured3d.vert"
                ; ShaderType.FragmentShader, @"DoubleTexture.frag" 
                ;]
            |> GlProg.withUniforms ["uTex1"; "uTex2"; "uModel"; "uView"; "uProjection"]
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
        //let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
                
        let mainKeyboard = inputs.Keyboards.FirstOrDefault()

        if not (mainKeyboard = null) then
            mainKeyboard.add_KeyDown (new Action<IKeyboard, Key, int>(onKeyDown))
            mainKeyboard.add_KeyUp (new Action<IKeyboard, Key, int>(onKeyUp))

        let rec setMouseEventHandlers (mouseList: IMouse list) idx =
            match mouseList with
            | [] -> ()
            | h::t ->
                h.Cursor.CursorMode <- CursorMode.Raw
                h.add_MouseMove (new Action<IMouse, Vector2>(onMouseMove))
                h.add_Scroll (new Action<IMouse, ScrollWheel>(onMouseWheelScroll))
                setMouseEventHandlers t (idx + 1)

        setMouseEventHandlers (List.ofSeq inputs.Mice) 0
        ()

    let onUpdate dt =
        let state = getState()

        if state.Window.ShouldClose then 
            ctx.Window.Close()

        if state.Window.ShouldUpdateResolution then
            let res = state.Window.Resolution
            let newSize = new Silk.NET.Maths.Vector2D<int>(res.Width, res.Height)

            ctx.Window.Size <- newSize                
            ctx.Gl.Viewport (0, 0, uint32 newSize.X, uint32 newSize.Y)
            dispatch (Window ResolutionUpdated)

        if 
            state.Window.IsFullscreen && 
            not (ctx.Window.WindowState = WindowState.Fullscreen) 
        then      
            let displaySize = ctx.Window.Monitor.Bounds.Size

            ctx.Gl.Viewport (0, 0, uint32 displaySize.X, uint32 displaySize.Y)
            ctx.Window.WindowState <- WindowState.Fullscreen

        elif 
            state.Window.IsFullscreen = false && 
            ctx.Window.WindowState = WindowState.Fullscreen 
        then
            let res = state.Window.Resolution
            let newSize = new Silk.NET.Maths.Vector2D<int>(res.Width, res.Height)

            ctx.Gl.Viewport (0, 0, uint32 newSize.X, uint32 newSize.Y)
            ctx.Window.WindowState <- WindowState.Normal
            sendResolutionUpdate res.Width res.Height
                
        ctx.Window.Title <- 
            sprintf "%s [%i FPS]" state.Window.Title state.FpsCounter.CurrentFps

        let dynCamSpeed = CameraSpeed.make <| state.Camera.MoveSpeed * single(dt)

        dispatch (Camera (UpdatePosition dynCamSpeed))

    let onRender dt =
        let state = getState()

        ctx.Gl.Enable GLEnum.DepthTest
        ctx.Gl.Clear <| uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)

        (cubeVao, ctx)
        |> GlTex.bind GLEnum.Texture0 texture1
        |> GlTex.bind GLEnum.Texture1 texture2
        |> ignore

        let viewMatrix = CameraSlice.createViewMatrix state.Camera

        let fovRadians = toRadians <| Degrees.value state.Camera.Fov

        let aspectRatio = 
            single(state.Window.Resolution.Width) / single(state.Window.Resolution.Height)
        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspectRatio, 0.1f, 100.0f)
       
        // Prepares the shader
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformI "uTex1" 0
        |> GlProg.setUniformI "uTex2" 1
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

        dispatch (FpsCounter(FrameRenderCompleted dt))
        ()
    
    ctx.Window.add_Update (new Action<float>(onUpdate))
    ctx.Window.add_Load (new Action(onLoad))
    ctx.Window.add_Render (new Action<float>(onRender))
    ctx.Window.Run ()
    0
