open Silk.NET.Windowing.Glfw
open Silk.NET.GLFW


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
      "46_Antialiasing", 
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
         
   let mutable vaoCube = Unchecked.defaultof<_>
   let mutable shader = Unchecked.defaultof<_>

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
      shader <-
         GlProg.emptyBuilder
         |> GlProg.withName "3dShader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "shader.vert"
            ShaderType.FragmentShader, "shader.frag" 
         ]
         |> GlProg.withUniforms ["uModel"]
         |> GlProg.build ctx
           
      ctx
      |> Baseline.bindShaderToUbo shader matricesUboDef state dispatch
      |> ignore
                 
      // Comment this or press F10 to unlock the camera
      dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)
      
      vaoCube <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"]
      |> GlVbo.withAttrDefinitions 
         CubeCCW.vertexPositions
      |> GlVbo.build (vaoCube, ctx)
      |> ignore
         
      dispatch (Camera (ForcePosition (v3 0.41f -0.44f -0.62f)))
      dispatch (Camera (ForceTarget (v3 0.48f -0.39f 0.78f)))

      dispatch (Camera Lock)
      dispatch (Mouse UseCursorRaw)
      
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

      let setUboUniformM4 =  Baseline.setUboUniformM4 state ctx
      setUboUniformM4 matricesUboDef "uProjection" projectionMatrix
      setUboUniformM4 matricesUboDef "uView" viewMatrix
        
      // **********************************************************************
      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> ignore

      GlVao.bind (vaoCube, ctx)
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)

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
   |> addActionInterceptor onActionIntercepted
   |> buildAndRun
   0