namespace Galante

type GlShaderStatus = 
    | GlShaderCompiled
    | GlShaderCompilationError of string
    | GlShaderAttached
    | GlShaderDeleted