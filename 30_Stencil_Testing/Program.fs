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
      "30_Stencil_Testing", 
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
   let mutable shaderSimple = Unchecked.defaultof<_>
   let mutable shaderSingleColor = Unchecked.defaultof<_>
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

      shaderSingleColor <-
         GlProg.emptyBuilder
         |> GlProg.withName "SingleColor"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Simple3D.vert"
               ; ShaderType.FragmentShader, @"SingleColor.frag" 
               ;]
         |> GlProg.withUniforms [
               "uModel"
               "uView"
               "uProjection"
         ]
         |> GlProg.build ctx
            
      // CUBE
      cubeVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

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
      dispatch (Camera (ForcePosition (new Vector3(-2.99f, 0.95f, -3.46f))))
      dispatch (Camera (ForceTarget (new Vector3(0.66f, -0.29f, 0.68f))))

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
      ctx.Gl.Enable GLEnum.StencilTest
      ctx.Gl.StencilFunc (GLEnum.Notequal, 1, 255u)
      ctx.Gl.StencilOp (GLEnum.Keep, GLEnum.Keep, GLEnum.Replace)

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit ||| GLEnum.StencilBufferBit)
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
      // Renders the floor without writing to the stencil buffer
      ctx.Gl.StencilMask 0u

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
        
      // CUBES 1st PASS
      // Write the cube to the stencil buffer so the 2nd pass will skip this area
      ctx.Gl.StencilFunc (GLEnum.Always, 1, 255u)
      ctx.Gl.StencilMask (uint 255u)

      let cube1pos = new Vector3(-1.0f, 0.0f, -1.0f);
      let cube2pos = new Vector3(2.0f, 0.0f, 0.0f);

      // Cube 1
      let cube1_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(cube1pos)
        
      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube1_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore        
      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)
                        
      // Cube 2
      let cube2_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(cube2pos)
        
      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube2_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore        
      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)
        
      // CUBES 2nd PASS
      // Changes the stencil mode so the tests pass only where the buffer was 
      // not marked by the previous render pass.
      // Renders the 1st cube again but scaled up.
      ctx.Gl.StencilFunc (GLEnum.Notequal, 1, 255u)
      ctx.Gl.StencilMask (uint 0u)
      ctx.Gl.Disable GLEnum.DepthTest
        
      (shaderSingleColor, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> ignore    

      // Cube 1 border
      let depthBufferBits =
         ctx.Gl.GetFramebufferAttachmentParameter(
            GLEnum.DrawFramebuffer, 
            GLEnum.Depth, 
            GLEnum.FramebufferAttachmentDepthSize)

      let stencilBufferBits = 
         ctx.Gl.GetFramebufferAttachmentParameter(
            GLEnum.DrawFramebuffer, 
            GLEnum.Stencil, 
            GLEnum.FramebufferAttachmentStencilSize)

      let adapted_Cube1_ModelMatrix =
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(cube1pos / borderScale) *
         Matrix4x4.CreateScale borderScale
        
      (shaderSingleColor, ctx)
      |> GlProg.setUniformM4x4 "uModel" adapted_Cube1_ModelMatrix
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)
        
      // Cube 2 border
      let adapted_Cube2_ModelMatrix =
         Matrix4x4.Identity 
         * Matrix4x4.CreateTranslation(Vector3.Divide(cube2pos, borderScale)) 
         * Matrix4x4.CreateScale borderScale

      (shaderSingleColor, ctx)
      |> GlProg.setUniformM4x4 "uModel" adapted_Cube2_ModelMatrix
      |> ignore    
      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36u)
        
      ctx.Gl.BindVertexArray 0u
      ctx.Gl.StencilMask (uint 255u)
      ctx.Gl.StencilFunc (GLEnum.Always, 0, 255u)
      ctx.Gl.Enable GLEnum.DepthTest

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