﻿open System
open Silk.NET.Windowing
open Silk.NET.OpenGL
open Galante.OpenGL
open System.Drawing

[<EntryPoint>]
let main argv =
    let glOpts = 
        { GlWindowOptions.Default with
            Title = "02_Triangle"
            Size = new Size (600, 600) }

    let ctx = GlWin.create glOpts

    let mutable shaderBasic = Unchecked.defaultof<_>
    let mutable triangleVao = Unchecked.defaultof<_>

    let load () =
        shaderBasic <-
            GlProg.emptyBuilder
            |> GlProg.withName "Basic"
            |> GlProg.withShaders 
                [ ShaderType.VertexShader, @"Basic.vert"
                ; ShaderType.FragmentShader, @"Basic.frag" 
                ;]
            |> GlProg.withUniforms ["myColor"]
            |> GlProg.build ctx

        triangleVao <- 
            GlVao.create ctx
            |> GlVao.bind 
            |> fun (vao, _) -> vao

        let triangleVbo =
            GlVbo.emptyVboBuilder
            |> GlVbo.withAttrNames [ "Positions"]
            |> GlVbo.withAttrDefinitions [|
                [| [|-0.5f; -0.5f;  0.0f;|] |]
                [| [| 0.5f; -0.5f;  0.0f;|] |]
                [| [| 0.00f; 0.5f;  0.0f;|] |] 
            |]
            |> GlVbo.build (triangleVao, ctx)
        ()

    let render (dt: float) =
        ctx.Gl.Clear <| uint32 GLEnum.ColorBufferBit
        
        let greenValue = single <| Math.Sin(ctx.Window.Time);

        (shaderBasic, ctx)
        |> GlProg.setAsCurrent
        |> GlProg.setUniformV4 "myColor" (0.8f, greenValue, 0.0f, 1.0f)
        |> ignore
        ctx.Gl.BindVertexArray triangleVao.GlVaoHandle
        ctx.Gl.DrawArrays (GLEnum.Triangles, 0, 3u) 

    ctx.Window.add_Load (new Action(load))
    ctx.Window.add_Render (new Action<_>(render))
    ctx.Window.Run ()
    0