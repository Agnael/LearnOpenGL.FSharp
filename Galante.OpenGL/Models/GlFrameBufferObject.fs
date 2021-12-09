namespace Galante.OpenGL

type GlFrameBufferObject = 
    { GlFboHandle: uint32 
    ; ColorAttachment: GlEmptyTexture option
    ; DepthStencilAttachment: GlRenderBufferObject option

    // This object is ugly af because i didn´t understand that we'd need to use
    // textures for depth in the future, since renderbuffers are great for
    // writes only, and textures are more suitable for reading, according to
    // google. That explains why the tutorials switched to using textures for
    // shadow maps.
    ; DepthTexture: GlTexture option
    ;}