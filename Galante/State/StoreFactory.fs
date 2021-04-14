module StoreFactory
    open Sodium.Frp
    open Galante.OpenGL
    open System.Threading
    open System.Threading.Tasks
    
    type Agent<'a> = MailboxProcessor<'a>
    let waitingFor timeOut (v:'a)= 
        let cts = new CancellationTokenSource(timeOut|> int)
        let tcs = new TaskCompletionSource<'a>()
        cts.Token.Register(fun (_) ->  tcs.SetCanceled()) |> ignore
        tcs ,Async.AwaitTask tcs.Task

    type MyProcessor<'a>(f:'a->unit) =
        let agent = Agent<'a>.Start(fun inbox -> 
             let rec loop() = async {

                let! msg = inbox.Receive()
                // some more complex should be used here
                f msg
                return! loop() 
             }
             loop()
        )

        member this.Post(msg:'a) = 
            agent.Post msg

    type ActionDispatcher<'a> = 'a -> unit
    type IoDispatcher<'a> = (unit -> Option<'a>) -> unit

    let createStore<'s, 'a> 
        ( initialState: 's
        , reducer: 'a -> 's -> 's
        , actionFilter: 'a -> 's -> ActionDispatcher<'a> -> IoDispatcher<'a> -> GlWindowCtx -> 'a option
        , ctx
        ) =
        // ACTION DISPATCHING
        let actionSink = sinkS<'a>()
        let dispatchAction action = 
            let currSyncCtx = SynchronizationContext.Current
            sendS action actionSink

        // IO DISPATCHING V1
        //let mainSyncCtx = SynchronizationContext.Current
        //let ioAgentProcessor (inbox: MailboxProcessor<unit->Option<'a>>) =
        //    let rec loop() = 
        //        async {
        //            let currSyncCtx = SynchronizationContext.Current
        //            let shouldBeMain = mainSyncCtx
        //            do! Async.SwitchToContext mainSyncCtx

        //            let! (ioTask) = inbox.Receive()
        //            let ioTaskResult = ioTask()

        //            if ioTaskResult.IsSome then 
        //                dispatchAction ioTaskResult.Value

        //            do! Async.SwitchToContext currSyncCtx

        //            return! loop()
        //        }
        //    loop()

        //let ioAgent = MailboxProcessor.Start ioAgentProcessor
        //ioAgent.Error.Add(fun x -> 
        //    printfn "AGENT ERROR:: %s" (x.ToString()))
        //let dispatchIo someTask = ioAgent.Post someTask
        
        //// IO DISPATCHING V2
        //let dispatchIo (someTask: unit -> Option<'a>) =
        //    async {
        //        let ioTaskResult = someTask()
                
        //        if ioTaskResult.IsSome then 
        //            try
        //                dispatchAction ioTaskResult.Value
        //            with
        //            | ex -> 
        //                let asd = 34
        //                ()
        //    }
        //    //|> Async.AwaitTask
        //    //|> Async.RunSynchronously
        //    |> Async.Start

        let mutable actionListeners = List.empty<'s -> 'a -> ('a->unit) -> GlWindowCtx -> unit>

        // STATE
        let stateReductionPipeline action state =
            //actionFilter action state dispatchAction dispatchIo ctx
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