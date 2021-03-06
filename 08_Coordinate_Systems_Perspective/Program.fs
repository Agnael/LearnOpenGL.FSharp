﻿open System
open Galante.OpenGL
open Silk.NET.Windowing
open Silk.NET.OpenGL
open System.Drawing
open System.Numerics

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "08_Coordinate_Systems_Perspective"
            Size = new Size (800, 600) }

    let ctx = GlWin.create glOpts

    let mutable quadVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
        
    let onLoad () = 
        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "3dShader"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Textured3d.vert"
                ; ShaderType.FragmentShader, @"DoubleTexture.frag" 
                ;]
            |> GlProg.withUniforms [
                "texture1"; 
                "texture2"; 
                "uModel"; 
                "uView"; 
                "uProjection"]
            |> GlProg.build ctx

        quadVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let quadVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
            |> GlVbo.withAttrDefinitions [|
                [| [| -0.5f;  0.5f; 0.0f |]; [| 0.0f; 1.0f |] |]
                [| [|  0.5f;  0.5f; 0.0f |]; [| 1.0f; 1.0f |] |]
                [| [| -0.5f; -0.5f; 0.0f |]; [| 0.0f; 0.0f |] |]
                [| [|  0.5f; -0.5f; 0.0f |]; [| 1.0f; 0.0f |] |]
            |]
            |> GlVbo.build (quadVao, ctx)
            
        texture1 <- GlTex.create2D @"wall.jpg" ctx
        texture2 <- GlTex.create2D @"awesomeface.png" ctx

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onUpdate dt = ()
        
    let toRadians degrees = degrees * MathF.PI / 180.0f
    let fov = toRadians 45.0f
    let aspectRatio = 800.0f/600.0f

    let onRender dt =
        ctx.Gl.Clear <| uint32 GLEnum.ColorBufferBit

        (quadVao, ctx)
        |> GlTex.setActive GLEnum.Texture0 texture1
        |> GlTex.setActive GLEnum.Texture1 texture2
        |> ignore

        let modelMatrix =
            Matrix4x4.Identity *
            Matrix4x4.CreateRotationX(toRadians -55.0f) *
            Matrix4x4.CreateRotationY(toRadians 40.0f)

        let cameraDistance =
            float32 <| Math.Sin(ctx.Window.Time * 3.0) + 3.0
            
        // Note that we're translating the scene in the reverse 
        // direction of where we want to move.
        let viewMatrix = 
            Matrix4x4.Identity * 
            Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, -cameraDistance))

        let projectionMatrix = 
            Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, 0.1f, 100.0f)
       
        // Prepares shader and draws the original image
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformI "texture1" 0
        |> GlProg.setUniformI "texture2" 1
        |> GlProg.setUniformM4x4 "uModel" modelMatrix
        |> GlProg.setUniformM4x4 "uView" viewMatrix
        |> GlProg.setUniformM4x4 "uProjection" projectionMatrix
        |> ignore
    
        GlVao.bind (quadVao, ctx) |> ignore
        ctx.Gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
        ()
    
    ctx.Window.add_Update (new Action<float>(onUpdate))
    ctx.Window.add_Load (new Action(onLoad))
    ctx.Window.add_Render (new Action<float>(onRender))
    ctx.Window.Run ()
    0
