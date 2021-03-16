namespace Galante.OpenGL

open Microsoft.Extensions.Logging
open Microsoft.Extensions.FileProviders
open Silk.NET.OpenGL
open Silk.NET.Windowing
open Silk.NET.Input

type GlWindowContext = 
    { Gl: GL
    ; Window: IWindow
    ; FileProvider: IFileProvider
    ; Logger: ILogger
    ;}