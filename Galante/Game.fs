module Game
   open Galante
   open Galante.OpenGL
   open System
   open Silk.NET.Input
   open System.Numerics
   open Silk.NET.Maths
   open Silk.NET.Windowing
   open StoreFactory
   open GalanteMath

   let emptyGameBuilder<'gs, 'ga> 
      winOpts (initState: 'gs) (reducer: 'ga -> 'gs -> 'gs) =
      { GameBuilder.WindowOptions = winOpts
      ; InitialState = initState
      ; Reducer = reducer
      ; OnInputContextLoaded = fun ctx ic state dispatch -> ()
      ; OnLoad = []
      ; OnUpdate = []
      ; OnRender = []
      ; OnKeyDown = []
      ; OnKeyUp = []
      ; OnMouseMove = []
      ; OnMouseWheel = []
      ; OnWindowResize = []
      ; OnWindowClose = []
      ; OnActionListen = []
      ;}

   let addActionInterceptor 
      (h: 's -> 'a -> ('a->unit) -> GlWindowCtx -> unit) b =
      { b with OnActionListen = h::b.OnActionListen }

   let withWindowOptions winOpts b = 
      { b with WindowOptions = winOpts }

   let withInitialState s b = { b with InitialState = s }

   let withOnInputContextLoadedCallback c b = 
      { b with OnInputContextLoaded = c }

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

   let addOnWindowResize h (b: GameBuilder<'gs, 'ga>) =
      { b with OnWindowResize = h::b.OnWindowResize }

   let addOnWindowClose h (b: GameBuilder<'gs, 'ga>) =
      { b with OnWindowClose = h::b.OnWindowClose }

   let private registerOnUpdateHandlers 
      handlers (window: IWindow, ctx, getState, dispatch) =
      let register h =
         let mapped dt =
               h ctx (getState()) dispatch (DeltaTime.make dt)
         window.add_Update (
               new Action<double>(mapped))

      List.iter register handlers
      (window, ctx, getState, dispatch)
            
   let private registerOnWindowResizeHandlers
      handlers (window: IWindow, ctx, getState, dispatch) =
      let register h = 
         let mapped (size: Vector2D<int>) = 
               let sizeV = new Vector2(single size.X, single size.Y)
               h ctx (getState()) dispatch sizeV
         window.add_Resize (new Action<_>(mapped))

      List.iter register handlers
      (window, ctx, getState, dispatch)
            
   let private registerOnWindowCloseHandlers
      handlers (window: IWindow, ctx, getState, dispatch) =
      let register h = 
         let mapped () =
            h ctx (getState()) dispatch

         window.add_Closing (new Action(mapped))

      List.iter register handlers
      (window, ctx, getState, dispatch)

   let private registerOnRenderHandlers 
      handlers (w: IWindow, ctx, getState, dispatch) =
      let register h =
         let mapped dt =
               h ctx (getState()) dispatch (DeltaTime.make dt)
         w.add_Render (new Action<_>(mapped))

      List.iter register handlers
      (w, ctx, getState, dispatch)
     
   let private registerOnActionHandlers 
      handlers (w: IWindow, ctx, getState, dispatch) =
      let register h =
         let mapped dt =
            h ctx (getState()) dispatch (DeltaTime.make dt)
         w.add_Render (new Action<_>(mapped))
     
      List.iter register handlers
      (w, ctx, getState, dispatch)
             
   type private KeyboardHandlerType = | OnKeyDown | OnKeyUp
   type private MouseHandlerType = | OnMove | OnWheel
    
   let private registerOnLoadHandlers 
      handlers 
      onInputContextLoaded 
      onKeyDownHandlers 
      onKeyUpHandlers 
      onMouseMoveHandlers 
      onMouseWheelHandlers 
      (w: IWindow, ctx: GlWindowCtx, getState, dispatchAction, dispatchIo) =

      // Master onLoad handler, executes the rest of the onLoads
      // after itself.
      let onLoad () =
         let state = getState()
         let input = w.CreateInput()
         onInputContextLoaded ctx input state dispatchAction

         let onLoadHandlerExecute h = h ctx input state dispatchAction
         List.iter onLoadHandlerExecute handlers
            
         let addOnEachKeyboard handlerType h =
               let mapAndRegisterHandler (keyboard: IKeyboard) =
                  if not (keyboard = null) then
                     let mappedHandler keyboard key keyCode =
                           h ctx (getState()) dispatchAction keyboard key

                     match handlerType with
                     | OnKeyDown -> 
                           keyboard.add_KeyDown 
                              (new Action<_,_,_>(mappedHandler))
                     | OnKeyUp ->
                           keyboard.add_KeyUp 
                              (new Action<_,_,_>(mappedHandler))

               Seq.iter mapAndRegisterHandler input.Keyboards

         let addOnEachMouse handlerType h =
               let mapAndRegisterHandler (mouse: IMouse) =
                  if not (mouse = null) then
                     match handlerType with
                     | OnMove ->
                           let mapped _ pos = 
                              h ctx (getState()) dispatchAction pos
                           mouse.add_MouseMove 
                              (new Action<_,_>(mapped))
                     | OnWheel ->
                           let mapped _ (wheel: ScrollWheel) =
                              let posV = v2 wheel.X wheel.Y
                              h ctx (getState()) dispatchAction posV
                           mouse.add_Scroll (new Action<_,_>(mapped))
               Seq.iter mapAndRegisterHandler input.Mice


         List.iter (addOnEachKeyboard OnKeyDown) onKeyDownHandlers
         List.iter (addOnEachKeyboard OnKeyUp) onKeyUpHandlers
         List.iter (addOnEachMouse OnMove) onMouseMoveHandlers
         List.iter (addOnEachMouse OnWheel) onMouseWheelHandlers
        
      w.add_Load (new Action(onLoad))
      (w, ctx, getState, dispatchAction)

   let buildAndRun (b: GameBuilder<'gs, 'ga>) =    
      let ctx = GlWin.create b.WindowOptions
        
      let (getState, dispatchAction, addActionListener) = 
         StoreFactory.createStore(b.InitialState, b.Reducer, ctx)

      (ctx.Window, ctx, getState, dispatchAction, addActionListener)
      |> registerOnLoadHandlers 
         b.OnLoad 
         b.OnInputContextLoaded
         b.OnKeyDown
         b.OnKeyUp
         b.OnMouseMove
         b.OnMouseWheel
      |> registerOnUpdateHandlers b.OnUpdate
      |> registerOnRenderHandlers b.OnRender
      |> registerOnWindowResizeHandlers b.OnWindowResize
      |> registerOnWindowCloseHandlers b.OnWindowClose
      |> ignore

      b.OnActionListen
      |> List.iter (fun h -> addActionListener h)

      ctx.Window.Run()
      ()