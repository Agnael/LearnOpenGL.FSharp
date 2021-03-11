namespace Galante.OpenGL

type GlShaderStatus = 
    | GlShaderCompiled
    | GlShaderCompilationError of string
    | GlShaderAttached
    | GlShaderDeleted