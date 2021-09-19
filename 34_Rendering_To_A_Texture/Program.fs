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
open GlFbo
open GlTex
open Galante

let initialState = 
   BaselineState.createDefault(
      "34_Rendering_To_A_Texture", 
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
   let mutable cubeTexture = Unchecked.defaultof<_>
   let mutable floorTexture = Unchecked.defaultof<_>

   let mutable customFramebuffer = Unchecked.defaultof<_>
   let mutable screenQuadVao = Unchecked.defaultof<_>
   let mutable shaderPreNormalized = Unchecked.defaultof<_>
            
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
                                            
      // Hardcoded camera position and target, so it looks just like the
      // LearnOpenGL.com example right away.
      dispatch (Camera (ForcePosition (new Vector3(-2.13f, 0.64f, 2.64f))))
      dispatch (Camera (ForceTarget (new Vector3(0.60f, -0.22f, -0.76f))))

      // Comment this or press F10 to unlock the camera
      //dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)

      // Custom framebuffer
      customFramebuffer <-
         fboCreateStandard ctx.Window.Size.X ctx.Window.Size.Y ctx
         |> fun (fbo, _) -> fbo

      // Target screen-sized quad
      screenQuadVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      // vertex attributes for a quad that fills the entire screen in 
      // Normalized Device Coordinates.
      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
      |> GlVbo.withAttrDefinitions [|
         [| [| -1.0f; 1.0f |]; [| 0.0f; 1.0f |] |]
         [| [| -1.0f; -1.0f |]; [| 0.0f; 0.0f |] |]
         [| [| 1.0f; -1.0f |]; [| 1.0f; 0.0f |] |]
         
         [| [| -1.0f; 1.0f |]; [| 0.0f; 1.0f |] |]
         [| [| 1.0f; -1.0f |]; [| 1.0f; 0.0f |] |]
         [| [| 1.0f; 1.0f |]; [| 1.0f; 1.0f |] |]

      |]
      |> GlVbo.build (screenQuadVao, ctx)
      |> ignore

      // During FBO creation, that framebuffer is left as bound, so the default
      // one is bound again. Not necessary to do though.
      fboBindDefault ctx |> ignore

      // Shader that doesn´t do any camera calculations and expects direct UVs
      shaderPreNormalized <-
         GlProg.emptyBuilder
         |> GlProg.withName "PreNormalized"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"PreNormalized.vert"
               ; ShaderType.FragmentShader, @"Simple3D.frag" 
               ;]
         |> GlProg.withUniforms ["uTexture"]
         |> GlProg.build ctx

   let onUpdate (ctx: GlWindowCtx) (state) dispatch (DeltaTime deltaTime) =
      (ctx, state, dispatch, deltaTime)
      |> Baseline.updateWindowClosure
      |> Baseline.updateWindowTitle
      |> Baseline.updateWindowResolution
      |> Baseline.updateWindowMode
      |> Baseline.updateCursorMode
      |> Baseline.updateCameraPosition
      |> ignore

   let renderScene ctx state =
      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

      let res = state.Window.Resolution
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)
      let projectionMatrix = 
         Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)
      
      // Prepares the shader
      (shaderSimple, ctx)
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

      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube1_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)
       
      // CUBE 2
      let cube2_ModelMatrix = 
         Matrix4x4.Identity *
         Matrix4x4.CreateTranslation(new Vector3(2.0f, 0.0f, 0.0f))

      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" cube2_ModelMatrix
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)

      // PLANE
      GlVao.bind (planeVao, ctx) |> ignore
       
      (planeVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 floorTexture
      |> ignore
       
      (shaderSimple, ctx)
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 6ul)
      //ctx.Gl.BindVertexArray 0ul
      ()


   let onRender ctx state dispatch (DeltaTime deltaTime) =
      // First pass (Real scene on the custom framebuffer)
      fboBind (customFramebuffer, ctx)
      |> ignore
      
      ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      |> ctx.Gl.Clear

      ctx.Gl.Enable GLEnum.DepthTest
      ctx.Gl.DepthFunc GLEnum.Less

      let customFbColorAtt =
         match customFramebuffer.ColorAttachment with
         | Some colorAttachment -> colorAttachment
         | None -> failwith "No color attachment was associated to this FBO"
      
      // Ensures the scene is drawn filling the whole texture
      ctx.Gl.Viewport (
         new Size(customFbColorAtt.Width, customFbColorAtt.Height))
                      
      renderScene ctx state

      // Second pass (Real scene on the default framebuffer)
      fboBindDefault ctx |> ignore
      ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
      ctx.Gl.Clear (uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit))
      
      ctx.Gl.Viewport ctx.Window.Size
      renderScene ctx state

      // Third pass (Custom framebuffer on the default framebuffer)
      let winRes = state.Window.Resolution
      let textureSize = new Size(winRes.Width / 4, winRes.Height / 4)

      // Aims at rendering it at the top center of the screen
      let texturePosX = winRes.Width / 2 - textureSize.Width / 2
      let texturePosY = 0
      let texturePosition = new Point(texturePosX, texturePosY)

      ctx.Gl.Viewport (texturePosition, textureSize)

      // Disabled depth testing just to be 100% sure this quad will get 
      // rendered on the front.
      ctx.Gl.Disable GLEnum.DepthTest

      (screenQuadVao, ctx)
      |> GlTex.setActiveEmptyTexture GLEnum.Texture0 customFbColorAtt
      |> ignore

      // Prepares the shader
      (shaderPreNormalized, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformI "uTexture" 0
      |> ignore

      ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 6ul)

      dispatch (FpsCounter(FrameRenderCompleted deltaTime))
      ()

   let onInputContextLoaded ctx ic state dispatch = 
      dispatch (Window (InitializeInputContext ic))

   let onWindowResize ctx state dispatch (newSize: Vector2) =
      // Destroys the previous framebuffer, which now has an outdated size
      fboDestroy customFramebuffer ctx

      customFramebuffer <-
         fboCreateStandard (int newSize.X) (int newSize.Y) ctx
         |> fun (fbo, _) -> fbo

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
      |> buildAndRun
   0