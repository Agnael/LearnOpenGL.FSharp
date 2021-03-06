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
            Title = "07_Transformations"
            Size = new Size (600, 600) }

    let ctx = GlWin.create glOpts

    let mutable quadVao = Unchecked.defaultof<_>
    let mutable shader = Unchecked.defaultof<_>
    let mutable texture1 = Unchecked.defaultof<_>
    let mutable texture2 = Unchecked.defaultof<_>
    let mutable timer = 0.0f
        
    let onLoad () = 
        shader <-
            GlProg.emptyBuilder
            |> GlProg.withName "DoubleTexture"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"DoubleTextureWithTransformer.vert"
                ; ShaderType.FragmentShader, @"DoubleTexture.frag" 
                ;]
            |> GlProg.withUniforms ["texture1"; "texture2"; "uTransformation"]
            |> GlProg.build ctx

        quadVao <-
            GlVao.create ctx
            |> GlVao.bind
            |> fun (vao, _) -> vao

        let quadVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames ["Positions"; "Texture coords"]
            |> GlVbo.withAttrDefinitions [|
                [| [| -0.9f; 0.9f; 0.0f |]; [| 0.0f; 1.0f |] |]
                [| [| -0.3f; 0.9f; 0.0f |]; [| 1.0f; 1.0f |] |]
                [| [| -0.9f; 0.3f; 0.0f |]; [| 0.0f; 0.0f |] |]
                [| [| -0.3f; 0.3f; 0.0f |]; [| 1.0f; 0.0f |] |]
            |]
            |> GlVbo.build (quadVao, ctx)
            
        texture1 <- GlTex.create2D @"wall.jpg" ctx
        texture2 <- GlTex.create2D @"awesomeface.png" ctx

        // Define en qué orden se van a dibujar los 2 triángulos que forman el cuadrilátero
        let quadEbo = GlEbo.create ctx [| 0ul; 1ul; 2ul; 2ul; 1ul; 3ul; |]
        ()

    let onUpdate dt =
        timer <- timer + float32(dt) / 2.0f
        ()

    let onRender dt =
        ctx.Gl.Clear <| uint32 GLEnum.ColorBufferBit
        
        (quadVao, ctx)
        |> GlTex.setActive GLEnum.Texture0 texture1
        |> GlTex.setActive GLEnum.Texture1 texture2
        |> ignore
        
        // Prepares shader and draws the original image
        (shader, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformI "texture1" 0
        |> GlProg.setUniformI "texture2" 1
        |> GlProg.setUniformM4x4 "uTransformation" Matrix4x4.Identity
        |> ignore

        GlVao.bind (quadVao, ctx) |> ignore
        ctx.Gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())

        // Applies the 1st transformation matrix to the same shader and draws the same VAO again
        let rotationRadians: float32 = timer
        let transformation1 = 
            Matrix4x4.Identity * 
            // We know each SIDE of the quad is 0.3uv long, and we know its vertices,
            // this allows us to calculate it's center, translate that center to the origin,
            // perform the rotation and translate it back.
            // CenterX = (-0.9 - (-0.3)) / 2 + (-0.3)   = -0.6 => To translate to origin: +0.6
            // CenterY = (0.9 - 0.3) / 2 + (0.3)        = 0.6  => To translate to origin: -0.6
            Matrix4x4.CreateTranslation (new Vector3(+0.6f, -0.6f, 0.0f)) * 
            Matrix4x4.CreateRotationZ rotationRadians *
            Matrix4x4.CreateTranslation (new Vector3(-0.6f, +0.6f, 0.0f)) *
            // This last translation is to move it aside from the previous drawing so it is distinguishable
            Matrix4x4.CreateTranslation (new Vector3(0.6f, 0.0f, 0.0f))

        GlProg.setUniformM4x4 "uTransformation" transformation1 (shader, ctx) |> ignore        
        GlVao.bind (quadVao, ctx) |> ignore
        ctx.Gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())

        // Applies a 2nd transformation and draws the same VAO again
        let ninetyDegreeRadians = 90.0f * MathF.PI / 180.0f
        let transformation2 = 
            Matrix4x4.Identity * 
            Matrix4x4.CreateTranslation (new Vector3(+0.6f, -0.6f, 0.0f)) * 
            Matrix4x4.CreateRotationZ ninetyDegreeRadians *
            Matrix4x4.CreateTranslation (new Vector3(-0.6f, +0.6f, 0.0f)) *
            Matrix4x4.CreateTranslation (new Vector3(1.2f, 0.0f, 0.0f))
                
        GlProg.setUniformM4x4 "uTransformation" transformation2 (shader, ctx) |> ignore        
        GlVao.bind (quadVao, ctx) |> ignore
        ctx.Gl.DrawElements (GLEnum.Triangles, 6ul, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
        ()
    
    ctx.Window.add_Update (new Action<float>(onUpdate))
    ctx.Window.add_Load (new Action(onLoad))
    ctx.Window.add_Render (new Action<float>(onRender))
    ctx.Window.Run ()
    0