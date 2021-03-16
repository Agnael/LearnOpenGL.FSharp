namespace Galante
    open Galante.OpenGL
    open Silk.NET.Input
    open System.Numerics

    //type GameBuilder<'t, 'gs, 'wo, 'l, 'u, 'r, 'kd, 'ku, 'mm, 'mw> =
    //    internal 
    //        { WindowOptions: 'wo
    //        ; InitialState: 'gs
    //        ; OnLoad: 'l
    //        ; OnUpdate: 'u
    //        ; OnRender: 'r
    //        ; OnKeyDown: 'kd
    //        ; OnKeyUp: 'ku
    //        ; OnMouseMove: 'mm
    //        ; OnMouseWheel: 'mw
    //        ;}

    type Dispatch<'TGameAction> = 'TGameAction -> unit

    type DeltaTime =
        | DeltaTime of double
        static member value (DeltaTime v) = v
        static member make v = DeltaTime(v)

    type GameBuilder<'TGameState, 'TGameAction> =
        { WindowOptions: GlWindowOptions
        ; InitialState: 'TGameState
        ; Reducer: 'TGameAction -> 'TGameState -> 'TGameState
        ; OnLoad: (GlWindowContext -> 'TGameState -> Dispatch<'TGameAction> -> unit) list
        ; OnUpdate: (GlWindowContext -> 'TGameState -> Dispatch<'TGameAction> -> DeltaTime -> unit) list
        ; OnRender: (GlWindowContext -> 'TGameState -> Dispatch<'TGameAction> -> DeltaTime -> unit) list
        ; OnKeyDown: (GlWindowContext -> 'TGameState -> Dispatch<'TGameAction> -> Key -> unit) list
        ; OnKeyUp: (GlWindowContext -> 'TGameState -> Dispatch<'TGameAction> -> Key -> unit) list
        ; OnMouseMove: (GlWindowContext -> 'TGameState -> Dispatch<'TGameAction> -> Vector2 -> unit) list
        ; OnMouseWheel: (GlWindowContext -> 'TGameState -> Dispatch<'TGameAction> -> Vector2 -> unit) list
        ;}