namespace Galante

open Silk.NET.OpenGL
open Microsoft.Extensions.Logging
open Microsoft.Extensions.FileProviders

type GlShader =
    { GlShaderHandle: uint32
    ; Type: ShaderType
    ; SourceFilePath: string
    ; GlStatus: GlShaderStatus
    ;}