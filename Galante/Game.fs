module Game
    open Galante
    open Galante.OpenGL
    open Silk.NET.Windowing
    open System
    open Silk.NET.Input
    open System.Numerics

    let emptyGameBuilder<'gs, 'ga> winOpts (initState: 'gs) (reducer: 'ga -> 'gs -> 'gs) =
        { WindowOptions = winOpts
        ; InitialState = initState
        ; Reducer = reducer
        ; OnLoad = []
        ; OnUpdate = []
        ; OnRender = []
        ; OnKeyDown = []
        ; OnKeyUp = []
        ; OnMouseMove = []
        ; OnMouseWheel = []
        ;}

    let withWindowOptions winOpts b = { b with WindowOptions = winOpts }
    let withInitialState s b = { b with InitialState = s }
    let addOnLoad h b = { b with OnLoad = h::b.OnLoad }
    let addOnUpdate h b = { b with OnUpdate = h::b.OnUpdate }
    let addOnRender h b = { b with OnRender = h::b.OnRender }
    let withReducer reducer b = { b with Reducer = reducer }

    let addOnKeyDown h (b: GameBuilder<'gs, 'ga>) = 
        { b with OnKeyDown = h::b.OnKeyDown }

    let addOnKeyUp h (b: GameBuilder<'gs, 'ga>) = 
        { b with OnKeyUp = h::b.OnKeyUp }

    let addOnMouseMove h (b: GameBuilder<'gs, 'ga>) = 
        { b with OnMouseMove = h::b.OnMouseMove }

    let addOnMouseWheel h (b: GameBuilder<'gs, 'ga>) =
        { b with OnMouseWheel = h::b.OnMouseWheel }
                
    let rec private registerOnLoadHandlers handlers (w: IWindow, ctx, getState, dispatch) =
        match handlers with
        | [] -> (w, ctx, getState, dispatch)
        | h::t ->
            w.add_Load (new Action(fun () -> h ctx (getState()) dispatch))
            registerOnLoadHandlers t (w, ctx, getState, dispatch)

    let rec private registerOnUpdateHandlers handlers (w: IWindow, ctx, getState, dispatch) =
        match handlers with
        | [] -> (w, ctx, getState, dispatch)
        | h::t ->
            w.add_Update (
                new Action<double>(
                    fun dt -> h ctx (getState()) dispatch (DeltaTime.make dt))
            )
            registerOnUpdateHandlers t (w, ctx, getState, dispatch)
            
    let rec private registerOnRenderHandlers handlers (w: IWindow, ctx, getState, dispatch) =
        match handlers with
        | [] -> (w, ctx, getState, dispatch)
        | h::t ->
            w.add_Update (
                new Action<double>(
                    fun dt -> h ctx (getState()) dispatch (DeltaTime.make dt))
            )
            registerOnUpdateHandlers t (w, ctx, getState, dispatch)

    let buildAndRun (b: GameBuilder<'gs, 'ga>) =      
        let (getState, dispatch) = 
            StoreFactory.createStore<'gs, 'ga>(b. InitialState, b.Reducer)

        let ctx = GlWin.create b.WindowOptions
        
        (ctx.Window, ctx, getState, dispatch)
        |> registerOnLoadHandlers b.OnLoad
        |> registerOnUpdateHandlers b.OnUpdate
        |> registerOnRenderHandlers b.OnRender
        |> ignore

        ctx.Window.Run()