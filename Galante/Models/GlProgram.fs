namespace Galante

type GlProgram =
    { GlProgramHandle: uint32 
    ; Name: string
    ; Shaders: GlShader list
    ; LinkingStatus: GlProgramLinkingStatus
    ; Uniforms: GlProgramUniform list
    ;}