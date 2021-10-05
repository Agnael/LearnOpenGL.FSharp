module StoreFactory
    open Sodium.Frp
    open Galante.OpenGL
    open System.Threading
    open System.Threading.Tasks
    
    type ActionDispatcher<'a> = 'a -> unit
    type IoDispatcher<'a> = (unit -> Option<'a>) -> unit

    let createStore<'s, 'a> 
        ( initialState: 's
        , reducer: 'a -> 's -> 's
        , ctx
        ) =
        // ACTION DISPATCHING
        let actionSink = sinkS<'a>()
        let dispatchAction action = 
            let currSyncCtx = SynchronizationContext.Current
            sendS action actionSink

        let mutable actionListeners = List.empty<'s -> 'a -> ('a->unit) -> GlWindowCtx -> unit>

        // STATE
        let stateReductionPipeline action state =
            Some action
            |> function
                | Some newAction -> reducer newAction state
                | None -> state

        let state =
            loopWithNoCapturesC 
                (fun state -> 
                    actionSink
                    |> snapshotC state stateReductionPipeline  
                    |> Stream.hold initialState
                )

        let getState () = sampleC state
                
        // LISTENERS
        let globalActionListener =
            actionSink
            |> listenS (fun action -> 
                async {
                    actionListeners
                    |> List.iter (fun h -> h (getState()) action dispatchAction ctx)
                }
                |> Async.Start
            )

        let addActionListener h = 
            actionListeners <- h::actionListeners

        (getState, dispatchAction, addActionListener)