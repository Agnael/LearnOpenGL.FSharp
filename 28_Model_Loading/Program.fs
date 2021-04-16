open System
open Galante.OpenGL
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics
open BaseCameraSlice
open BaseMouseSlice
open BaseFpsCounterSlice
open BaseWindowSlice
open BaseAssetSlice
open GalanteMath
open BaselineState
open Game
open Galante
open Microsoft.FSharp.NativeInterop
open Model
open System.IO
open System.Threading
open System.Threading.Tasks

#nowarn "9"

let initialState = 
    BaselineState.createDefault(
        "28_Model_Loading", 
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

    let mutable shaderSimple3d = Unchecked.defaultof<_>
    let mutable shaderLightSrc = Unchecked.defaultof<_>
    let mutable fallbackGlTexture = Unchecked.defaultof<_>
    let mutable cubeVao = Unchecked.defaultof<_>
    
    let mutable mdlBackpack = Unchecked.defaultof<_>
    let mutable mdlNanosuit = Unchecked.defaultof<_>
    let mutable mdlCyborg = Unchecked.defaultof<_>
    let mutable mdlPlanet = Unchecked.defaultof<_>
    let mutable mdlRock = Unchecked.defaultof<_>
    let mutable mdlVampire = Unchecked.defaultof<_>
    let mutable mdlMannequin = Unchecked.defaultof<_>    
    
    let lightColor = new Vector3(1.0f)
        
    let lightDiffuseColor = lightColor * new Vector3(0.5f)
    let lightAmbientColor = lightDiffuseColor * new Vector3(0.2f)
    let lightSpecularColor = new Vector3(1.0f, 1.0f, 1.0f)
    
    // Attenuation constants hand tweaked but based on:
    // TABLE: http://wiki.ogre3d.org/-Point+Light+Attenuation
    let attenuationKc = 0.6f
    let attenuationKl = 0.1f
    let attenuationKq = 2.0f
         
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
        let fallbackImagePath = Path.Combine("Resources", "Textures", "fallback.jpg")
        fallbackGlTexture <-
            GlTex.loadImage fallbackImagePath ctx
            |> fun img -> GlTex.create2D img ctx

        shaderSimple3d <-
            GlProg.emptyBuilder
            |> GlProg.withName "Simple3D"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Simple3d.vert"
                ; ShaderType.FragmentShader, @"Simple3d.frag" 
                ;]
            |> GlProg.withUniforms [
                "uModel"
                "uView"
                "uProjection"
                "uMaterial.diffuseMap"
                "uMaterial.specularMap"
                "uMaterial.shininess"
                "uViewerPos"
                "uPointLight.position"
                "uPointLight.constantComponent"
                "uPointLight.linearComponent"
                "uPointLight.quadraticComponent"
                "uPointLight.ambientColor"
                "uPointLight.diffuseColor"
                "uPointLight.specularColor"
                "uDirectionalLight.direction"
                "uDirectionalLight.ambientColor"
                "uDirectionalLight.diffuseColor"
                "uDirectionalLight.specularColor"
            ]
            |> GlProg.build ctx
            
        // Creates a cube using hardcoded data
        // Cube VAO
        cubeVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        // Cube VBO
        GlVbo.emptyVboBuilder
        |> GlVbo.withAttrNames ["Positions"; "Normals"; "Texture coords"]
        |> GlVbo.withAttrDefinitions 
            Cube.vertexPositionsAndNormalsAndTextureCoords
        |> GlVbo.build (cubeVao, ctx)
        |> ignore
           
        // Loads shader to draw the light cube
        shaderLightSrc <-
            GlProg.emptyBuilder
            |> GlProg.withName "LightSource"
            |> GlProg.withShaders [
                ShaderType.VertexShader, "Simple3d.vert"
                ShaderType.FragmentShader, "LightSource.frag"
            ]
            |> GlProg.withUniforms [
                "uModel"
                "uView" 
                "uProjection"
                "uEmittedLightColor"
            ]
            |> GlProg.build ctx

        // Comment this or press F10 to unlock the camera
        dispatch (Mouse UseCursorNormal)
        dispatch (Camera Lock)

        let modelsDir = Path.Combine("Resources", "Models")
        let loadTexture path = dispatch (Asset (LoadImageStart path))

        let makeMdlPath pathParts = 
            pathParts
            |> List.append [modelsDir]
            |> fun fullList -> Path.Combine(List.toArray fullList)

        mdlBackpack <- 
            Model.loadU (makeMdlPath ["Backpack"; "backpack.obj"]) ctx loadTexture

        mdlNanosuit <- 
            Model.loadF (makeMdlPath ["Nanosuit"; "nanosuit.obj"]) ctx loadTexture

        mdlCyborg <- 
            Model.loadF (makeMdlPath ["Cyborg"; "cyborg.obj"]) ctx loadTexture

        mdlPlanet <- 
            Model.loadF (makeMdlPath ["Planet"; "planet.obj"]) ctx loadTexture

        mdlRock <- 
            Model.loadU (makeMdlPath ["Rock"; "rock.obj"]) ctx loadTexture

        mdlVampire <- 
            Model.loadF (makeMdlPath ["Vampire"; "dancing_vampire.dae"]) ctx loadTexture

        mdlMannequin <- 
            Model.loadF (makeMdlPath ["Mannequin"; "0.obj"]) ctx loadTexture
        ()

    let onUpdate (ctx: GlWindowCtx) (state) dispatch (DeltaTime deltaTime) =
        (ctx, state, dispatch, deltaTime)
        |> Baseline.updateWindowClosure
        |> Baseline.updateWindowTitle
        |> Baseline.updateWindowResolution
        |> Baseline.updateWindowMode
        |> Baseline.updateCursorMode
        |> Baseline.updateCameraPosition
        |> ignore

        // Binds images as textures on the GL thread so they are available on 
        // render and don't need to be created on the GPU on each render.
        state.Asset.ImagesLoaded
        |> Map.iter (fun imgPath imgAsset ->
            if imgAsset.GlTexture.IsNone then
                let glTexture = GlTex.create2D imgAsset.Image ctx
                dispatch (Asset (BindedImageAsTexture(imgPath, glTexture)))
        )

    let onRender ctx state dispatch (DeltaTime deltaTime) =
        ctx.Gl.Enable GLEnum.DepthTest

        uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
        |> ctx.Gl.Clear
        
        // Sets a dark grey background so the cube´s color changes are visible
        ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
        
        let time = single ctx.Window.Time
        let lightSrcPathRadius = 1.8f
        let lightSrcPathCurrX = MathF.Sin(time / 4.0f) * lightSrcPathRadius
        let lightSrcPathCurrZ = MathF.Cos(time / 4.0f) * lightSrcPathRadius

        let lightSrcPosition = 
            new Vector3(lightSrcPathCurrX, 0.5f, lightSrcPathCurrZ)

        let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

        let res = state.Window.Resolution
        let fov = Radians.value <| toRadians(state.Camera.Fov)
        let ratio = single(res.Width) / single(res.Height)
        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)
       
        let lightCubeScale = 0.05f
        let lightSourceModelMatrix =
            Matrix4x4.CreateTranslation (lightSrcPosition / lightCubeScale)
            * Matrix4x4.CreateScale (new Vector3(lightCubeScale))

        // Draws the moving point light
        (shaderLightSrc, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformM4x4 "uModel" lightSourceModelMatrix
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> GlProg.setUniformV3 "uEmittedLightColor" lightColor
        |> ignore

        (cubeVao, ctx)
        |> GlVao.bind
        |> ignore

        ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

        // Prepares the shader
        (shaderSimple3d, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> GlProg.setUniformV3 "uViewerPos" state.Camera.Position
        |> GlProg.setUniformV3 "uPointLight.position" lightSrcPosition
        |> GlProg.setUniformF "uPointLight.constantComponent" attenuationKc
        |> GlProg.setUniformF "uPointLight.linearComponent" attenuationKl
        |> GlProg.setUniformF "uPointLight.quadraticComponent" attenuationKq
        |> GlProg.setUniformV3 "uPointLight.ambientColor" lightColor
        |> GlProg.setUniformV3 "uPointLight.diffuseColor" lightColor
        |> GlProg.setUniformV3 "uPointLight.specularColor" lightColor
        |> GlProg.setUniformV3 "uDirectionalLight.direction" (new Vector3(0.0f, -1.0f, 0.0f))
        |> GlProg.setUniformV3 "uDirectionalLight.ambientColor" lightAmbientColor
        |> GlProg.setUniformV3 "uDirectionalLight.diffuseColor" lightDiffuseColor
        |> GlProg.setUniformV3 "uDirectionalLight.specularColor" lightSpecularColor
        |> ignore
        
        let getTextureHandler imgPath =
            state.Asset.ImagesLoaded.TryFind imgPath
            |> function
                | Some asset -> 
                    match asset.GlTexture with
                    | Some assetGlTexture -> assetGlTexture
                    | None -> fallbackGlTexture
                | None -> fallbackGlTexture

        // 2pi / 7 models, so they are drawn around the origin.
        let mdlCircleDist = (MathF.PI * 2.0f) / 7.0f
        let mdlCircleRadius = 1.2f

        let getModelMatrix (orderInCircle: int) (scale: float32) yTranslate yRotateRadians =
            let posAngleInCircle = mdlCircleDist * single(orderInCircle)
            let absolutePosX = MathF.Sin(posAngleInCircle) * mdlCircleRadius
            let absolutePosZ = MathF.Cos(posAngleInCircle) * mdlCircleRadius
            let absolutePos = new Vector3(absolutePosX, 0.0f, absolutePosZ)

            Matrix4x4.Identity *
            Matrix4x4.CreateRotationY yRotateRadians *
            Matrix4x4.CreateTranslation (
                (absolutePos/scale) + new Vector3(0.0f, yTranslate/scale, 0.0f)
            ) *
            Matrix4x4.CreateScale(scale)
         
        // Backpack        
        (shaderSimple3d, ctx)
        |> GlProg.setUniformM4x4 "uModel" (getModelMatrix 0 0.15f 0.6f 0.0f)
        |> ignore
        Model.draw mdlBackpack shaderSimple3d getTextureHandler ctx

        // Nanosuit
        (shaderSimple3d, ctx)
        |> GlProg.setUniformM4x4 "uModel" (getModelMatrix 1 0.07f 0.0f (MathF.PI / 4.0f))
        |> ignore
        Model.draw mdlNanosuit shaderSimple3d getTextureHandler ctx

        // Cyborg
        (shaderSimple3d, ctx)
        |> GlProg.setUniformM4x4 "uModel" (getModelMatrix 2 0.3f 0.0f (MathF.PI / 2.0f))
        |> ignore
        Model.draw mdlCyborg shaderSimple3d getTextureHandler ctx

        // Planet
        (shaderSimple3d, ctx)
        |> GlProg.setUniformM4x4 "uModel" (getModelMatrix 3 0.1f 0.5f 0.0f)
        |> ignore
        Model.draw mdlPlanet shaderSimple3d getTextureHandler ctx

        // Rock        
        (shaderSimple3d, ctx)
        |> GlProg.setUniformM4x4 "uModel" (getModelMatrix 4 0.25f 0.2f 0.0f)
        |> ignore
        Model.draw mdlRock shaderSimple3d getTextureHandler ctx

        // Vampire
        (shaderSimple3d, ctx)
        |> GlProg.setUniformM4x4 "uModel" (getModelMatrix 5 0.005f 0.0f (-MathF.PI/1.7f))
        |> ignore
        Model.draw mdlVampire shaderSimple3d getTextureHandler ctx

        // Mannequin
        (shaderSimple3d, ctx)
        |> GlProg.setUniformM4x4 "uModel" (getModelMatrix 6 0.75f 0.0f(-MathF.PI/3.6f))
        |> ignore
        Model.draw mdlMannequin shaderSimple3d getTextureHandler ctx  

        let cameraDistanceFactorBehindLightSrc = 1.4f
        let newCameraPos = 
            new Vector3(
                lightSrcPosition.X * cameraDistanceFactorBehindLightSrc, 
                lightSrcPosition.Y * cameraDistanceFactorBehindLightSrc + 0.3f, 
                lightSrcPosition.Z * cameraDistanceFactorBehindLightSrc)

        // Forces the camera to look slightly below the origin
        let newCameraTarget =
            newCameraPos * 
            new Vector3(-1.0f, 1.0f, -1.0f) + 
            new Vector3(0.0f, -1.8f, 0.0f)

        dispatch (Camera (ForcePosition newCameraPos))
        dispatch (Camera (ForceTarget newCameraTarget))
        dispatch (FpsCounter(FrameRenderCompleted deltaTime))
        ()

    let onInputContextLoaded ctx ic state dispatch = 
        dispatch (Window (InitializeInputContext ic))

    let onWindowResize ctx state dispatch newSize =
        (ctx, state, dispatch, newSize)
        |> Baseline.handleWindowResize
        |> ignore

    let testAddActionListener =
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
        |> addOnActionListener (fun state action dispatch ctx ->
            match action with
            | Asset assetAction ->
                let assetDispatch a = dispatch (Asset a)
                BaseAssetSlice.listen state.Asset assetAction assetDispatch ctx
            | _ -> 
                ()
        )
        |> buildAndRun
    0