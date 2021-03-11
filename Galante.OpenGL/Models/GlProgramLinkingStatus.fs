namespace Galante.OpenGL

open Silk.NET.OpenGL
open Microsoft.Extensions.Logging
open Microsoft.Extensions.FileProviders

type GlProgramLinkingStatus =
    | GlProgramLinkingPending
    | GlProgramLinkingSuccesful
    | GlProgramLinkingError of string