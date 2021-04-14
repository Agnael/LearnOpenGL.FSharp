open System
open Galante.OpenGL
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics
open BaseCameraSlice
open BaseMouseSlice
open BaseFpsCounterSlice
open BaseWindowSlice
open GalanteMath
open BaselineState
open Game
open Galante

let initialState = 
    BaselineState.createDefault(
        "17_Phong_Specular_Lighting", 
        new Size(640, 360))

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
    let mutable shaderLighted = Unchecked.defaultof<_>
    let mutable shaderLightSource = Unchecked.defaultof<_>

    let lightSourcePosition = new Vector3(1.2f, 1.0f, 2.0f)
        
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
        shaderLighted <-
            GlProg.emptyBuilder
            |> GlProg.withName "Lighted"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Basic3d.vert"
                ; ShaderType.FragmentShader, @"Lighted.frag" 
                ;]
            |> GlProg.withUniforms [
                "uObjectColor"
                "uLightColor"
                "uModel"
                "uView"
                "uProjection"
                "uLightSourcePos"
                "uViewerPos"
            ]
            |> GlProg.build ctx

        shaderLightSource <-
            GlProg.emptyBuilder
            |> GlProg.withName "LightSource"
            |> GlProg.withShaders [
                ShaderType.VertexShader, "Basic3d.vert"
                ShaderType.FragmentShader, "LightSource.frag"
            ]
            |> GlProg.withUniforms [
                "uModel"
                "uView" 
                "uProjection"
            ]
            |> GlProg.build ctx

        cubeVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let cubeVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Normals"]
            |> GlVbo.withAttrDefinitions Cube.vertexPositionsAndNormals
            |> GlVbo.build (cubeVao, ctx)
                                    
        dispatch (Mouse UseCursorRaw)

        // Hardcoded camera position and target, so it looks just like the
        // LearnOpenGL.com example right away.
        dispatch (Camera (ForcePosition (new Vector3(1.35f, 1.51f, 3.84f))))
        dispatch (Camera (ForceTarget (new Vector3(-0.22f, -0.36f, -0.90f))))

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

        let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

        let res = state.Window.Resolution
        let fov = Radians.value <| toRadians(state.Camera.Fov)
        let ratio = single(res.Width) / single(res.Height)
        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)
       
        // Prepares the shader
        (shaderLighted, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformV3 "uObjectColor" (new Vector3(1.0f, 0.5f, 0.31f))
        |> GlProg.setUniformV3 "uLightColor" (new Vector3(1.0f, 1.0f, 1.0f))
        |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity 
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> GlProg.setUniformV3 "uLightSourcePos" lightSourcePosition
        |> GlProg.setUniformV3 "uViewerPos" state.Camera.Position
        |> ignore

        GlVao.bind (cubeVao, ctx) |> ignore        
        ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

        let lightSourceModelMatrix =
            Matrix4x4.CreateTranslation (lightSourcePosition / 0.2f)
            * Matrix4x4.CreateScale (new Vector3(0.2f))

        (shaderLightSource, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformM4x4 "uModel" lightSourceModelMatrix
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> ignore
        ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

        dispatch (FpsCounter(FrameRenderCompleted deltaTime))
        ()

    let onInputContextLoaded ctx ic state dispatch = 
        dispatch (Window (InitializeInputContext ic))

    let onWindowResize ctx state dispatch newSize =
        (ctx, state, dispatch, newSize)
        |> Baseline.handleWindowResize
        |> ignore

    emptyGameBuilder glWindowOptions initialState gameReducer gameActionFilter
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