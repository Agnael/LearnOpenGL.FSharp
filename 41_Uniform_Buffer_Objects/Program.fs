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
open Gl
open Microsoft.FSharp.NativeInterop

let initialState = 
   BaselineState.createDefault(
      "41_Uniform_Buffer_Objects", 
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
         IsVsync = true
         Title = initialState.Window.Title
         Size = initialRes }

   let mutable cubeVao = Unchecked.defaultof<_>

   let mutable shaderRed = Unchecked.defaultof<_>
   let mutable shaderGreen = Unchecked.defaultof<_>
   let mutable shaderBlue = Unchecked.defaultof<_>
   let mutable shaderYellow = Unchecked.defaultof<_>

   let mutable matricesUbo = Unchecked.defaultof<_>
   
   let matricesUniformBlock = {
      Name = "Matrices";
      UniformNames = ["uProjection"; "uView"]
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
            
   let onLoad (ctx: GlWindowCtx) input state dispatch =
      shaderRed <-
         GlProg.emptyBuilder
         |> GlProg.withName "Red"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Vertex.vert"
               ; ShaderType.FragmentShader, @"Red.frag" 
               ;]
         |> GlProg.withUniforms ["uModel"]
         |> GlProg.build ctx

      shaderGreen <-
         GlProg.emptyBuilder
         |> GlProg.withName "Green"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Vertex.vert"
               ; ShaderType.FragmentShader, @"Green.frag" 
               ;]
         |> GlProg.withUniforms ["uModel"]
         |> GlProg.build ctx

      shaderBlue <-
         GlProg.emptyBuilder
         |> GlProg.withName "Shader"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Vertex.vert"
               ; ShaderType.FragmentShader, @"Blue.frag" 
               ;]
         |> GlProg.withUniforms ["uModel"]
         |> GlProg.build ctx

      shaderYellow <-
         GlProg.emptyBuilder
         |> GlProg.withName "Shader"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Vertex.vert"
               ; ShaderType.FragmentShader, @"Yellow.frag" 
               ;]
         |> GlProg.withUniforms ["uModel"]
         |> GlProg.build ctx
            
      // CUBE
      cubeVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "TextureCoords"]
      |> GlVbo.withAttrDefinitions 
         CubeCCW.vertexPositionsAndTextureCoords
      |> GlVbo.build (cubeVao, ctx)
      |> ignore
                                                     
      // Hardcoded camera position and target, so it looks just like the
      // LearnOpenGL.com example right away.
      dispatch (Camera (ForcePosition (new Vector3(0.00f, -0.66f, 4.13f))))
      dispatch (Camera (ForceTarget (new Vector3(-0.00f, 0.15f, -0.98f))))

      dispatch (Mouse UseCursorRaw)

      // Comment this or press F10 to unlock the camera
      //dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)
      
      // **********************************************************************
      // UBO creation ---------------------------------------------------------
      let res = state.Window.Resolution
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)

      let matricesUniformIdxRed = 
         glGetUniformBlockIndex shaderRed "Matrices" ctx
      let matricesUniformIdxGreen = 
         glGetUniformBlockIndex shaderGreen "Matrices" ctx
      let matricesUniformIdxBlue = 
         glGetUniformBlockIndex shaderBlue "Matrices" ctx
      let matricesUniformIdxYellow = 
         glGetUniformBlockIndex shaderYellow "Matrices" ctx
      
      // **********************************************************************
      // Links each shader's uniform block to this uniform binding point
      ctx
      |> glUniformBlockBinding shaderRed matricesUniformIdxRed 0ul
      |> glUniformBlockBinding shaderGreen matricesUniformIdxGreen 0ul
      |> glUniformBlockBinding shaderBlue matricesUniformIdxBlue 0ul
      |> glUniformBlockBinding shaderYellow matricesUniformIdxYellow 0ul
      |> ignore
      
      // **********************************************************************
      // Creates the actual Uniform Buffer Object
      matricesUbo <- ctx.Gl.GenBuffer();

      ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, matricesUbo)

      ctx.Gl.BufferData(
         BufferTargetARB.UniformBuffer, 
         unativeint (2 * sizeof<Matrix4x4>), 
         IntPtr.Zero.ToPointer(), 
         BufferUsageARB.StaticDraw)

      ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0ul)

      // Defines the range of the buffer, that links to a uniform binding point
      ctx.Gl.BindBufferRange(
         BufferTargetARB.UniformBuffer,
         0ul,
         matricesUbo,
         nativeint(0),
         unativeint(2 * sizeof<Matrix4x4>))

      // Stores the projection matrix
      let projectionMatrix = 
         Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)

      ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, matricesUbo)

      let mutable projectionMatrixMutableCopy = projectionMatrix
      let projectionMatrixNativeInt = 
         NativePtr.toNativeInt<Matrix4x4> &&projectionMatrixMutableCopy

      ctx.Gl.BufferSubData(
         BufferTargetARB.UniformBuffer,
         IntPtr.Zero,
         unativeint(sizeof<Matrix4x4>),
         projectionMatrixNativeInt.ToPointer())

      ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0ul)

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

      ctx.Gl.Enable GLEnum.Blend
      ctx.Gl.BlendFunc (
         BlendingFactor.SrcAlpha, 
         BlendingFactor.OneMinusSrcAlpha)

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      |> ctx.Gl.Clear

      // Enables access to the point access GL variable in the vertex shader
      ctx.Gl.Enable EnableCap.ProgramPointSize
        
      // Sets a dark grey background so the cube´s color changes are visible
      ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
        
      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

      // Stores the view matrix
      ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, matricesUbo)

      let mutable viewMatrixMutableCopy = viewMatrix
      let viewMatrixNativeInt = 
         NativePtr.toNativeInt<Matrix4x4> &&viewMatrixMutableCopy

      // Note that this time, enough bytes for a M4x4 is skipped before writing
      // this second value in the uniform layout.
      ctx.Gl.BufferSubData(
         BufferTargetARB.UniformBuffer,
         nativeint(sizeof<Matrix4x4>),
         unativeint(sizeof<Matrix4x4>),
         viewMatrixNativeInt.ToPointer())

      ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0ul)

      // **********************************************************************
      let renderCube shader translationVector =
         GlVao.bind (cubeVao, ctx) |> ignore

         let modelMatrix = 
            Matrix4x4.Identity *
            Matrix4x4.CreateTranslation translationVector

         (shader, ctx)
         |> GlProg.setAsCurrent
         |> GlProg.setUniformM4x4 "uModel" modelMatrix
         |> ignore
         ctx.Gl.DrawArrays(GLEnum.Triangles, 0, 36u);

      // RED cube
      renderCube shaderRed (new Vector3(-0.75f, 0.75f, 0.0f))
      renderCube shaderGreen (new Vector3(0.75f, 0.75f, 0.0f))
      renderCube shaderBlue (new Vector3(0.75f, -0.75f, 0.0f))
      renderCube shaderYellow (new Vector3(-0.75f, -0.75f, 0.0f))
      
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