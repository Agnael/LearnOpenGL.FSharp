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
open Model

let initialState = 
   BaselineState.createDefault(
      "37_Reflection", 
      new Size(640, 360))

let initialRes = initialState.Window.Resolution

let v3toFloatArray (v3: Vector3): single array = 
   [| v3.X; v3.Y; v3.X |]

let v3arrayToFloatArrayArray (v3array: Vector3 array) =
   v3array
   |> Array.map (fun x -> [| v3toFloatArray x |])

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
   let mutable shaderSimple = Unchecked.defaultof<_>
   let mutable cubeTexture = Unchecked.defaultof<_>
            
   let mutable shaderSkybox = Unchecked.defaultof<_>
   let mutable skyboxVao = Unchecked.defaultof<_>
   let mutable skyboxCubemap = Unchecked.defaultof<_>

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
      shaderSimple <-
         GlProg.emptyBuilder
         |> GlProg.withName "Reflective"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Reflective.vert"
               ; ShaderType.FragmentShader, @"Reflective.frag" 
               ;]
         |> GlProg.withUniforms [
               "uModel"
               "uView"
               "uProjection"
               "uAmbientCubemap"
               "uCameraPosition"
         ]
         |> GlProg.build ctx
            
      // CUBE
      cubeVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Normals"]
      |> GlVbo.withAttrDefinitions 
         Cube.vertexPositionsAndNormals
      |> GlVbo.build (cubeVao, ctx)
      |> ignore
                                                     
      // Hardcoded camera position and target, so it looks just like the
      // LearnOpenGL.com example right away.
      dispatch (Camera (ForcePosition (new Vector3(2.12f, 1.16f, -3.46f))))
      dispatch (Camera (ForceTarget (new Vector3(-0.36f, -0.15f, 0.91f))))

      // Comment this or press F10 to unlock the camera
      //dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)

      shaderSkybox <-
         GlProg.emptyBuilder
         |> GlProg.withName "Skybox"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Skybox.vert"
               ; ShaderType.FragmentShader, @"Skybox.frag" 
               ;]
         |> GlProg.withUniforms [
               "uView"
               "uProjection"
               "uSkybox"
         ]
         |> GlProg.build ctx

      skyboxVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"]
      |> GlVbo.withAttrDefinitions 
         Skybox.vertexPositions
      |> GlVbo.build (skyboxVao, ctx)
      |> ignore

      let cubemap = 
         GlTex.loadCubemap (Path.Combine("Skyboxes", "water_sky")) ".jpg" ctx

      //let cubemap = 
      //   GlTex.loadCubemap (Path.Combine("Skyboxes", "storforsen")) ".jpg" ctx

      skyboxCubemap <- buildCubemapGlTexture cubemap ctx

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

      // Needs to be Lequal instead of Less, so that the z-depth trick works
      // for the skybox, and it gets drawn behind everything even though
      // it´s rendered last.
      ctx.Gl.DepthFunc GLEnum.Lequal

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
       
       // CUBES
      (shaderSimple, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> GlProg.setUniformV3 "uCameraPosition" state.Camera.Position
      |> ignore
                     
      let borderScale = 1.05f
      GlVao.bind (cubeVao, ctx) |> ignore
        
      (cubeVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 skyboxCubemap
      |> ignore        
        
      let cube_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f))
        
      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube_ModelMatrix
      |> GlProg.setUniformI "uAmbientCubemap" 0
      |> ignore        
      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)
                
      // SKYBOX
      // Renders the skybox first, with disabled depth writing so it´s 
      // always in the background.
      ctx.Gl.DepthMask false
      (shaderSkybox, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> ignore

      (skyboxVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 skyboxCubemap
      |> ignore   
       
      (shaderSkybox, ctx)
      |> GlProg.setUniformI "uSkybox" 0
      |> ignore        
      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)  

      // Re enables the depth writing after the skybox is rendered
      ctx.Gl.DepthMask true

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