namespace Galante.OpenGL

type GlFrameBufferObject = 
    { GlFboHandle: uint32 
    ; ColorAttachment: GlEmptyTexture option
    ; DepthStencilAttachment: GlRenderBufferObject option
    ;}