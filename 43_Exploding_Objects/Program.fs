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
      "43_Exploding_Objects", 
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
         
   let mutable fallbackGlTexture = Unchecked.defaultof<_>
   let mutable mdlBackpack = Unchecked.defaultof<_>

   let mutable shader = Unchecked.defaultof<_>

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
            ShaderType.VertexShader, "Simple3d.vert"
            ShaderType.GeometryShader, "Geometry.geom"
            ShaderType.FragmentShader, "Simple3d.frag" 
         ]
         |> GlProg.withUniforms [
            "uProjection"
            "uView"
            "uModel"
            "uMaterial.diffuseMap"
            "uMaterial.specularMap"
            "uMaterial.shininess"
            "uTime"
         ]
         |> GlProg.build ctx
               
      // Comment this or press F10 to unlock the camera
      dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)

      // Fallback texture
      let fallbackImagePath =
         Path.Combine("Resources", "Textures", "fallback.jpg")

      fallbackGlTexture <-
         GlTex.loadImage fallbackImagePath ctx
         |> fun img -> GlTex.create2d img ctx

      // Loads the backpack model
      let modelsDir = Path.Combine("Resources", "Models")
      let loadTexture path = dispatch (Asset (LoadImageStart path))

      let makeMdlPath pathParts = 
         pathParts
         |> List.append [modelsDir]
         |> fun fullList -> Path.Combine(List.toArray fullList)

      mdlBackpack <- 
         Model.loadU (makeMdlPath ["Backpack"; "backpack.obj"]) ctx loadTexture          
         
      dispatch (Camera (ForcePosition (v3 -6.55f 0.16f 5.79f)))
      dispatch (Camera (ForceTarget (v3 0.72f 0.03f -0.7f)))

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
      |> ignore
      
      // Binds images as textures on the GL thread so they are available on 
      // render and don't need to be created on the GPU on each render.
      state.Asset.ImagesLoaded
      |> Map.iter (fun imgPath imgAsset ->
         if imgAsset.GlTexture.IsNone then
               let glTexture = GlTex.create2d imgAsset.Image ctx
               dispatch (Asset (BindedImageAsTexture(imgPath, glTexture)))
      )

   let onRender (ctx: GlWindowCtx) state dispatch (DeltaTime deltaTime) =
      ctx.Gl.Enable GLEnum.DepthTest

      // Needs to be Lequal instead of Less, so that the z-depth trick works
      // for the skybox, and it gets drawn behind everything even though
      // it´s rendered last.
      ctx.Gl.DepthFunc GLEnum.Less

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      |> ctx.Gl.Clear

      // Enables access to the point access GL variable in the vertex shader
      ctx.Gl.Enable EnableCap.ProgramPointSize
        
      // Sets a dark grey background so the cube´s color changes are visible
      ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
        
      // **********************************************************************
      let res = state.Window.Resolution
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)
      let projectionMatrix = 
         Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)

      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera
            
      let getTextureHandler imgPath =
         state.Asset.ImagesLoaded.TryFind imgPath
         |> function
               | Some asset -> 
                  match asset.GlTexture with
                  | Some assetGlTexture -> assetGlTexture
                  | None -> fallbackGlTexture
               | None -> fallbackGlTexture

      (shader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> GlProg.setUniformF "uTime" (single ctx.Window.Time)
      |> ignore

      Model.draw mdlBackpack shader getTextureHandler ctx
               
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