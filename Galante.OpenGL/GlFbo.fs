module GlFbo

open Galante.OpenGL
open Silk.NET.OpenGL
open GlTex
open Gl
open System
open System.Numerics

let fboCreate ctx = 
   ctx.Gl.GenFramebuffer ()
   |> fun handle -> {
      GlFrameBufferObject.GlFboHandle = handle
      ColorAttachment = None
      DepthStencilAttachment = None
      DepthTexture = None
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
   
let fboAttachEmptyDepthTexture2d (width: int) (height: int) (fbo, ctx) =
   let texture =
      emptyImagelessGlTexture
      |> imagelessWithTextureTarget TextureTarget.Texture2D
      |> imagelessWithFormat PixelFormat.DepthComponent
      |> imagelessWithInternalFormat 
         (LanguagePrimitives.EnumToValue GLEnum.DepthComponent)
      |> imagelessWithWrapModeS GLEnum.ClampToBorder
      |> imagelessWithWrapModeT GLEnum.ClampToBorder
      |> imagelessWithTextureMinFilter GLEnum.Nearest
      |> imagelessWithTextureMagFilter GLEnum.Nearest
      |> imagelessWithWidth width
      |> imagelessWithHeight height
      |> buildImagelessGlTexture ctx

   glBindTexture TextureTarget.Texture2D texture.GlTexHandle ctx |> ignore
   glTexParameterBorderColor 
      ctx 
      TextureTarget.Texture2D 
      TextureParameterName.TextureBorderColor
      (new Vector4 (1.0f, 1.0f, 1.0f, 1.0f))

   (fbo, ctx)
   |> fboBind
   |> fun (_, ctx) -> 
      glFramebufferTexture2d
         FramebufferTarget.Framebuffer
         FramebufferAttachment.DepthAttachment
         texture.TextureTarget
         texture.GlTexHandle
         0
         ctx
   |> glDrawBuffer DrawBufferMode.None
   |> glReadBuffer ReadBufferMode.None
   |> fboBindDefault
   |> ignore
   
   let updatedFbo = { fbo with DepthTexture = Some texture }
   (updatedFbo, ctx)
   
let fboAttachEmptyDepthCubemap (width: int) (height: int) (fbo, ctx) =
   let cubemapTex =
      emptyImagelessGlTexture
      |> imagelessWithTextureTarget TextureTarget.TextureCubeMap
      |> imagelessWithFormat PixelFormat.DepthComponent
      |> imagelessWithInternalFormat (LanguagePrimitives.EnumToValue GLEnum.DepthComponent)
      |> imagelessWithWrapModeS GLEnum.ClampToEdge
      |> imagelessWithWrapModeT GLEnum.ClampToEdge
      |> imagelessWithTextureMinFilter GLEnum.Nearest
      |> imagelessWithTextureMagFilter GLEnum.Nearest
      |> imagelessWithWidth width
      |> imagelessWithHeight height
      |> buildImagelessGlTextureCubemap ctx

   glBindTexture TextureTarget.TextureCubeMap cubemapTex.GlTexHandle ctx |> ignore

   //glTexParameterBorderColor 
   //   ctx 
   //   TextureTarget.Texture2D 
   //   TextureParameterName.TextureBorderColor
   //   (new Vector4 (1.0f, 1.0f, 1.0f, 1.0f))

   (fbo, ctx)
   |> fboBind
   |> fun (_, ctx) -> 
      glFramebufferTexture
         FramebufferTarget.Framebuffer
         FramebufferAttachment.DepthAttachment
         cubemapTex.GlTexHandle
         0
         ctx
   |> glDrawBuffer DrawBufferMode.None
   |> glReadBuffer ReadBufferMode.None
   |> fboBindDefault
   |> ignore
   
   let updatedFbo = { fbo with DepthTexture = Some cubemapTex }
   (updatedFbo, ctx)

let fboDestroy fbo ctx =
   ctx.Gl.DeleteFramebuffer fbo.GlFboHandle

let fboCreateStandard w h ctx =
   fboCreate ctx
   |> fboBind
   |> fboAttachEmptyColorTexture2d w h
   |> fboAttachRenderBuffer w h
   |> fboValidateStatus

let fboCreateWithDepthTextureOnly w h ctx =
   fboCreate ctx
   |> fboBind
   |> fboAttachEmptyDepthTexture2d w h
   |> fboValidateStatus

let fboCreateWithDepthTextureOnlyCubemap w h ctx =
   fboCreate ctx
   |> fboBind
   |> fboAttachEmptyDepthCubemap w h
   |> fboValidateStatus