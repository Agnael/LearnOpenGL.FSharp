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
open Model

let initialState = 
   BaselineState.createDefault(
      "32_Blending", 
      new Size(640, 360))

let initialRes = initialState.Window.Resolution

let grassPositions = 
   [|
      new Vector3(-1.5f, 0.0f, -0.48f)
      new Vector3(1.5f, 0.0f, 0.51f)
      new Vector3(0.0f, 0.0f, 0.7f)
      new Vector3(-0.3f, 0.0f, -2.3f)
      new Vector3(0.5f, 0.0f, -0.6f)
   |]

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
   let mutable planeVao = Unchecked.defaultof<_>
   let mutable windowVao = Unchecked.defaultof<_>
   let mutable shaderSimple = Unchecked.defaultof<_>
   let mutable cubeTexture = Unchecked.defaultof<_>
   let mutable floorTexture = Unchecked.defaultof<_>
   let mutable windowTexture = Unchecked.defaultof<_>
            
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
         |> GlProg.withName "Simple"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Simple3D.vert"
               ; ShaderType.FragmentShader, @"Simple3D.frag" 
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

      // WINDOW
      windowVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao
            
      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
      |> GlVbo.withAttrDefinitions 
         Cube.vertexPositionsAndTextureCoords
      |> GlVbo.build (windowVao, ctx)
      |> ignore

      windowTexture <-
         GlTex.loadImageF "window.png" ctx
         |> fun img -> GlTex.create2dTransparent img ctx
                                            
      // Hardcoded camera position and target, so it looks just like the
      // LearnOpenGL.com example right away.
      dispatch (Camera (ForcePosition (new Vector3(-1.11f, 0.71f, -4.88f))))
      dispatch (Camera (ForceTarget (new Vector3(0.20f, -0.13f, 0.96f))))

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
      ctx.Gl.DepthFunc GLEnum.Less

      // Specific for the blending exercise
      ctx.Gl.Enable GLEnum.Blend
      ctx.Gl.BlendFunc (
         BlendingFactor.SrcAlpha, 
         BlendingFactor.OneMinusSrcAlpha)

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
       
      // Prepares the shaders
      (shaderSimple, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> ignore

      // PLANE
      GlVao.bind (planeVao, ctx) |> ignore
        
      (planeVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 floorTexture
      |> ignore
        
      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 6u)
      ctx.Gl.BindVertexArray 0u
                
      // CUBES PREP
      let borderScale = 1.05f
      GlVao.bind (cubeVao, ctx) |> ignore
        
      (cubeVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 cubeTexture
      |> ignore        
        
      // Cube 1
      let cube1_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(new Vector3(-1.0f, 0.0f, -1.0f))
        
      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube1_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore        
      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)
                        
      // Cube 2
      let cube2_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(new Vector3(2.0f, 0.0f, 0.0f))
        
      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube2_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore        
      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)
                
      // Draws grass
      (shaderSimple, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> ignore
              
      (windowVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 windowTexture
      |> ignore

      grassPositions
      // NOTE that semitransparent objects are ordered by their distance from
      // the camera. The furthest ones get to be rendered first.
      |> Array.sortByDescending (
         fun grassPos -> Vector3.Distance(state.Camera.Position, grassPos))
      |> Array.iter (
         fun grassPosition ->
            let grassPosModel = 
                  Matrix4x4.Identity *
                  Matrix4x4.CreateTranslation grassPosition

            (shaderSimple, ctx)
            |> GlProg.setUniformM4x4 "uModel" grassPosModel
            |> ignore

            ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 6u)
      )

      // Frame completed
      dispatch (FpsCounter(FrameRenderCompleted deltaTime))
      ()

   let onInputContextLoaded ctx ic state dispatch = 
      dispatch (Window (InitializeInputContext ic))

   let onWindowResize ctx state dispatch newSize =
      (ctx, state, dispatch, newSize)
      |> Baseline.handleWindowResize
      |> ignore

   // TODO: How did this return value come to exist?
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