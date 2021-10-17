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
open Microsoft.FSharp.NativeInterop
open Serilog
open Serilog.Extensions.Logging

let initialState = 
   BaselineState.createDefault(
      "47_Blinn_Phong", 
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
      (new SerilogLoggerFactory(serilogLogger)).CreateLogger("GlobalCategory");

   // No need to get the current state since this is executed
   // before starting the game loop, so using the initial state 
   // is just fine.
   let glWindowOptions = 
      { GlWindowOptions.Default with
         // The antialiasing sample count is being set here!!!
         MainFramebufferSampleCount = 16us
         IsVsync = false
         Title = initialState.Window.Title
         Logger = Some microsoftLogger
         Size = initialRes }
         
   let mutable vaoPlane = Unchecked.defaultof<_>
   let mutable shader = Unchecked.defaultof<_>

   let mutable woodTexture = Unchecked.defaultof<_>
   let mutable useBlinn = true
   let mutable lightPos = v3 0.0f 0.0f 0.0f

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
      |> ignore

      match key with
      | Key.Number1 -> useBlinn <- false
      | Key.Number2 -> useBlinn <- true
      | Key.Number3 -> lightPos <- lightPos - v3 0.0f 0.1f 0.0f
      | Key.Number4 -> lightPos <- lightPos + v3 0.0f 0.1f 0.0f
      | _ -> ()

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
            
   let onLoad (ctx: GlWindowCtx) input (state: BaselineState) dispatch =
      (ctx, input, state, dispatch)
      |> Baseline.loadImGuiController 
      |> ignore

      shader <-
         GlProg.emptyBuilder
         |> GlProg.withName "3dShader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "shader.vert"
            ShaderType.FragmentShader, "shader.frag" 
         ]
         |> GlProg.withUniforms [
            "uModel"
            "uTexture"
            "uLightPos"
            "uViewerPos"
            "uUseBlinn"
         ]
         |> GlProg.build ctx
           
      ctx
      |> Baseline.bindShaderToUbo shader matricesUboDef state dispatch
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

      let texturesDir = Path.Combine ("Resources", "Textures")
      let woodImg = 
         GlTex.loadImage (Path.Combine (texturesDir, "wood.png")) ctx

      woodTexture <- GlTex.create2d woodImg ctx
        
      dispatch (Camera (ForcePosition (v3 -7.93f 2.79f 3.97f)))
      dispatch (Camera (ForceTarget (v3 0.88f -0.48f -0.35f)))
      
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

   let onRender (ctx: GlWindowCtx) state dispatch (DeltaTime deltaTime) =
      ctx.Gl.Enable GLEnum.DepthTest

      // Needs to be Lequal instead of Less, so that the z-depth trick works
      // for the skybox, and it gets drawn behind everything even though
      // it´s rendered last.
      ctx.Gl.DepthFunc GLEnum.Less

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      |> ctx.Gl.Clear
              
      // Sets a dark grey background so the cube´s color changes are visible
      ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
      
      // **********************************************************************
      // Sets matrices UBO for all shaders
      let res = state.Window.Resolution
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)
      let projectionMatrix = 
         Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)

      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

      ctx
      |> Baseline.setUboUniformM4 
            matricesUboDef "uProjection" projectionMatrix state
      |> Baseline.setUboUniformM4
            matricesUboDef "uView" viewMatrix state
      |> ignore
        
      // **********************************************************************
      (vaoPlane, ctx)
      |> GlTex.setActive GLEnum.Texture0 woodTexture
      |> ignore

      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> GlProg.setUniformV3 "uLightPos" lightPos
      |> GlProg.setUniformV3 "uViewerPos" state.Camera.Position
      |> GlProg.setUniformB "uUseBlinn" useBlinn
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      GlVao.bind (vaoPlane, ctx)
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)

      (ctx, state, dispatch, deltaTime)
      |> Baseline.renderGui
      |> ignore

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