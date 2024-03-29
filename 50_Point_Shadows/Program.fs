﻿open Silk.NET.Windowing.Glfw
open Silk.NET.GLFW
open Silk.NET.OpenGL.Extensions.ImGui
open Silk.NET.Input

#nowarn "9"
#nowarn "51"
open System
open Galante.OpenGL
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics
open BaseCameraSlice
open BaseMouseSlice
open BaseFpsCounterSlice
open BaseAssetSlice
open BaseWindowSlice
open GalanteMath
open BaselineState
open Game
open Galante
open System.IO
open GlTex
open GlFbo
open Model
open BaseGlSlice
open Gl
open Quad
open BaseGuiSlice
open Microsoft.FSharp.NativeInterop
open Serilog
open Serilog.Extensions.Logging

let initialState = 
   BaselineState.createDefault(
      "50_Shadow_Mapping", 
      new Size(640, 360))

let initialRes = initialState.Window.Resolution

let v3toFloatArray (v3: Vector3): single array = 
   [| v3.X; v3.Y; v3.X |]

let v3arrayToFloatArrayArray (v3array: Vector3 array) =
   v3array
   |> Array.map (fun x -> [| v3toFloatArray x |])
   
[<EntryPoint>]
let main argv =
   let serilogLogger = 
      (new LoggerConfiguration())
         .Enrich.FromLogContext()
         .MinimumLevel.Verbose()
         .WriteTo.Console()
         .CreateLogger();

   let microsoftLogger = 
      (new SerilogLoggerFactory(serilogLogger)).CreateLogger("GlobalCategory")
         
   // No need to get the current state since this is executed
   // before starting the game loop, so using the initial state 
   // is just fine.
   let glWindowOptions = 
      { GlWindowOptions.Default with
         // The antialiasing sample count is being set here!!!
         MainFramebufferSampleCount = 16us
         IsVsync = true
         Title = initialState.Window.Title
         Logger = Some microsoftLogger
         Size = initialRes }
         
   let lightPos = v3 0.0f 0.0f 0.0f
   let camNearPlane = 0.1f
   let camFarPlane = 100.0f

   let shadowMapWidth = 1024
   let shadowMapHeight = 1024
   let shadowNearPlane = 1.0f
   let shadowFarPlane = 25.0f

   let mutable vaoPlane = Unchecked.defaultof<_>
   let mutable vaoCube = Unchecked.defaultof<_>
   let mutable vaoQuad = Unchecked.defaultof<_>
   let mutable shader = Unchecked.defaultof<_>
   let mutable cubemapDepthShader = Unchecked.defaultof<_>
   let mutable lightSourceShader = Unchecked.defaultof<_>
   //let mutable debugQuadShader = Unchecked.defaultof<_>

   let mutable shadowCubemapFbo = Unchecked.defaultof<_>
   let mutable containerImg = Unchecked.defaultof<_>
   let mutable containerTexture = Unchecked.defaultof<_>
   
   let matricesUboDef: GlUniformBlockDefinition = {
      Name = "Matrices"
      UniformNames = [
         "uProjection"
         "uView"
      ]
   }

   let onKeyDown ctx state dispatch kb key =
      let initResW = initialState.Window.Resolution.Width
      let initResH = initialState.Window.Resolution.Height

      (ctx, state, dispatch, kb, key)        
      |> Baseline.detectFullScreenSwitch // ALT+ENTER        
      |> Baseline.detectGameClosing // ESC        
      |> Baseline.detectCameraMovementStart // W|A|S|D|Left_Shift|Space
      |> Baseline.detectResolutionChange initResW initResH // F5|F6|F7
      |> Baseline.detectCursorModeChange // F9|F10
      |> Baseline.detectShowFullInfo // F1
      |> ignore

   let onKeyUp ctx state dispatch kb key =         
      (ctx, state, dispatch, kb, key)    
      |> Baseline.detectCameraMovementStop
      |> Baseline.detectHideFullInfo // F1
      |> ignore

   let onMouseMove ctx state dispatch newPos = 
      (ctx, state, dispatch, newPos)
      |> Baseline.handleCameraAngularChange
      |> ignore
                
   let onMouseWheel ctx state dispatch (newPos: Vector2) =
      (ctx, state, dispatch, newPos)
      |> Baseline.handleCameraZoom
      |> ignore
            
   let onLoad (ctx: GlWindowCtx) input (state: BaselineState) dispatch =

      (ctx, input, state, dispatch)
      |> Baseline.loadImGuiController 
      |> Baseline.loadBaseGuiElements
      |> ignore

      let res = state.Window.Resolution
      
      // Add extra controls, specific to this exercise
      let dispatchGui guiAction = dispatch (Gui guiAction)
      let addAlwaysVisibleControlInstruction explanation controls =
         dispatchGui (
            AddAlwaysVisibleControlInstruction (
               { Explanation = explanation; Controls = controls }
            )
         )

      shader <-
         GlProg.emptyBuilder
         |> GlProg.withName "Shader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "shader.vert"
            ShaderType.FragmentShader, "shader.frag" 
         ]
         |> GlProg.withUniforms [
            "uModel"
            "uFarPlane"
            "uView"
            "uProjection"
            "uUseReverseNormals"
            "uLightPosition"
            "uTexture"
            "uShadowMap"
            "uViewerPos"
         ]
         |> GlProg.build ctx
       
      cubemapDepthShader <-
         GlProg.emptyBuilder
         |> GlProg.withName "CubemapDepthShader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "cubemapDepthShader.vert"
            ShaderType.FragmentShader, "cubemapDepthShader.frag" 
            ShaderType.GeometryShader, "cubemapDepthShader.geom" 
         ]
         |> GlProg.withUniforms [
            "uModel"
            "uLightPosition"
            "uFarPlane"
            "uUseReverseNormals"
            "uLightSpaceMatrices[6]"
         ]
         |> GlProg.build ctx

      lightSourceShader <-
         GlProg.emptyBuilder
         |> GlProg.withName "LightSourceShader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "lightSource.vert"
            ShaderType.FragmentShader, "lightSource.frag"
         ]
         |> GlProg.withUniforms [
            "uModel"
         ]
         |> GlProg.build ctx

      ctx
      |> Baseline.bindShaderToUbo shader matricesUboDef state dispatch
      |> Baseline.bindShaderToUbo cubemapDepthShader matricesUboDef state dispatch
      |> Baseline.bindShaderToUbo lightSourceShader matricesUboDef state dispatch
      |> ignore
                 
      // Comment this or press F10 to unlock the camera
      dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)
      
      vaoPlane <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Normals"; "TexCoords"]
      |> GlVbo.withAttrDefinitions 
            Plane.vertexPositionsAndNormalsAndTextureCoords
      |> GlVbo.build (vaoPlane, ctx)
      |> ignore
      
      vaoCube <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Normals"; "TexCoords"]
      |> GlVbo.withAttrDefinitions 
            CubeCCW.vertexPositionsAndNormalsAndTextureCoords
      |> GlVbo.build (vaoCube, ctx)
      |> ignore

      vaoQuad <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "TexCoords"]
      |> GlVbo.withAttrDefinitions Quad.vertexPositionsAndTextureCoords
      |> GlVbo.build (vaoQuad, ctx)
      |> ignore

      shadowCubemapFbo <- 
         fboCreateWithDepthTextureOnlyCubemap shadowMapWidth shadowMapHeight ctx
         |> fun (fbo, _) -> fbo

      let texturesDir = Path.Combine ("Resources", "Textures")
      containerImg <- GlTex.loadImage (Path.Combine (texturesDir, "container.jpg")) ctx

      containerTexture <- GlTex.create2d containerImg ctx

      dispatch (Camera (ForcePosition (v3 -1.33f 1.42f 4.36f)))
      dispatch (Camera (ForceTarget (v3 0.61f -0.23f -0.76f)))
      
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
      |> Baseline.glCreateQueuedTextures
      |> ignore

   let renderCube shader ctx modelMatrix =
      GlVao.bind (vaoCube, ctx) |> ignore   
      GlProg.setUniform "uModel" modelMatrix (shader, ctx) |> ignore
      glDrawArrays ctx PrimitiveType.Triangles 0 36u

   let makeCubeTransform (rotation: Vector3) (scale: single) (pos: Vector3) =
      Matrix4x4.Identity *
      Matrix4x4.CreateScale scale *
      Matrix4x4.CreateRotationX rotation.X *
      Matrix4x4.CreateRotationY rotation.Y *
      Matrix4x4.CreateRotationZ rotation.Z *
      Matrix4x4.CreateTranslation pos 

   let renderScene shader ctx =      
      let renderCube = renderCube shader ctx

      // Room cube
      // Note that we disable culling here since we render the inside of the cube instead
      // of the usual outside, which throws off normal culling methods.      
      glDisable ctx EnableCap.CullFace
      GlProg.setUniformB "uUseReverseNormals" true (shader, ctx) |> ignore
      renderCube <| makeCubeTransform (v3i 0 0 0) 10.0f (v3 0.0f 0.0f 0.0f)
      GlProg.setUniformB "uUseReverseNormals" false (shader, ctx) |> ignore
      glEnable EnableCap.CullFace ctx |> ignore

      // Internal cubes
      renderCube <| makeCubeTransform (v3i 0 0 0) 0.50f (v3 4.0f -3.5f 0.0f)
      renderCube <| makeCubeTransform (v3i 0 0 0) 0.75f (v3 2.0f 3.0f 1.0f)
      renderCube <| makeCubeTransform (v3i 0 0 0) 0.5f (v3 -3.0f -1.0f 0.0f)
      renderCube <| makeCubeTransform (v3i 0 0 0) 0.5f (v3 -1.5f 1.0f 1.5f)
      renderCube <| makeCubeTransform (v3i 1 0 1) 0.75f (v3 -1.5f 2.0f -3.0f)

   let onRender (ctx: GlWindowCtx) state dispatch (DeltaTime deltaTime) =
      let res = state.Window.Resolution

      let glViewport = glViewport 0 0
      let glClear = glClear ctx
      let glEnable flag = glEnable flag ctx |> ignore
      let glClearColor = glClearColor ctx
      
      // - Sets matrices UBO for all shaders
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)
      let projectionMatrix = createPerspectiveFov fov ratio camNearPlane camFarPlane

      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera
      
      let setUboUniformM4 = Baseline.setUboUniformM4 state ctx
      setUboUniformM4 matricesUboDef "uProjection" projectionMatrix
      setUboUniformM4 matricesUboDef "uView" viewMatrix

      fboBindDefault ctx |> ignore
      glEnable EnableCap.DepthTest
      glClear (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      glClearColor 0.1f 0.1f 0.1f 1.0f
      
      // **********************************************************************
      // 1. Renders the scene into the depth texture, with the light's POV.
      fboBind (shadowCubemapFbo, ctx) |> ignore
      glEnable EnableCap.DepthTest
      glClear GLEnum.DepthBufferBit
      glViewport shadowMapWidth shadowMapHeight ctx
            
      let shadowScreenRatio: float32 = float32(shadowMapWidth / shadowMapHeight)
      let lightFovRadians = Radians.value(toRadians (Degrees.make 90.0f))

      let lightProjectionMatrix = createPerspectiveFov lightFovRadians shadowScreenRatio shadowNearPlane shadowFarPlane

      let lightViewMatrix_right = createLookAt lightPos (lightPos + (v3i 1 0 0)) (v3i 0 -1 0)
      let lightViewMatrix_left = createLookAt lightPos (lightPos + (v3i -1 0 0)) (v3i 0 -1 0)
      let lightViewMatrix_top = createLookAt lightPos (lightPos + (v3i 0 1 0)) (v3i 0 0 1)
      let lightViewMatrix_bottom = createLookAt lightPos (lightPos + (v3i 0 -1 0)) (v3i 0 0 -1)
      let lightViewMatrix_near = createLookAt lightPos (lightPos + (v3i 0 0 1)) (v3i 0 -1 0)
      let lightViewMatrix_far = createLookAt lightPos (lightPos + (v3i 0 0 -1)) (v3i 0 -1 0)

      // I have NO idea why this multiplication MUST be done in the
      // inverse order (relative to how it´s done in the shader),
      // but it doesn´t work otherwise.
      let lightSpaceMatrix_right = lightViewMatrix_right * lightProjectionMatrix
      let lightSpaceMatrix_left = lightViewMatrix_left * lightProjectionMatrix
      let lightSpaceMatrix_top = lightViewMatrix_top  * lightProjectionMatrix
      let lightSpaceMatrix_bottom = lightViewMatrix_bottom  * lightProjectionMatrix
      let lightSpaceMatrix_near = lightViewMatrix_near * lightProjectionMatrix
      let lightSpaceMatrix_far = lightViewMatrix_far  * lightProjectionMatrix

      let makeLightSpaceMatrix camTargetOffset camUpDir =
         ((createLookAt lightPos (lightPos + camTargetOffset) camUpDir)) * lightProjectionMatrix

      //let lightSpaceMatrices = [
      //   makeLightSpaceMatrix (v3i  1  0  0) (v3i 0 -1  0)   // Right
      //   makeLightSpaceMatrix (v3i -1  0  0) (v3i 0 -1  0)   // Left
      //   makeLightSpaceMatrix (v3i  0  1  0) (v3i 0  0  1)   // Top
      //   makeLightSpaceMatrix (v3i  0 -1  0) (v3i 0  0 -1)   // Bottom
      //   makeLightSpaceMatrix (v3i  0  0  1) (v3i 0 -1  0)   // Near
      //   makeLightSpaceMatrix (v3i  0  0 -1) (v3i 0 -1  0)   // Far
      //]

      let lightSpaceMatrices = [
         lightSpaceMatrix_right
         lightSpaceMatrix_left
         lightSpaceMatrix_top
         lightSpaceMatrix_bottom
         lightSpaceMatrix_near
         lightSpaceMatrix_far
      ]
      
      (cubemapDepthShader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniforms "uLightSpaceMatrices" lightSpaceMatrices
      |> GlProg.setUniformV3 "uLightPosition" lightPos
      |> GlProg.setUniformF "uFarPlane" shadowFarPlane
      |> ignore

      renderScene cubemapDepthShader ctx

      // **********************************************************************
      // 2. Renders the normal scene, but using the generated shadow map
      // - Resets viewport, flags and framebuffer
      fboBindDefault ctx |> ignore
      glViewport res.Width res.Height ctx
      glClear (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
        
      // - Renders the scene again using the shadow map
      let shadowMapTexture: GlTexture =
         match shadowCubemapFbo.DepthTexture with
         | None -> failwith "Shadow map not in FBO."
         | Some handle -> handle

      // The plane vao has nothing to do with this. It´s just necessary to
      // satisfy this poorly designed function.
      (vaoPlane, ctx)
      |> GlTex.setActiveCubemap GLEnum.Texture0 shadowMapTexture
      |> GlTex.setActive GLEnum.Texture1 containerTexture
      |> ignore

      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniform "uFarPlane" shadowFarPlane
      |> GlProg.setUniform "uViewerPos" state.Camera.Position
      |> GlProg.setUniform "uLightPosition" lightPos
      |> GlProg.setUniform "uShadowMap" 0
      |> GlProg.setUniform "uTexture" 1
      |> ignore

      renderScene shader ctx
      
      // **********************************************************************
      // Renders light source
      GlProg.setAsCurrent (lightSourceShader, ctx) |> ignore
      renderCube lightSourceShader ctx <| makeCubeTransform (v3i 0 0 0) 0.1f lightPos

      // ImGui
      glViewport res.Width res.Height ctx
      Baseline.renderGui (ctx, state, dispatch, deltaTime) |> ignore

      // **********************************************************************
      // Frame completed
      dispatch (FpsCounter(FrameRenderCompleted deltaTime))
      ()

   let onInputContextLoaded ctx ic state dispatch = 
      dispatch (Window (InitializeInputContext ic))

   let onWindowResize ctx state dispatch newSize =
      (ctx, state, dispatch, newSize)
      |> Baseline.handleWindowResize
      |> ignore

   let onWindowClose ctx state dispatch =
      (ctx, state, dispatch)
      |> Baseline.disposeGui
      |> ignore

   let onActionIntercepted state action dispatch ctx =
      Baseline.handleInterceptedAction state action dispatch ctx

      match action with
      | Gl glAction -> 
         let glDispatch a = dispatch (Gl a)
         BaseGlSlice.listen state.Gl glAction glDispatch ctx
      | _ -> ()

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
   |> addOnWindowClose onWindowClose
   |> addActionInterceptor onActionIntercepted
   |> buildAndRun
   0