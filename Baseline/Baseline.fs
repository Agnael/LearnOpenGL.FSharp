﻿[<RequireQualifiedAccess>]
module Baseline

open GalanteMath
open Galante
open Silk.NET.Input
open BaseWindowSlice
open BaseMouseSlice
open System.Drawing
open BaseCameraSlice
open System.Numerics
open Galante.OpenGL
open Silk.NET.Windowing
open BaselineState
open System
open BaseAssetSlice
open BaseGlSlice
open Gl
open Silk.NET.OpenGL
open Microsoft.FSharp
open Microsoft.FSharp.NativeInterop
        
let private aMovement dispatch cameraAction = 
   dispatch (Camera cameraAction)

let private aWindow dispatch windowAction = 
   dispatch (Window windowAction)

let private aWindowResolutionUpdate dispatch w h  = 
   dispatch (Window (ResolutionUpdate (new Size(w, h))))

let private aMouse dispatch mouseAction = 
   dispatch (Mouse mouseAction)

let private aCamera dispatch cameraAction = dispatch (Camera cameraAction)

// Input handlers
let detectFullScreenSwitch 
   (ctx: GlWindowCtx, state, dispatch, kb: IKeyboard, key) =
    
   let isAnyAltPressed =
      kb.IsKeyPressed(Key.AltLeft) || 
      kb.IsKeyPressed(Key.AltRight)

   if isAnyAltPressed && kb.IsKeyPressed(Key.Enter) then 
      aWindow dispatch ToggleFullscreen

   (ctx, state, dispatch, kb, key)

let detectCameraMovementStart 
   (ctx: GlWindowCtx, state, dispatch, kb: IKeyboard, key) =
   match key with
   | Key.W         -> aMovement dispatch MoveForwardStart 
   | Key.A         -> aMovement dispatch MoveLeftStart 
   | Key.S         -> aMovement dispatch MoveBackStart 
   | Key.D         -> aMovement dispatch MoveRightStart 
   | Key.Space     -> aMovement dispatch MoveUpStart 
   | Key.ShiftLeft -> aMovement dispatch MoveDownStart 
   | _ -> ()
   (ctx, state, dispatch, kb, key)

let detectCameraMovementStop 
   (ctx: GlWindowCtx, state, dispatch, kb: IKeyboard, key) =
   match key with
   | Key.W         -> aMovement dispatch MoveForwardStop
   | Key.A         -> aMovement dispatch MoveLeftStop
   | Key.S         -> aMovement dispatch MoveBackStop
   | Key.D         -> aMovement dispatch MoveRightStop
   | Key.Space     -> aMovement dispatch MoveUpStop
   | Key.ShiftLeft -> aMovement dispatch MoveDownStop
   | _ -> ()
   (ctx, state, dispatch, kb, key)

let detectGameClosing
   (ctx: GlWindowCtx, state, dispatch, kb: IKeyboard, key) =
   match key with 
   | Key.Escape -> aWindow dispatch Close 
   | _ -> ()
   (ctx, state, dispatch, kb, key)

let detectResolutionChange
   initialW initialH (ctx: GlWindowCtx, state, dispatch, kb: IKeyboard, key) =
   match key with
   | Key.F5        -> aWindowResolutionUpdate dispatch initialW initialH
   | Key.F6        -> aWindowResolutionUpdate dispatch 1280 720 
   | Key.F7        -> aWindowResolutionUpdate dispatch 1920 1080 
   | Key.F8        -> 
      let nullOffset = { CameraOffset.X = 0.0f; Y = 0.0f; }
      aCamera dispatch (AngularChange nullOffset)
   | _ -> ()
   (ctx, state, dispatch, kb, key)

let detectCursorModeChange
   (ctx: GlWindowCtx, state, dispatch, kb: IKeyboard, key) =
   match key with
   | Key.F9 -> 
      aMouse dispatch UseCursorNormal
      aCamera dispatch Lock
   | Key.F10 -> 
      aMouse dispatch UseCursorRaw
      aCamera dispatch Unlock
   | _ -> ()
   (ctx, state, dispatch, kb, key)

let handleCameraAngularChange (ctx, state, dispatch, newMousePos: Vector2)  = 
   aMouse dispatch (NewPosition newMousePos)

   if not state.Mouse.IsFirstMoveReceived then
      aMouse dispatch FirstMoveReceived
   else
      // Reversed  y-coordinates since they range from bottom to top
      let cameraOffset =
         { CameraOffset.X = newMousePos.X - state.Mouse.X 
         ; CameraOffset.Y = state.Mouse.Y - newMousePos.Y
         ;}
      dispatch (Camera (AngularChange cameraOffset))
   (ctx, state, dispatch, newMousePos)
        
let handleCameraZoom (ctx, state, dispatch, newWheelPos: Vector2) =
   let zoomOffset = ZoomOffset (newWheelPos.Y * state.Camera.ZoomSpeed)
   dispatch (Camera (ZoomChange zoomOffset))
   (ctx, state, dispatch, newWheelPos)

// On update handlers
let updateWindowResolution (ctx: GlWindowCtx, state: BaselineState, dispatch, dt) =
   if state.Window.ShouldUpdateResolution then
      let res = state.Window.Resolution
      let newSize = 
         new Silk.NET.Maths.Vector2D<int>(res.Width, res.Height)

      ctx.Window.Size <- newSize                
      ctx.Gl.Viewport (0, 0, uint32 newSize.X, uint32 newSize.Y)
      aWindow dispatch ResolutionUpdated
   (ctx, state, dispatch, dt)

let updateWindowMode (ctx: GlWindowCtx, state: BaselineState, dispatch, dt) =
   if  
      state.Window.IsFullscreen && 
      not (ctx.Window.WindowState = WindowState.Fullscreen) 
   then      
      let displaySize = ctx.Window.Monitor.Bounds.Size

      ctx.Gl.Viewport (0, 0, uint32 displaySize.X, uint32 displaySize.Y)
      ctx.Window.WindowState <- WindowState.Fullscreen

   elif 
      state.Window.IsFullscreen = false && 
      ctx.Window.WindowState = WindowState.Fullscreen 
   then
      let res = state.Window.Resolution
      let newSize = 
         new Silk.NET.Maths.Vector2D<int>(res.Width, res.Height)
      ctx.Gl.Viewport (0, 0, uint32 newSize.X, uint32 newSize.Y)
      ctx.Window.WindowState <- WindowState.Normal
      aWindowResolutionUpdate dispatch res.Width res.Height
   (ctx, state, dispatch, dt)

let v3toString (v3: Vector3) =
   sprintf
      "<%.2fx %.2fy %.2fz>" 
      (Math.Round(float v3.X, 2))
      (Math.Round(float v3.Y, 2)) 
      (Math.Round(float v3.Z, 2))

let updateWindowTitle (ctx: GlWindowCtx, state: BaselineState, dispatch, dt) =
   ctx.Window.Title <-
      sprintf 
         "%s [%i FPS][Pos %s][Dir %s][Pitch %f][Yaw %f]" 
         state.Window.Title
         state.FpsCounter.CurrentFps
         (v3toString state.Camera.Position)
         (v3toString state.Camera.TargetDirection)
         (Degrees.value state.Camera.Pitch)
         (Degrees.value state.Camera.Yaw)
   (ctx, state, dispatch, dt)

let updateCursorMode (ctx: GlWindowCtx, state: BaselineState, dispatch, dt) =
   match state.Window.InputContext with
   | Initialized inputs -> 
      // All mice should have identical state
      let mouse = Seq.head inputs.Mice

      if not (state.Mouse.CursorMode = mouse.Cursor.CursorMode) then
         mouse.Cursor.CursorMode <- state.Mouse.CursorMode
   | _ -> ()
   (ctx, state, dispatch, dt)

let updateWindowClosure (ctx: GlWindowCtx, state: BaselineState, dispatch, dt) =
   if state.Window.ShouldClose then 
      ctx.Window.Close()
   (ctx, state, dispatch, dt)

let updateCameraPosition (ctx: GlWindowCtx, state, dispatch, dt: double) =
   let dynCamSpeed = 
      state.Camera.MoveSpeed * single(dt)
      |> CameraSpeed.make
   dispatch (Camera (UpdatePosition dynCamSpeed))
   (ctx, state, dispatch, dt)

// On window resize
let handleWindowResize (ctx, state, dispatch, newSize: Vector2) =
   let newSize = new Size(int newSize.X, int newSize.Y)
   if state.Window.Resolution <> newSize then
      aWindowResolutionUpdate dispatch newSize.Width newSize.Height
   (ctx, state, dispatch, newSize)
   
let handleInterceptedAction state action dispatch ctx =
   match action with
   | Asset assetAction ->
      let assetDispatch a = dispatch (Asset a)
      BaseAssetSlice.listen state.Asset assetAction assetDispatch ctx
   | _ -> ()

/// Binds images as textures on the GL thread so they are available on 
/// render and don't need to be created on the GPU on each render.
let glCreateQueuedTextures (ctx: GlWindowCtx, state, dispatch, dt: double) =
   let createAndDispatchTexture imgPath imgAsset =
      if imgAsset.GlTexture.IsNone then
            let glTexture = GlTex.create2d imgAsset.Image ctx
            dispatch (Asset (BindedImageAsTexture(imgPath, glTexture)))

   state.Asset.ImagesLoaded
   |> Map.iter createAndDispatchTexture

   (ctx, state, dispatch, dt)


            
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

let bindShaderToUbo shader uboDef state dispatch (ctx: GlWindowCtx) =
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
            UniformBlockBindingIndex = 
               uint32 (state.Gl.SharedUbos.Length + 14)
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