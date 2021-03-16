namespace Galante
    open System.Drawing
    open Silk.NET.Input
    open System.Numerics
    open Silk.NET.Windowing
    open Galante.OpenGL

    type GameContext<'TGameState, 'TGameAction> =
        { WindowContext: GlWindowContext
        ;}
    