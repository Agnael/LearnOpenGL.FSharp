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
open Serilog
open Serilog.Extensions.Logging

let initialState = 
   BaselineState.createDefault(
      "45_Instancing", 
      new Size(640, 360))

let initialRes = initialState.Window.Resolution

let v3toFloatArray (v3: Vector3): single array = 
   [| v3.X; v3.Y; v3.X |]

let v3arrayToFloatArrayArray (v3array: Vector3 array) =
   v3array
   |> Array.map (fun x -> [| v3toFloatArray x |])

let rockCount = 100000
let radiusAroundPlanet = 150.0f
let offset = 25.0f

let random = new System.Random()

let rockModelMatrices: Matrix4x4 array =
   let createModelMatrix idx =
      // 1. translation: displace along circle with 'radius' in range [-offset, offset]
      let angle: single = single  idx / (single rockCount) * 360.0f
      let getDisplacement (): single = 
         single (
            random.Next() % (2 * (int offset) * 100)
         ) / 
         100.0f - offset

      let x = (sin angle) * radiusAroundPlanet + getDisplacement()
      // keep height of field smaller compared to width of x and z
      let y = getDisplacement() * 0.4f
      let z = (cos angle) * radiusAroundPlanet + getDisplacement()
      let translationVec = v3 x y z
      
      // 2. scale: scale between 0.05 and 0.25f
      let scale = single (random.Next() % 20) / 100.0f + 0.05f;
      
      // 3. rotation: add random rotation around a (semi)randomly picked 
      // rotation axis vector
      let rotationAngle = 
         single (random.Next() % 360)
         |> Degrees
         |> toRadians
         |> fun (Radians value) -> value

      let centerPoint = translationVec * scale

      Matrix4x4.Identity *
      Matrix4x4.CreateTranslation translationVec *
      Matrix4x4.CreateScale scale *
      Matrix4x4.CreateRotationX (rotationAngle, centerPoint) *
      Matrix4x4.CreateRotationY (rotationAngle, centerPoint) *
      Matrix4x4.CreateRotationZ (rotationAngle, centerPoint)

   Array.zeroCreate rockCount
   |> Array.mapi (fun idx _ -> createModelMatrix idx)

[<EntryPoint>]
let main argv =
   let serilogLogger = 
      (new LoggerConfiguration())
         .Enrich.FromLogContext()
         .MinimumLevel.Verbose()
         .WriteTo.Console()
         .CreateLogger();

   let microsoftLogger = 
      (new SerilogLoggerFactory(serilogLogger)).CreateLogger("GlobalCategory");

   // No need to get the current state since this is executed
   // before starting the game loop, so using the initial state 
   // is just fine.
   let glWindowOptions = 
      { GlWindowOptions.Default with
         IsVsync = false
         Title = initialState.Window.Title
         Logger = Some microsoftLogger
         Size = initialRes }
         
   let mutable fallbackGlTexture = Unchecked.defaultof<_>
   let mutable mdlPlanet = Unchecked.defaultof<_>
   let mutable mdlRock = Unchecked.defaultof<_>

   let mutable shader = Unchecked.defaultof<_>
   let mutable shaderInstanced = Unchecked.defaultof<_>

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
      shader <-
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

      shaderInstanced <-
         GlProg.emptyBuilder
         |> GlProg.withName "3dShaderInstanced"
         |> GlProg.withShaders [
            ShaderType.VertexShader, "3dInstanced.vert"
            ShaderType.FragmentShader, "3d.frag" 
         ]
         |> GlProg.withUniforms [
            "uMaterial.diffuseMap"
            "uMaterial.specularMap"
            "uMaterial.shininess"
         ]
         |> GlProg.build ctx
           
      ctx
      |> Baseline.bindShaderToUbo shader matricesUboDef state dispatch
      |> Baseline.bindShaderToUbo shaderInstanced matricesUboDef state dispatch
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

      mdlPlanet <- 
         Model.loadU (makeMdlPath ["Planet"; "planet.obj"]) ctx loadTexture  

      mdlRock <- 
         Model.loadU (makeMdlPath ["Rock"; "rock.obj"]) ctx loadTexture       

      // Configure instanced model matrix array for the rock
      // -------------------------
      
      // Set transformation matrices as an instance vertex attribute (with 
      // divisor 1)
      // NOTE: we're cheating a little by taking the, now publicly declared, 
      // VAO of the model's mesh(es) and adding new vertexAttribPointers
      // normally you'd want to do this in a more organized fashion, but 
      // for learning purposes this will do.
      let bindInstancedAttr (instancedVboHandle: uint32) (mesh: Mesh.Mesh) =
         GlVao.bind (mesh.Vao, ctx)
         |> ignore

         let mat4Stride = uint32 sizeof<Matrix4x4>
         let vapType = VertexAttribPointerType.Float

         let row1offset = IntPtr.Zero.ToPointer()
         let row2offset = 
            nativeint sizeof<Vector4>
            |> NativePtr.ofNativeInt<Vector4>
            |> NativePtr.toVoidPtr

         let row3offset = 
            nativeint (sizeof<Vector4> * 2)
            |> NativePtr.ofNativeInt<Vector4>
            |> NativePtr.toVoidPtr

         let row4offset = 
            nativeint (sizeof<Vector4> * 3)
            |> NativePtr.ofNativeInt<Vector4>
            |> NativePtr.toVoidPtr

         // Set attribute pointers for matrix (4 times vec4)
         ctx
         |> glEnableVertexAttribArray 3ul 
         |> glVertexAttribPointer 3ul 4 vapType false mat4Stride row1offset

         |> glEnableVertexAttribArray 4ul
         |> glVertexAttribPointer 4ul 4 vapType false mat4Stride row2offset

         |> glEnableVertexAttribArray 5ul
         |> glVertexAttribPointer 5ul 4 vapType false mat4Stride row3offset

         |> glEnableVertexAttribArray 6ul
         |> glVertexAttribPointer 6ul 4 vapType false mat4Stride row4offset

         |> glVertexAttribDivisor 3ul 1ul
         |> glVertexAttribDivisor 4ul 1ul
         |> glVertexAttribDivisor 5ul 1ul
         |> glVertexAttribDivisor 6ul 1ul
         |> glBindVertexArray 0ul
         |> ignore
         
         ctx.Gl.BindBuffer (BufferTargetARB.ArrayBuffer, instancedVboHandle)
         
      let matricesArraySize = 
         rockModelMatrices.Length * sizeof<Matrix4x4>

      let matricesArraySizeNativeint = 
         unativeint matricesArraySize

      use rockModelMatricesFixed = fixed rockModelMatrices
      let instancedRockModelMatricesVbo = glGenBuffer ctx
         
      ctx
      |> glBindBuffer 
         BufferTargetARB.ArrayBuffer
         instancedRockModelMatricesVbo
      |> glBufferData
            BufferTargetARB.ArrayBuffer
            (unativeint (rockModelMatrices.Length * sizeof<Matrix4x4>))
            (NativePtr.toVoidPtr rockModelMatricesFixed)
            BufferUsageARB.StaticDraw
      |> ignore

      mdlRock.Meshes
      |> Array.iter (bindInstancedAttr instancedRockModelMatricesVbo)
                        
      dispatch (Camera (ForcePosition (v3 -20.01f 8.27f -10.18f)))
      dispatch (Camera (ForceTarget (v3 0.90f -0.39f 0.19f)))

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
      |> Baseline.glCreateQueuedTextures
      |> ignore

   let onRender (ctx: GlWindowCtx) state dispatch (DeltaTime deltaTime) =
      ctx.Gl.Enable GLEnum.DepthTest

      // Needs to be Lequal instead of Less, so that the z-depth trick works
      // for the skybox, and it gets drawn behind everything even though
      // it´s rendered last.
      ctx.Gl.DepthFunc GLEnum.Less

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      |> ctx.Gl.Clear
              
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

      let setUboUniformM4 =  Baseline.setUboUniformM4 state ctx
      setUboUniformM4 matricesUboDef "uProjection" projectionMatrix
      setUboUniformM4 matricesUboDef "uView" viewMatrix
        
      // **********************************************************************
            
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
      |> GlProg.setUniformM4x4 "uModel" Matrix4x4.Identity
      |> ignore
      Model.draw mdlPlanet shader getTextureHandler ctx
      
      (shaderInstanced, ctx)
      |> GlProg.setAsCurrent
      |> ignore
      
      Model.drawInstanced 
         mdlRock 
         shaderInstanced 
         getTextureHandler 
         (uint32 rockModelMatrices.Length) 
         ctx

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