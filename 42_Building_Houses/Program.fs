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

let initialState = 
   BaselineState.createDefault(
      "42_Building_Houses", 
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

   let mutable vaoPoints = Unchecked.defaultof<_>

   let mutable shader = Unchecked.defaultof<_>

   //let mutable matricesUbo = Unchecked.defaultof<_>
   
   let matricesUboDef = {
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
            
   let setUboUniformM4
      (uboDef: GlUniformBlockDefinition)
      uboUniformName
      m4
      state
      (ctx: GlWindowCtx) = 
         let sharedUbo = 
            state.Gl.SharedUbos
            |> List.find (fun x -> x.Ubo.Definition.Name = uboDef.Name)

         if sharedUbo.BoundShaders.Length > 0 then
            // Since UBOs will be allways assumed to be using the 
            // "shared" layout, we can take ANY shader that´s using
            // it as a valid model to get any necessary value, such as
            // offests of a uniform within it´s uniform block.
            let sampleShader = 
               sharedUbo.BoundShaders.Head

            // Fetches this uniform´s offset within the uniform block,
            // to insert it without altering existing data in the
            // buffer.
            let mutable targetUniformIndex: uint32 array = Array.zeroCreate 1
         
            ctx.Gl.GetUniformIndices(
               sampleShader.Shader.GlProgramHandle, 
               1ul,
               [| uboUniformName |],
               &targetUniformIndex.[0])

            let mutable targetUniformOffset: int array = 
               Array.zeroCreate 1
               
            ctx.Gl.GetActiveUniforms(
               sampleShader.Shader.GlProgramHandle,
               1ul,
               &targetUniformIndex.[0],
               UniformPName.UniformOffset,
               &targetUniformOffset.[0])

            // Stores the value
            ctx.Gl.BindBuffer(
               BufferTargetARB.UniformBuffer, 
               sharedUbo.Ubo.GlUboHandle)

            let mutable m4MutableCopy = m4
            let projectionMatrixNativeInt = 
               NativePtr.toNativeInt<Matrix4x4> &&m4MutableCopy

            ctx.Gl.BufferSubData(
               BufferTargetARB.UniformBuffer,
               nativeint targetUniformOffset.[0],
               unativeint(sizeof<Matrix4x4>),
               projectionMatrixNativeInt.ToPointer())

            ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0ul)
         ctx

   let bindShaderToUbo
      (shader: GlProgram)
      (uboDef: GlUniformBlockDefinition) 
      (state: BaselineState)
      dispatch
      (ctx: GlWindowCtx) =
         let bindToBindingPoint ubo shader =
            // Get´s the block´s index within the current shader
            let uboName = ubo.Definition.Name

            let blockIndexWithinShader = 
               glGetUniformBlockIndex shader uboName ctx

            // Links the shader´s uniform block to the UBO's binding point
            ctx
            |> glUniformBlockBinding 
               shader
               blockIndexWithinShader 
               ubo.UniformBlockBindingIndex 
            |> ignore

            let sharedUboShaderRef: SharedUboShaderRef = {
               Shader = shader
               UniformBlockIndex = blockIndexWithinShader
            }
            dispatch (Gl (BindShaderToUbo (uboDef, sharedUboShaderRef)))

         state.Gl.SharedUbos
         |> List.tryFind (fun x -> x.Ubo.Definition.Name = uboDef.Name)
         |> function
            | Some sharedUbo ->
               bindToBindingPoint sharedUbo.Ubo shader
            | None ->
               let sharedUbo = {
                  GlUboHandle = ctx.Gl.GenBuffer()
                  UniformBlockBindingIndex = uint32 (state.Gl.SharedUbos.Length + 14)
                  Definition = uboDef
               }

               let asd = glGetUniformIndices shader uboDef ctx
               
               let uniformBlockIndexWithinShader = 
                  ctx.Gl.GetUniformBlockIndex(
                     shader.GlProgramHandle,
                     uboDef.Name)

               let mutable uboSize = 0

               ctx.Gl.GetActiveUniformBlock(
                  shader.GlProgramHandle,
                  uniformBlockIndexWithinShader,
                  UniformBlockPName.UniformBlockDataSize,
                  &uboSize)

               ctx
               |> glBindBuffer BufferTargetARB.UniformBuffer sharedUbo
               |> glBufferDataEmpty
                     BufferTargetARB.UniformBuffer
                     (unativeint uboSize)
                     BufferUsageARB.StaticDraw
               |> glBindBufferDefault BufferTargetARB.UniformBuffer
               |> glBindBufferRange
                     BufferTargetARB.UniformBuffer
                     sharedUbo.UniformBlockBindingIndex
                     sharedUbo.GlUboHandle
                     (nativeint 0)
                     (unativeint uboSize)
               |> ignore

               dispatch (Gl (AddSharedUbo sharedUbo))
               bindToBindingPoint sharedUbo shader
         ctx

   let onLoad (ctx: GlWindowCtx) input (state: BaselineState) dispatch =
      shader <-
         GlProg.emptyBuilder
         |> GlProg.withName "Shader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "Vertex.vert"
            ShaderType.GeometryShader, "Geometry.geom"
            ShaderType.FragmentShader, "Fragment.frag" 
         ]
         |> GlProg.withUniforms []
         |> GlProg.build ctx

      // CUBE
      vaoPoints <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      GlVbo.emptyVboBuilder
      |> GlVbo.withAttrNames ["Positions"; "Colors"]
      |> GlVbo.withAttrDefinitions [|
         [| [| -0.5f;  0.5f |]; [| 1.0f; 0.0f; 0.0f |] |]
         [| [|  0.5f;  0.5f |]; [| 0.0f; 1.0f; 0.0f |] |]
         [| [|  0.5f; -0.5f |]; [| 0.0f; 0.0f; 1.0f |] |]
         [| [| -0.5f; -0.5f |]; [| 1.0f; 1.0f; 0.0f |] |]
      |]
      |> GlVbo.build (vaoPoints, ctx)
      |> ignore
                                                     
      dispatch (Mouse UseCursorRaw)
      
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
        
      // **********************************************************************
      GlVao.bind (vaoPoints, ctx) |> ignore

      (shader, ctx)
      |> GlProg.setAsCurrent
      |> ignore

      ctx.Gl.DrawArrays(GLEnum.Points, 0, 4u);
               
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