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
open Galante

let initialState = 
   BaselineState.createDefault(
      "27_Multiple_Lights", 
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
   let mutable shaderLighted = Unchecked.defaultof<_>
   let mutable shaderLightSource = Unchecked.defaultof<_>
   let mutable containerDiffuseMapTex = Unchecked.defaultof<_>
   let mutable containerSpecularMapTex = Unchecked.defaultof<_>
             
   let ptLightPositions = [
      new Vector3(0.7f, 0.2f, 2.0f)
      new Vector3(2.3f, -3.3f, -4.0f)
      new Vector3(-4.0f, 2.0f, -12.0f)
      new Vector3(0.0f, 0.0f, -3.0f)
   ]

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
      shaderLighted <-
         GlProg.emptyBuilder
         |> GlProg.withName "Lighted"
         |> GlProg.withShaders 
               [ ShaderType.VertexShader, @"Basic3d.vert"
               ; ShaderType.FragmentShader, @"Lighted.frag" 
               ;]
         |> GlProg.withUniforms [
               "uModel"
               "uView"
               "uProjection"
               "uViewerPos"
               "uMaterial.diffuseMap"
               "uMaterial.specularMap"
               "uMaterial.shininess"
                
               "uDirectionalLight.direction"
               "uDirectionalLight.ambientColor"
               "uDirectionalLight.diffuseColor"
               "uDirectionalLight.specularColor"
                
               "uPointLights[0].position"
               "uPointLights[0].ambientColor"
               "uPointLights[0].diffuseColor"
               "uPointLights[0].specularColor"
               "uPointLights[0].constantComponent"
               "uPointLights[0].linearComponent"
               "uPointLights[0].quadraticComponent"
                
               "uPointLights[1].position"
               "uPointLights[1].ambientColor"
               "uPointLights[1].diffuseColor"
               "uPointLights[1].specularColor"
               "uPointLights[1].constantComponent"
               "uPointLights[1].linearComponent"
               "uPointLights[1].quadraticComponent"
                
               "uPointLights[2].position"
               "uPointLights[2].ambientColor"
               "uPointLights[2].diffuseColor"
               "uPointLights[2].specularColor"
               "uPointLights[2].constantComponent"
               "uPointLights[2].linearComponent"
               "uPointLights[2].quadraticComponent"
                
               "uPointLights[3].position"
               "uPointLights[3].ambientColor"
               "uPointLights[3].diffuseColor"
               "uPointLights[3].specularColor"
               "uPointLights[3].constantComponent"
               "uPointLights[3].linearComponent"
               "uPointLights[3].quadraticComponent"

               "uSpotLight.position"
               "uSpotLight.direction"
               "uSpotLight.innerCutOffAngleCos"
               "uSpotLight.outerCutOffAngleCos"
               "uSpotLight.ambientColor"
               "uSpotLight.diffuseColor"
               "uSpotLight.specularColor"
               "uSpotLight.constantComponent"
               "uSpotLight.linearComponent"
               "uSpotLight.quadraticComponent"
         ]
         |> GlProg.build ctx
        
      shaderLightSource <-
         GlProg.emptyBuilder
         |> GlProg.withName "LightSource"
         |> GlProg.withShaders [
               ShaderType.VertexShader, "Basic3d.vert"
               ShaderType.FragmentShader, "LightSource.frag"
         ]
         |> GlProg.withUniforms [
               "uModel"
               "uView" 
               "uProjection"
               "uEmittedLightColor"
         ]
         |> GlProg.build ctx

      cubeVao <-
         GlVao.create ctx
         |> GlVao.bind
         |> fun (vao, _) -> vao

      let cubeVbo =
         GlVbo.emptyVboBuilder
         |> GlVbo.withAttrNames ["Positions"; "Normals"; "Texture coords"]
         |> GlVbo.withAttrDefinitions 
               Cube.vertexPositionsAndNormalsAndTextureCoords
         |> GlVbo.build (cubeVao, ctx)
            
      containerDiffuseMapTex <- 
         GlTex.loadImage "container2.png" ctx
         |> fun img -> GlTex.create2d img ctx

      containerSpecularMapTex <- 
         GlTex.loadImage "container2_specular.png" ctx
         |> fun img -> GlTex.create2d img ctx
                                    
      // Hardcoded camera position and target, so it looks just like the
      // LearnOpenGL.com example right away.
      dispatch (Camera (ForcePosition (new Vector3(-1.74f, -1.51f, 1.86f))))
      dispatch (Camera (ForceTarget (new Vector3(0.56f, 0.53f, -0.62f))))

      // Comment this or press F10 to unlock the camera
      //dispatch (Mouse UseCursorNormal)
      //dispatch (Camera Lock)

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

      uint32 (GLEnum.ColorBufferBit ||| GLEnum.DepthBufferBit)
      |> ctx.Gl.Clear
        
      // Sets a dark grey background so the cube´s color changes are visible
      ctx.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f)
        
      let time = single ctx.Window.Time
      let lightSrcPathRadius = 2.0f
      let lightSrcPathCurrX = MathF.Sin(time) * lightSrcPathRadius
      let lightSrcPathCurrZ = MathF.Cos(time) * lightSrcPathRadius

      let lightSrcPosition = 
         new Vector3(lightSrcPathCurrX, 1.3f, lightSrcPathCurrZ)

      let viewMatrix = BaseCameraSlice.createViewMatrix state.Camera

      let res = state.Window.Resolution
      let fov = Radians.value <| toRadians(state.Camera.Fov)
      let ratio = single(res.Width) / single(res.Height)
      let projectionMatrix = 
         Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, 0.1f, 100.0f)
       
      let materialShininess = 32.0f

      let lightColor = new Vector3(1.0f)
            
      let lightDiffuseColor = lightColor * new Vector3(0.5f)
      let lightAmbientColor = lightDiffuseColor * new Vector3(0.2f)
      let lightSpecularColor = new Vector3(1.0f, 1.0f, 1.0f)

      (cubeVao, ctx)
      |> GlTex.setActive GLEnum.Texture0 containerDiffuseMapTex
      |> GlTex.setActive GLEnum.Texture1 containerSpecularMapTex
      |> ignore
        
      let outerCutOffCos = 
         Degrees.make 18.0f
         |> cosF

      let innerCutOffCos = 
         Degrees.make 12.0f
         |> cosF
                    
      let attenuationKc = 1.0f
      let attenuationKl = 0.09f
      let attenuationKq = 0.032f

      // Prepares the shader
      (shaderLighted, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> GlProg.setUniformV3 "uViewerPos" state.Camera.Position
      |> GlProg.setUniformI "uMaterial.diffuseMap" 0
      |> GlProg.setUniformI "uMaterial.specularMap" 1
      |> GlProg.setUniformF "uMaterial.shininess" materialShininess

      // Directional light
      |> GlProg.setUniformV3 "uDirectionalLight.direction" (new Vector3(-0.2f, -1.0f, -0.3f))
      |> GlProg.setUniformV3 "uDirectionalLight.ambientColor" (new Vector3(0.05f, 0.05f, 0.05f))
      |> GlProg.setUniformV3 "uDirectionalLight.diffuseColor" (new Vector3(0.4f, 0.4f, 0.4f))
      |> GlProg.setUniformV3 "uDirectionalLight.specularColor" (new Vector3(0.5f, 0.5f, 0.5f))

      // Point lights
      |> GlProg.setUniformV3 "uPointLights[0].position" ptLightPositions.[0]
      |> GlProg.setUniformV3 "uPointLights[0].ambientColor" (new Vector3(0.05f, 0.05f, 0.05f))
      |> GlProg.setUniformV3 "uPointLights[0].diffuseColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformV3 "uPointLights[0].specularColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformF "uPointLights[0].constantComponent" attenuationKc
      |> GlProg.setUniformF "uPointLights[0].linearComponent" attenuationKl
      |> GlProg.setUniformF "uPointLights[0].quadraticComponent" attenuationKq
        
      |> GlProg.setUniformV3 "uPointLights[1].position" ptLightPositions.[1]
      |> GlProg.setUniformV3 "uPointLights[1].ambientColor" (new Vector3(0.05f, 0.05f, 0.05f))
      |> GlProg.setUniformV3 "uPointLights[1].diffuseColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformV3 "uPointLights[1].specularColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformF "uPointLights[1].constantComponent" attenuationKc
      |> GlProg.setUniformF "uPointLights[1].linearComponent" attenuationKl
      |> GlProg.setUniformF "uPointLights[1].quadraticComponent" attenuationKq
        
      |> GlProg.setUniformV3 "uPointLights[2].position" ptLightPositions.[2]
      |> GlProg.setUniformV3 "uPointLights[2].ambientColor" (new Vector3(0.05f, 0.05f, 0.05f))
      |> GlProg.setUniformV3 "uPointLights[2].diffuseColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformV3 "uPointLights[2].specularColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformF "uPointLights[2].constantComponent" attenuationKc
      |> GlProg.setUniformF "uPointLights[2].linearComponent" attenuationKl
      |> GlProg.setUniformF "uPointLights[2].quadraticComponent" attenuationKq
        
      |> GlProg.setUniformV3 "uPointLights[3].position" ptLightPositions.[3]
      |> GlProg.setUniformV3 "uPointLights[3].ambientColor" (new Vector3(0.05f, 0.05f, 0.05f))
      |> GlProg.setUniformV3 "uPointLights[3].diffuseColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformV3 "uPointLights[3].specularColor" (new Vector3(0.8f, 0.8f, 0.8f))
      |> GlProg.setUniformF "uPointLights[3].constantComponent" attenuationKc
      |> GlProg.setUniformF "uPointLights[3].linearComponent" attenuationKl
      |> GlProg.setUniformF "uPointLights[3].quadraticComponent" attenuationKq

      // Spotlight
      |> GlProg.setUniformV3 "uSpotLight.position" state.Camera.Position
      |> GlProg.setUniformV3 "uSpotLight.direction" state.Camera.TargetDirection
      |> GlProg.setUniformF "uSpotLight.innerCutOffAngleCos" innerCutOffCos
      |> GlProg.setUniformF "uSpotLight.outerCutOffAngleCos" outerCutOffCos
      |> GlProg.setUniformF "uSpotLight.constantComponent" 1.0f
      |> GlProg.setUniformF "uSpotLight.linearComponent" 0.09f
      |> GlProg.setUniformF "uSpotLight.quadraticComponent" 0.032f
      |> GlProg.setUniformV3 "uSpotLight.ambientColor" lightAmbientColor
      |> GlProg.setUniformV3 "uSpotLight.diffuseColor" lightDiffuseColor
      |> GlProg.setUniformV3 "uSpotLight.specularColor" lightSpecularColor
      |> ignore
        
      // Draw each container box
      Cube.transformations
      |> Seq.iter 
         (fun transformation ->
               let modelMatrix =
                  Matrix4x4.CreateRotationX transformation.RotationX
                  * Matrix4x4.CreateRotationY transformation.RotationY
                  * Matrix4x4.CreateRotationZ transformation.RotationZ
                  * Matrix4x4.CreateTranslation transformation.Translation

               GlProg.setUniformM4x4 "uModel" modelMatrix (shaderLighted, ctx)
               |> ignore    

               ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)
         )
            
      (shaderLightSource, ctx)
      |> GlProg.setAsCurrent
      |> GlProg.setUniformM4x4 "uView" viewMatrix
      |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
      |> GlProg.setUniformV3 "uEmittedLightColor" lightColor
      |> ignore

      // Draw each point light source
      ptLightPositions
      |> Seq.iter 
         (fun pos ->
                
               let lightSourceModelMatrix =
                  Matrix4x4.CreateTranslation (pos / 0.2f)
                  * Matrix4x4.CreateScale (new Vector3(0.2f))

               (shaderLightSource, ctx)
               |> GlProg.setUniformM4x4 "uModel" lightSourceModelMatrix 
               |> ignore
                
               ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 36ul)
         )

      dispatch (FpsCounter(FrameRenderCompleted deltaTime))
      ()

   let onInputContextLoaded ctx ic state dispatch = 
      dispatch (Window (InitializeInputContext ic))

   let onWindowResize ctx state dispatch newSize =
      (ctx, state, dispatch, newSize)
      |> Baseline.handleWindowResize
      |> ignore

   let addListener =
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
      |> buildAndRun
   0