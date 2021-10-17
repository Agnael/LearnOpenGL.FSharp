namespace Galante
    open Galante.OpenGL
    open Silk.NET.Input
    open System.Numerics
    open StoreFactory

    type Dispatch<'TGameAction> = 'TGameAction -> unit

    type DeltaTime =
        | DeltaTime of double
        static member value (DeltaTime v) = v
        static member make v = DeltaTime(v)
    
    type private onInputContextLoadedHandler<'gs,'ga> =
        GlWindowCtx -> IInputContext -> 'gs -> Dispatch<'ga> -> unit

    type private onLoadHandler<'gs, 'ga> =
        GlWindowCtx -> IInputContext -> 'gs -> Dispatch<'ga> -> unit

    type private onUpdateHandler<'gs, 'ga> =
        GlWindowCtx -> 'gs -> Dispatch<'ga> -> DeltaTime -> unit

    type private onRenderHandler<'gs, 'ga> =
        GlWindowCtx -> 'gs-> Dispatch<'ga> -> DeltaTime -> unit

    type private onKeyDownHandler<'gs, 'ga> = 
        GlWindowCtx -> 'gs -> Dispatch<'ga> -> IKeyboard -> Key -> unit

    type private onKeyUpHandler<'gs, 'ga> = 
        GlWindowCtx -> 'gs -> Dispatch<'ga> -> IKeyboard -> Key -> unit

    type private onMouseMoveHandler<'gs, 'ga> =
        GlWindowCtx -> 'gs -> Dispatch<'ga> -> Vector2 -> unit

    type private onMouseWheelHandler<'gs, 'ga> = 
        GlWindowCtx -> 'gs -> Dispatch<'ga> -> Vector2 -> unit

    type private onWindowResizeHandler<'gs, 'ga> =
        GlWindowCtx -> 'gs -> Dispatch<'ga> -> Vector2 -> unit

    type private onWindowCloseHandler<'gs, 'ga> =
        GlWindowCtx -> 'gs -> Dispatch<'ga> -> unit

    type GameBuilder<'gs, 'ga> =
        { WindowOptions: GlWindowOptions
        ; InitialState: 'gs
        ; Reducer: 'ga -> 'gs -> 'gs
        ; OnInputContextLoaded: onInputContextLoadedHandler<'gs,'ga>
        ; OnLoad: onLoadHandler<'gs,'ga> list
        ; OnUpdate: onUpdateHandler<'gs,'ga> list
        ; OnRender: onRenderHandler<'gs,'ga> list
        ; OnKeyDown: onKeyDownHandler<'gs,'ga> list
        ; OnKeyUp: onKeyUpHandler<'gs,'ga> list
        ; OnMouseMove: onMouseMoveHandler<'gs,'ga> list
        ; OnMouseWheel: onMouseWheelHandler<'gs,'ga> list
        ; OnWindowResize: onWindowResizeHandler<'gs,'ga> list
        ; OnWindowClose: onWindowCloseHandler<'gs,'ga> list
        ; OnActionListen: ('gs -> 'ga -> ('ga->unit) -> GlWindowCtx -> unit) list
        ;}