module GlFbo

open Galante.OpenGL
open Silk.NET.OpenGL
open GlTex

let fboCreate ctx = 
   ctx.Gl.GenFramebuffer ()
   |> fun handle -> {
      GlFrameBufferObject.GlFboHandle = handle
      ColorAttachment = None
      DepthStencilAttachment = None
   }
   |> fun fbo -> (fbo, ctx)

let fboBindDefault ctx =
   ctx.Gl.BindFramebuffer (FramebufferTarget.Framebuffer, 0ul)
   ctx

let fboValidateStatus (fbo, ctx) =
   ctx.Gl.CheckFramebufferStatus (FramebufferTarget.Framebuffer)
   |> function
      | GLEnum.FramebufferComplete -> (fbo, ctx)
      | _ -> failwith "The framebuffer's status is not 'complete'"

let fboBind (fbo, ctx) = 
   ctx.Gl.BindFramebuffer (FramebufferTarget.Framebuffer, fbo.GlFboHandle)
   (fbo, ctx)

let fboAttachEmptyColorTexture2d width height (fbo, ctx) =
   let texture: GlEmptyTexture = texCreateEmpty2d width height ctx
   ctx.Gl.FramebufferTexture2D (
      FramebufferTarget.Framebuffer, 
      FramebufferAttachment.ColorAttachment0, 
      TextureTarget.Texture2D,
      texture.GlTexHandle,
      0)

   let updatedFbo = { fbo with ColorAttachment = Some texture }

   (updatedFbo, ctx)

let fboAttachRenderBuffer width height (fbo, ctx) =
   let rbo = 
      GlRbo.rboCreate ctx
      |> GlRbo.rboBind
      |> GlRbo.rboSetDepthStencilStorage width height
      |> fun (rbo, _) -> rbo

   ctx.Gl.FramebufferRenderbuffer (
      FramebufferTarget.Framebuffer, 
      GLEnum.DepthStencilAttachment, 
      RenderbufferTarget.Renderbuffer,
      rbo.GlRboHandle)
      
   let updatedFbo = { fbo with DepthStencilAttachment = Some rbo }

   (updatedFbo, ctx)

let fboDestroy fbo ctx =
   ctx.Gl.DeleteFramebuffer fbo.GlFboHandle

let fboCreateStandard w h ctx =
   fboCreate ctx
   |> fboBind
   |> fboAttachEmptyColorTexture2d w h
   |> fboAttachRenderBuffer w h
   |> fboValidateStatus