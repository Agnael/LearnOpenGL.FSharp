﻿open System
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
        "26_Spotlight", 
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
    let mutable containerDiffuseMapTex = Unchecked.defaultof<_>
    let mutable containerSpecularMapTex = Unchecked.defaultof<_>
            
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
                "uModel"
                "uView"
                "uProjection"
                "uViewerPos"
                "uMaterial.diffuseMap"
                "uMaterial.specularMap"
                "uMaterial.shininess"
                "uLight.position"
                "uLight.direction"
                "uLight.innerCutOffAngleCos"
                "uLight.outerCutOffAngleCos"
                "uLight.constantComponent"
                "uLight.linearComponent"
                "uLight.quadraticComponent"
                "uLight.ambientColor"
                "uLight.diffuseColor"
                "uLight.specularColor"
            ]
            |> GlProg.build ctx

        cubeVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let cubeVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Normals"; "Texture coords"]
            |> GlVbo.withAttrDefinitions 
                Cube.vertexPositionsAndNormalsAndTextureCoords
            |> GlVbo.build (cubeVao, ctx)

        containerDiffuseMapTex <- GlTex.create2D "container_diffuse.png" (cubeVao, ctx)
        containerSpecularMapTex <- GlTex.create2D "container_specular.png" (cubeVao, ctx)
                                    
        // Hardcoded camera position and target, so it looks just like the
        // LearnOpenGL.com example right away.
        dispatch (Camera (ForcePosition (new Vector3(-1.74f, -1.51f, 1.86f))))
        dispatch (Camera (ForceTarget (new Vector3(0.56f, 0.53f, -0.62f))))

        // Comment this or press F10 to unlock the camera
        dispatch (Mouse UseCursorNormal)
        dispatch (Camera Lock)

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
        
        // Sets a dark grey background so the cube´s color changes are visible
        ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
        
        let time = single ctx.Window.Time
        let lightSrcPathRadius = 2.0f
        let lightSrcPathCurrX = MathF.Sin(time) * lightSrcPathRadius
        let lightSrcPathCurrZ = MathF.Cos(time) * lightSrcPathRadius

        let lightSrcPosition = 
            new Vector3(lightSrcPathCurrX, 1.3f, lightSrcPathCurrZ)

        let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

        let res = state.Window.Resolution
        let fov = Radians.value <| toRadians(state.Camera.Fov)
        let ratio = single(res.Width) / single(res.Height)
        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)
       
        let materialShininess = 32.0f

        let lightColor = new Vector3(1.0f)
            
        let lightDiffuseColor = lightColor * new Vector3(0.5f)
        let lightAmbientColor = lightDiffuseColor * new Vector3(0.2f)
        let lightSpecularColor = new Vector3(1.0f, 1.0f, 1.0f)

        (cubeVao, ctx)
        |> GlTex.bind GLEnum.Texture0 containerDiffuseMapTex
        |> GlTex.bind GLEnum.Texture1 containerSpecularMapTex
        |> ignore
        
        let outerCutOffCos = 
            Degrees.make 18.0f
            |> cosF

        let innerCutOffCos = 
            Degrees.make 14.0f
            |> cosF

        // Prepares the shader
        (shaderLighted, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> GlProg.setUniformV3 "uViewerPos" state.Camera.Position
        |> GlProg.setUniformI "uMaterial.diffuseMap" 0
        |> GlProg.setUniformI "uMaterial.specularMap" 1
        |> GlProg.setUniformF "uMaterial.shininess" materialShininess
        |> GlProg.setUniformV3 "uLight.position" state.Camera.Position
        |> GlProg.setUniformV3 "uLight.direction" state.Camera.TargetDirection
        |> GlProg.setUniformF "uLight.innerCutOffAngleCos" innerCutOffCos
        |> GlProg.setUniformF "uLight.outerCutOffAngleCos" outerCutOffCos
        |> GlProg.setUniformF "uLight.constantComponent" 1.0f
        |> GlProg.setUniformF "uLight.linearComponent" 0.09f
        |> GlProg.setUniformF "uLight.quadraticComponent" 0.032f
        |> GlProg.setUniformV3 "uLight.ambientColor" lightAmbientColor
        |> GlProg.setUniformV3 "uLight.diffuseColor" lightDiffuseColor
        |> GlProg.setUniformV3 "uLight.specularColor" lightSpecularColor
        |> ignore
        
        let rec drawEachTransformation transformations idx =
            match transformations with
            | [] -> ()
            | h::t ->
                let currTransform = Cube.transformations.[idx]
                let modelMatrix =
                    Matrix4x4.CreateRotationX currTransform.RotationRadsX
                    * Matrix4x4.CreateRotationY currTransform.RotationRadsY
                    * Matrix4x4.CreateRotationZ currTransform.RotationRadsZ
                    * Matrix4x4.CreateTranslation currTransform.Translation

                GlProg.setUniformM4x4 "uModel" modelMatrix (shaderLighted, ctx)
                |> ignore    

                ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)
                drawEachTransformation t (idx + 1)
        drawEachTransformation Cube.transformations 0

        dispatch (FpsCounter(FrameRenderCompleted deltaTime))
        ()

    let onInputContextLoaded ctx ic state dispatch = 
        dispatch (Window (InitializeInputContext ic))

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