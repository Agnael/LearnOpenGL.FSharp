module StoreFactory
    open Sodium.Frp

    let createStore<'TState, 'TAction> (initialState: 'TState, reducer: 'TAction->'TState->'TState) =
        let actionSink = sinkS<'TAction>()
        let state =
            loopWithNoCapturesC (
                fun state -> 
                    actionSink
                    |> snapshotC state reducer
                    |> Stream.hold initialState
            )

        let getCurrentState () = sampleC state
        let dispatch action = sendS action actionSink
        (getCurrentState, dispatch)