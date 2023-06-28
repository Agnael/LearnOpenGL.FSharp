open Silk.NET.Windowing.Glfw
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
      "51_Normal_Mapping", 
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
   let mutable modelShader = Unchecked.defaultof<_>
   
   let mutable shadowCubemapFbo = Unchecked.defaultof<_>

   let mutable brickwallImg = Unchecked.defaultof<_>
   let mutable brickwallNormalsImg = Unchecked.defaultof<_>
   
   let mutable brickwallTexture = Unchecked.defaultof<_>
   let mutable brickwallNormalsTexture = Unchecked.defaultof<_>

   let mutable useNormalMapping = true
   
   let mutable fallbackGlTexture = Unchecked.defaultof<_>
   let mutable mdlCyborg = Unchecked.defaultof<_>
   
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

      match key with
      | Key.E -> useNormalMapping <- not useNormalMapping
      | _ -> ()

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

      addAlwaysVisibleControlInstruction 
         "Toggle normal mapping"
         [Single <| KeyboardKey Key.E]
         
      let fallbackImagePath = Path.Combine("Resources", "Textures", "fallback.jpg")

      fallbackGlTexture <-
         GlTex.loadImage fallbackImagePath ctx
         |> fun img -> GlTex.create2d img ctx

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
            "uUseNormalMapping"
            "uNormalMap"
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
            "uTexture"
            "uNormalMap"
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

      modelShader <-
         GlProg.emptyBuilder
         |> GlProg.withName "Simple3D"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, "model.vert"
               ; ShaderType.FragmentShader, "model.frag" 
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

      ctx
      |> Baseline.bindShaderToUbo shader matricesUboDef state dispatch
      |> Baseline.bindShaderToUbo cubemapDepthShader matricesUboDef state dispatch
      |> Baseline.bindShaderToUbo lightSourceShader matricesUboDef state dispatch
      |> ignore
                 
      // Comment this or press F10 to unlock the camera
      dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)

      let modelsDir = Path.Combine("Resources", "Models")
      let loadTexture path = dispatch (Asset (LoadImageStart path))

      let makeMdlPath pathParts = 
         pathParts
         |> List.append [modelsDir]
         |> fun fullList -> Path.Combine(List.toArray fullList)

      mdlCyborg <- 
         Model.loadWithTangentsBitangentsF (makeMdlPath ["Cyborg"; "cyborg.obj"]) ctx loadTexture
      
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
      |> GlVbo.withAttrNames ["Positions"; "Normals"; "TexCoords"; "Tangents"; "Bitangents"]
      |> GlVbo.withAttrDefinitions CubeCCW.vertexPositionsAndNormalsAndTextureCoordsAndTangentsBitangents
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
      brickwallImg <- GlTex.loadImage (Path.Combine (texturesDir, "brickwall.jpg")) ctx
      brickwallNormalsImg <- GlTex.loadImage (Path.Combine (texturesDir, "brickwall_normal.jpg")) ctx

      brickwallTexture <- GlTex.create2d brickwallImg ctx
      brickwallNormalsTexture <- GlTex.create2d brickwallNormalsImg ctx

      dispatch (Camera (ForcePosition (v3 -0.4f -0.32f 1.61f)))
      dispatch (Camera (ForceTarget (v3 -0.59f 0.16f -0.79f)))
      
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

   let renderCyborg state shader ctx = 
      glCullFace ctx CullFaceMode.Front

      let getTextureHandler imgPath =
         state.Asset.ImagesLoaded.TryFind imgPath
         |> function
               | Some asset -> 
                  match asset.GlTexture with
                  | Some assetGlTexture -> assetGlTexture
                  | None -> fallbackGlTexture
               | None -> fallbackGlTexture

      let cyborgMdlModelMatrix =
         Matrix4x4.Identity *
         Matrix4x4.CreateScale (0.7f) *
         Matrix4x4.CreateRotationY (1.67f) *
         Matrix4x4.CreateTranslation(-1.00f, -2.5f, 1.00f)

      (shader, ctx)
      |> GlProg.setUniformM4x4 "uModel" cyborgMdlModelMatrix
      |> ignore

      Model.drawWithUniformNames
         mdlCyborg 
         shader
         (Some "uTexture")
         None
         (Some "uNormalMap")
         None
         getTextureHandler 
         ctx

      glCullFace ctx CullFaceMode.Back

   let makeCubeTransform (rotation: Vector3) (scale: single) (pos: Vector3) =
      Matrix4x4.Identity *
      Matrix4x4.CreateScale scale *
      Matrix4x4.CreateRotationX rotation.X *
      Matrix4x4.CreateRotationY rotation.Y *
      Matrix4x4.CreateRotationZ rotation.Z *
      Matrix4x4.CreateTranslation pos 

   let renderScene state shader ctx =      
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
      
      renderCyborg state shader ctx

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

      let lightProjectionMatrix = 
         createPerspectiveFov lightFovRadians shadowScreenRatio shadowNearPlane shadowFarPlane

      let makeLightSpaceMatrix camTargetOffset camUpDir =
         ((createLookAt lightPos (lightPos + camTargetOffset) camUpDir)) * lightProjectionMatrix

      let lightSpaceMatrices = [
         makeLightSpaceMatrix (v3i  1  0  0) (v3i 0 -1  0)   // Right
         makeLightSpaceMatrix (v3i -1  0  0) (v3i 0 -1  0)   // Left
         makeLightSpaceMatrix (v3i  0  1  0) (v3i 0  0  1)   // Top
         makeLightSpaceMatrix (v3i  0 -1  0) (v3i 0  0 -1)   // Bottom
         makeLightSpaceMatrix (v3i  0  0  1) (v3i 0 -1  0)   // Near
         makeLightSpaceMatrix (v3i  0  0 -1) (v3i 0 -1  0)   // Far
      ]

      (cubemapDepthShader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniforms "uLightSpaceMatrices" lightSpaceMatrices
      |> GlProg.setUniformV3 "uLightPosition" lightPos
      |> GlProg.setUniformF "uFarPlane" shadowFarPlane
      |> ignore

      renderScene state cubemapDepthShader ctx

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
      |> GlTex.setActive GLEnum.Texture1 brickwallTexture
      |> GlTex.setActive GLEnum.Texture2 brickwallNormalsTexture
      |> ignore

      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniform "uFarPlane" shadowFarPlane
      |> GlProg.setUniform "uViewerPos" state.Camera.Position
      |> GlProg.setUniform "uLightPosition" lightPos
      |> GlProg.setUniform "uShadowMap" 0
      |> GlProg.setUniform "uTexture" 1
      |> GlProg.setUniform "uNormalMap" 2
      |> GlProg.setUniform "uUseNormalMapping" useNormalMapping
      |> ignore

      renderScene state shader ctx
      
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