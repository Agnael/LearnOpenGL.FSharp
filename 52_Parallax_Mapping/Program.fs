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
      "52_Parallax_Mapping", 
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
         
   let lightPos = v3 0.5f 1.0f 0.3f
   let camNearPlane = 0.1f
   let camFarPlane = 100.0f

   let shadowMapWidth = 1024
   let shadowMapHeight = 1024
   let shadowNearPlane = 1.0f
   let shadowFarPlane = 25.0f

   let mutable vaoQuad = Unchecked.defaultof<_>

   let mutable shader = Unchecked.defaultof<_>

   // Images
   let mutable bricksAlbedoImg = Unchecked.defaultof<_>
   let mutable bricksNormalsImg = Unchecked.defaultof<_>
   let mutable bricksDisplacementImg = Unchecked.defaultof<_>

   let mutable woodAlbedoImg = Unchecked.defaultof<_>
   let mutable woodNormalsImg = Unchecked.defaultof<_>
   let mutable woodDisplacementImg = Unchecked.defaultof<_>
   
   // Textures
   let mutable bricksTexture = Unchecked.defaultof<_>
   let mutable bricksNormalsTexture = Unchecked.defaultof<_>
   let mutable bricksDisplacementTexture = Unchecked.defaultof<_>
   
   let mutable woodTexture = Unchecked.defaultof<_>
   let mutable woodNormalsTexture = Unchecked.defaultof<_>
   let mutable woodDisplacementTexture = Unchecked.defaultof<_>

   let mutable heightScale = 0.05f
   let mutable isHeightScaleIncreasing = false
   let mutable isHeightScaleDecreasing = false
      
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

      match key with
      | Key.Q ->
         isHeightScaleDecreasing <- true
      | Key.E ->
         isHeightScaleIncreasing <- true
      | _ -> ()

   let onKeyUp ctx state dispatch kb key =         
      (ctx, state, dispatch, kb, key)    
      |> Baseline.detectCameraMovementStop
      |> Baseline.detectHideFullInfo // F1
      |> ignore

      match key with
      | Key.Q ->
         isHeightScaleDecreasing <- false
      | Key.E ->
         isHeightScaleIncreasing <- false
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

      addAlwaysVisibleControlInstruction "Decrease height scale" [Single (KeyboardKey Key.Q)]
      addAlwaysVisibleControlInstruction "Increase height scale" [Single (KeyboardKey Key.E)]

      shader <-
         GlProg.emptyBuilder
         |> GlProg.withName "Shader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "shader.vert"
            ShaderType.FragmentShader, "shader.frag" 
         ]
         |> GlProg.withUniforms [
            "uModel"
            "uLightPosition"
            "uTexture"
            "uShadowMap"
            "uViewerPos"
            "uNormalMap"
            "uDisplacementMap"
            "uHeightScale"
         ]
         |> GlProg.build ctx
               
      ctx
      |> Baseline.bindShaderToUbo shader matricesUboDef state dispatch
      |> ignore
                 
      // Comment this or press F10 to unlock the camera
      dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)
      
      vaoQuad <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Normals"; "TexCoords"; "Tangents"]
      |> GlVbo.withAttrDefinitions Quad.vertexPositionsAndNormalsAndTextureCoordsAndTangentsBitangents
      |> GlVbo.build (vaoQuad, ctx)
      |> ignore

      let texturesDir = Path.Combine ("Resources", "Textures")

      bricksAlbedoImg <- GlTex.loadImage (Path.Combine (texturesDir, "bricks2.jpg")) ctx
      bricksNormalsImg <- GlTex.loadImage (Path.Combine (texturesDir, "bricks2_normal.jpg")) ctx
      bricksDisplacementImg <- GlTex.loadImage (Path.Combine (texturesDir, "bricks2_disp.jpg")) ctx

      woodAlbedoImg <- GlTex.loadImage (Path.Combine (texturesDir, "wood.png")) ctx
      woodNormalsImg <- GlTex.loadImage (Path.Combine (texturesDir, "toy_box_normal.png")) ctx
      woodDisplacementImg <- GlTex.loadImage (Path.Combine (texturesDir, "toy_box_disp.png")) ctx

      bricksTexture <- GlTex.create2d bricksAlbedoImg ctx
      bricksNormalsTexture <- GlTex.create2d bricksNormalsImg ctx
      bricksDisplacementTexture <- GlTex.create2d bricksDisplacementImg ctx

      woodTexture <- GlTex.create2d woodAlbedoImg ctx
      woodNormalsTexture <- GlTex.create2d woodNormalsImg ctx
      woodDisplacementTexture <- GlTex.create2d woodDisplacementImg ctx

      dispatch (Camera (ForcePosition (v3 0.93f 1.31f 0.59f)))
      dispatch (Camera (ForceTarget (v3 -0.55f -0.71f -0.44f)))
      
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
      
      if isHeightScaleIncreasing && heightScale > 0.0f then
         heightScale <- heightScale - 0.0005f

      if isHeightScaleDecreasing && heightScale < 1.0f then
         heightScale <- heightScale + 0.0005f

   let renderScene state shader (ctx: GlWindowCtx) =
      //ctx.Gl.FrontFace GLEnum.Ccw
      //glCullFace ctx CullFaceMode.FrontAndBack

      let brickModelMatrix =
         Matrix4x4.Identity *
         Matrix4x4.CreateScale 0.5f *
         Matrix4x4.CreateRotationY 0.5f * 
         Matrix4x4.CreateTranslation (v3 -0.5f 0.0f 0.0f)

      let toyBoxModelMatrix =
         Matrix4x4.Identity *
         Matrix4x4.CreateScale 0.5f *
         Matrix4x4.CreateRotationY -0.5f* 
         Matrix4x4.CreateTranslation (v3 0.5f 0.0f 0.0f)

      // Brick wall
      (vaoQuad, ctx)
      |> GlTex.setActive GLEnum.Texture0 bricksTexture
      |> GlTex.setActive GLEnum.Texture1 bricksNormalsTexture
      |> GlTex.setActive GLEnum.Texture2 bricksDisplacementTexture
      |> ignore

      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniform "uModel" brickModelMatrix
      |> ignore
      
      GlVao.bind (vaoQuad, ctx) |> ignore
      glDrawArrays ctx PrimitiveType.Triangles 0 6u

      // Toy box
      (vaoQuad, ctx)
      |> GlTex.setActive GLEnum.Texture0 woodTexture
      |> GlTex.setActive GLEnum.Texture1 woodNormalsTexture
      |> GlTex.setActive GLEnum.Texture2 woodDisplacementTexture
      |> ignore

      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniform "uModel" toyBoxModelMatrix
      |> ignore
      
      GlVao.bind (vaoQuad, ctx) |> ignore
      glDrawArrays ctx PrimitiveType.Triangles 0 6u

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
      glViewport res.Width res.Height ctx
      glClear (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
        
      // - Renders the scene again using the shadow map
      // The quad vao has nothing to do with this. It´s just necessary to
      // satisfy this poorly designed function.
      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniform "uViewerPos" state.Camera.Position
      |> GlProg.setUniform "uLightPosition" lightPos
      |> GlProg.setUniform "uTexture" 0
      |> GlProg.setUniform "uNormalMap" 1
      |> GlProg.setUniform "uDisplacementMap" 2
      |> GlProg.setUniform "uHeightScale" heightScale
      |> ignore

      renderScene state shader ctx

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