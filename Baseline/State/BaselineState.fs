module BaselineState    
    open BaseCameraSlice
    open BaseMouseSlice
    open BaseFpsCounterSlice
    open BaseWindowSlice
    open BaseAssetSlice
    open Galante.OpenGL
    open StoreFactory

    type BaselineAction =
        | Camera of CameraAction
        | Mouse of MouseAction
        | FpsCounter of FpsCounterAction
        | Window of WindowAction
        | Asset of AssetAction
    
    type BaselineState =
        { Camera: CameraState 
        ; Mouse: MouseState
        ; FpsCounter: FpsCounterState
        ; Window: WindowState
        ; Asset: AssetState
        ;}
        static member createDefault (winTitle, winResolution) =
            { Camera = CameraState.Default 
            ; Mouse = MouseState.Default
            ; FpsCounter = FpsCounterState.Default
            ; Window = WindowState.createDefault (winTitle, winResolution)
            ; Asset = AssetState.Default
            ;}

    let gameActionFilter 
        action 
        (prevState: BaselineState) 
        (dispatchAction: ActionDispatcher<BaselineAction>) 
        (dispatchIo: IoDispatcher<BaselineAction>) 
        ctx 
        : BaselineAction option =

        match action with 
        | Asset action -> 
            let adaptedDispatchAction action = dispatchAction (Asset action)
            let adaptedDispatchIo (io: unit -> Option<AssetAction>) =
                let adaptedIo() = 
                    match io() with
                    | Some assetAction -> Some (Asset assetAction)
                    | None -> None

                dispatchIo adaptedIo
                ()

            BaseAssetSlice.filterAction action (prevState.Asset) adaptedDispatchAction adaptedDispatchIo ctx
            |> function
                | Some transformedAction -> Some (Asset transformedAction)
                | None -> None
        | _ -> Some action

    let gameReducer action prevState =
        match action with
        | Camera action ->
            { prevState with 
                Camera = BaseCameraSlice.reduce action prevState.Camera }
        | Mouse action ->
            { prevState with 
                Mouse = BaseMouseSlice.reduce action prevState.Mouse }
        | FpsCounter action ->
            { prevState with 
                FpsCounter = 
                    BaseFpsCounterSlice.reduce action prevState.FpsCounter }
        | Window action -> 
            { prevState with 
                Window = BaseWindowSlice.reduce action prevState.Window }
        | Asset action ->
            { prevState with 
                Asset = BaseAssetSlice.reduce action prevState.Asset }
        