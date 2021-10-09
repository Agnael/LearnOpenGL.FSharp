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
      "44_Visualizing_Normal_Vectors", 
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

   let mutable regularShader = Unchecked.defaultof<_>
   let mutable normalDisplayShader = Unchecked.defaultof<_>
   let mutable wireframeDisplayShader = Unchecked.defaultof<_>

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
      regularShader <-
         GlProg.emptyBuilder
         |> GlProg.withName "3dShader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "3d.vert"
            ShaderType.FragmentShader, "3d.frag" 
         ]
         |> GlProg.withUniforms [
            "uMaterial.diffuseMap"
            "uMaterial.specularMap"
            "uMaterial.shininess"
            "uModel"
         ]
         |> GlProg.build ctx

      normalDisplayShader <-
         GlProg.emptyBuilder
         |> GlProg.withName "NormalDisplayShader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "3dNormalDisplay.vert"
            ShaderType.GeometryShader, "3dNormalDisplay.geom"
            ShaderType.FragmentShader, "3dNormalDisplay.frag" 
         ]
         |> GlProg.withUniforms ["uModel"]
         |> GlProg.build ctx

      wireframeDisplayShader <-
         GlProg.emptyBuilder
         |> GlProg.withName "WireframeDisplayShader"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "3dWireframeDisplay.vert"
            ShaderType.FragmentShader, "3dWireframeDisplay.frag" 
         ]
         |> GlProg.withUniforms ["uModel"]
         |> GlProg.build ctx
           
      ctx
      |> Baseline.bindShaderToUbo regularShader matricesUboDef state dispatch
      |> Baseline.bindShaderToUbo normalDisplayShader matricesUboDef state dispatch
      |> Baseline.bindShaderToUbo wireframeDisplayShader matricesUboDef state dispatch
      |> ignore
                 
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
         
      dispatch (Camera (ForcePosition (v3 2.83f 0.69f 0.61f)))
      dispatch (Camera (ForceTarget (v3 -0.61f -0.17f -0.77f)))

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
      // Sets matrices UBO for all shaders
      let res = state.Window.Resolution
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)
      let projectionMatrix = 
         Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)

      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

      ctx
      |> Baseline.setUboUniformM4 
            matricesUboDef "uProjection" projectionMatrix state
      |> Baseline.setUboUniformM4 
            matricesUboDef "uView" viewMatrix state
      |> ignore
        
      // **********************************************************************
            
      let getTextureHandler imgPath =
         state.Asset.ImagesLoaded.TryFind imgPath
         |> function
               | Some asset -> 
                  match asset.GlTexture with
                  | Some assetGlTexture -> assetGlTexture
                  | None -> fallbackGlTexture
               | None -> fallbackGlTexture

      (regularShader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> ignore
      Model.draw mdlBackpack regularShader getTextureHandler ctx

      (wireframeDisplayShader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> ignore
      Model.drawWireframe mdlBackpack wireframeDisplayShader ctx

      (normalDisplayShader, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> ignore
      Model.drawTextureless mdlBackpack normalDisplayShader ctx
               
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