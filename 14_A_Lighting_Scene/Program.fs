open System
open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics
open Silk.NET.Input
open CameraSlice
open MouseSlice
open GalanteMath
open GameState
open FpsCounterSlice
open WindowSlice
open Game
open Galante

let initialState = 
    GameState.createDefault(
        "13_Camera_Walk_Around_With_Inputs", 
        new Size(720, 480))

let initialRes = initialState.Window.Resolution

[<EntryPoint>]
let main argv =
    // No need to get the current state since this is executed
    // before starting the game loop, so using the initial state 
    // is just fine.
    let glWindowOptions = 
        { GlWindowOptions.Default with
            IsVsync = false
            Title = initialState.Window.Title
            Size = initialRes }

    let mutable cubeVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
        
    let aMovement dispatch cameraAction = dispatch (Camera cameraAction)
    let aWindow dispatch windowAction = dispatch (Window windowAction)
    let aMouse dispatch mouseAction = dispatch (Mouse mouseAction)
    let aResolutionUpdate dispatch w h  = 
        dispatch (Window (ResolutionUpdate (new Size(w, h))))
    let aCamera dispatch cameraAction = dispatch (Camera cameraAction)

    let onKeyDown ctx state dispatch kb key =
        let initResW = initialState.Window.Resolution.Width
        let initResH = initialState.Window.Resolution.Height

        (ctx, state, dispatch, kb, key)        
        |> Baseline.detectFullScreenSwitch // ALT+ENTER        
        |> Baseline.detectGameClosing // ESC        
        |> Baseline.detectCameraMovementStart // W|A|S|D|Left_Shift|Space
        |> Baseline.detectResolutionChange initResW initResH // F5|F6|F7
        |> Baseline.detectCursorModeChange // F9|F10
        |> ignore

    let onKeyUp ctx state dispatch kb key =         
        (ctx, state, dispatch, kb, key)    
        |> Baseline.detectCameraMovementStop
        |> ignore

    let onMouseMove ctx state dispatch newPos = 
        (ctx, state, dispatch, newPos)
        |> Baseline.handleCameraAngularChange
        |> ignore
                
    let onMouseWheel ctx state dispatch (newPos: Vector2) =
        (ctx, state, dispatch, newPos)
        |> Baseline.handleCameraZoom
        |> ignore
            
    let onLoad (ctx: GlWindowCtx) input state dispatch =
        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "3dShader"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Textured3d.vert"
                ; ShaderType.FragmentShader, @"DoubleTexture.frag" 
                ;]
            |> GlProg.withUniforms 
                ["uTex1"; "uTex2"; "uModel"; "uView"; "uProjection"]
            |> GlProg.build ctx

        cubeVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let qubeVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
            |> GlVbo.withAttrDefinitions
                Cube.vertexPositionsAndTexturePositions
            |> GlVbo.build (cubeVao, ctx)
            
        texture1 <- GlTex.create2D @"wall.jpg" (cubeVao, ctx)
        texture2 <- GlTex.create2D @"awesomeface.png" (cubeVao, ctx)
                        
        aMouse dispatch UseCursorRaw

    let onUpdate (ctx: GlWindowCtx) (state) dispatch (DeltaTime deltaTime) =
        (ctx, state, dispatch, deltaTime)
        |> Baseline.updateWindowClosure
        |> Baseline.updateWindowTitle
        |> Baseline.updateWindowResolution
        |> Baseline.updateWindowMode
        |> Baseline.updateCursorMode
        |> Baseline.updateCameraPosition
        |> ignore

    let onRender ctx state dispatch (DeltaTime deltaTime) =
        ctx.Gl.Enable GLEnum.DepthTest

        uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
        |> ctx.Gl.Clear

        (cubeVao, ctx)
        |> GlTex.bind GLEnum.Texture0 texture1
        |> GlTex.bind GLEnum.Texture1 texture2
        |> ignore

        let viewMatrix = CameraSlice.createViewMatrix state.Camera

        let res = state.Window.Resolution
        let fovRads = Radians.value <| toRadF(state.Camera.Fov)

        let ratio = single(res.Width) / single(res.Height)
        let projectionMatrix = 
            Matrix4x4
                .CreatePerspectiveFieldOfView(fovRads, ratio, 0.1f, 100.0f)
       
        // Prepares the shader
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformI "uTex1" 0
        |> GlProg.setUniformI "uTex2" 1
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> ignore

        GlVao.bind (cubeVao, ctx) |> ignore
        
        // Draws a copy of the image per each transition registered, 
        // resulting in multiple cubes being rendered but always using 
        // the same VAO.
        let rec drawEachTranslation translations idx =
            match translations with
            | [] -> ()
            | h::t ->
                let currentTransform =  Cube.transformations.[idx]
                let rotationX = toRadF(Degrees.make currentTransform.RotationX)
                let rotationY = toRadF(Degrees.make currentTransform.RotationY)
                let rotationZ = toRadF(Degrees.make currentTransform.RotationZ)
                
                let modelMatrix =
                    Matrix4x4.CreateRotationX (Radians.value rotationX)
                    * Matrix4x4.CreateRotationY (Radians.value rotationY)
                    * Matrix4x4.CreateRotationZ (Radians.value rotationZ)
                    * Matrix4x4.CreateTranslation currentTransform.Translation

                GlProg.setUniformM4x4 "uModel" modelMatrix (shader, ctx) 
                |> ignore

                ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

                drawEachTranslation t (idx + 1)

        drawEachTranslation Cube.transformations 0

        dispatch (FpsCounter(FrameRenderCompleted deltaTime))
        ()

    let onInputContextLoaded ctx ic state dispatch = 
        aWindow dispatch (InitializeInputContext ic)

    let onWindowResize ctx state dispatch newSize =
        (ctx, state, dispatch, newSize)
        |> Baseline.handleWindowResize
        |> ignore

    emptyGameBuilder glWindowOptions initialState gameReducer
    |> withOnInputContextLoadedCallback onInputContextLoaded
    |> addOnLoad onLoad
    |> addOnUpdate onUpdate
    |> addOnRender onRender
    |> addOnKeyDown onKeyDown
    |> addOnKeyUp onKeyUp
    |> addOnMouseMove onMouseMove
    |> addOnMouseWheel onMouseWheel
    |> addOnWindowResize onWindowResize
    |> buildAndRun
    0