namespace Galante

open Silk.NET.OpenGL
open Microsoft.Extensions.Logging
open Microsoft.Extensions.FileProviders

type GlContext =
    { Gl: GL
    ; FileProvider: IFileProvider
    ; Logger: ILogger
    ;}