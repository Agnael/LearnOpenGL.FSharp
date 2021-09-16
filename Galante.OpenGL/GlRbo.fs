module GlRbo

open Galante.OpenGL
open Silk.NET.OpenGL
open GlTex

let rboCreate ctx = 
   ctx.Gl.GenRenderbuffer ()
   |> fun handle -> { GlRenderBufferObject.GlRboHandle = handle }
   |> fun rbo -> (rbo, ctx)

let rboBind (rbo, ctx) =
   ctx.Gl.BindRenderbuffer (RenderbufferTarget.Renderbuffer, rbo.GlRboHandle)
   (rbo, ctx)

let rboSetDepthStencilStorage width height (rbo, ctx) =
   rboBind (rbo, ctx) |> ignore

   ctx.Gl.RenderbufferStorage (
      GLEnum.Renderbuffer, 
      GLEnum.Depth24Stencil8, 
      uint32 width, 
      uint32 height)

   (rbo, ctx)