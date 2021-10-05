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
      "29_Visualizing_The_Depth_Buffer", 
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
   let mutable planeVao = Unchecked.defaultof<_>
   let mutable shaderDepthBuffer = Unchecked.defaultof<_>
   let mutable cubeTexture = Unchecked.defaultof<_>
   let mutable floorTexture = Unchecked.defaultof<_>
            
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
      shaderDepthBuffer <-
         GlProg.emptyBuilder
         |> GlProg.withName "Lighted"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"DepthBuffer.vert"
               ; ShaderType.FragmentShader, @"DepthBuffer.frag" 
               ;]
         |> GlProg.withUniforms [
               "uModel"
               "uView"
               "uProjection"
               "uTexture"
         ]
         |> GlProg.build ctx

      // CUBE
      cubeVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      let asd =
         Cube.vertexPositionsAndTextureCoords

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
      |> GlVbo.withAttrDefinitions 
         Cube.vertexPositionsAndTextureCoords
      |> GlVbo.build (cubeVao, ctx)
      |> ignore

      cubeTexture <- 
         GlTex.loadImage "marble.jpg" ctx
         |> fun img -> GlTex.create2d img ctx

      // PLANE
      planeVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao
            
      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
      |> GlVbo.withAttrDefinitions 
         Plane.vertexPositionsAndTextureCoords
      |> GlVbo.build (planeVao, ctx)
      |> ignore

      floorTexture <- 
         GlTex.loadImage "metal.png" ctx
         |> fun img -> GlTex.create2d img ctx
                                            
      // Hardcoded camera position and target, so it looks just like the
      // LearnOpenGL.com example right away.
      dispatch (Camera (ForcePosition (new Vector3(-6.31f, 1.96f, 6.37f))))
      dispatch (Camera (ForceTarget (new Vector3(0.65f, -0.32f, -0.68f))))

      // Comment this or press F10 to unlock the camera
      //dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)

   let onUpdate (ctx: GlWindowCtx) (state) dispatch (DeltaTime deltaTime) =
      (ctx, state, dispatch, deltaTime)
      |> Baseline.updateWindowClosure
      |> Baseline.updateWindowTitle
      |> Baseline.updateWindowResolution
      |> Baseline.updateWindowMode
      |> Baseline.updateCursorMode
      |> Baseline.updateCameraPosition
      |> ignore

   let onRender (ctx: GlWindowCtx) state dispatch (DeltaTime deltaTime) =
      ctx.Gl.Enable GLEnum.DepthTest
      ctx.Gl.DepthFunc GLEnum.Less

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      |> ctx.Gl.Clear
        
      // Sets a dark grey background so the cube´s color changes are visible
      ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
        
      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

      let res = state.Window.Resolution
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)
      let projectionMatrix = 
         Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)
       
      // Prepares the shader
      (shaderDepthBuffer, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> ignore

      // CUBES PREP
      GlVao.bind (cubeVao, ctx) |> ignore

      (cubeVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 cubeTexture
      |> ignore        

      // CUBE 1
      let cube1_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(new Vector3(-1.0f, 0.0f, -1.0f))

      (shaderDepthBuffer, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube1_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)
        
      // CUBE 2
      let cube2_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(new Vector3(2.0f, 0.0f, 0.0f))

      (shaderDepthBuffer, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube2_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

      // PLANE
      GlVao.bind (planeVao, ctx) |> ignore
        
      (planeVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 floorTexture
      |> ignore
        
      (shaderDepthBuffer, ctx)
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 6ul)

      dispatch (FpsCounter(FrameRenderCompleted deltaTime))
      ()

   let onInputContextLoaded ctx ic state dispatch = 
      dispatch (Window (InitializeInputContext ic))

   let onWindowResize ctx state dispatch newSize =
      (ctx, state, dispatch, newSize)
      |> Baseline.handleWindowResize
      |> ignore

   // TODO: How did this return value come to exist?
   let addListener =
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