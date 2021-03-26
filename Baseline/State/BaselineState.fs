module BaselineState    
    open BaseCameraSlice
    open BaseMouseSlice
    open BaseFpsCounterSlice
    open BaseWindowSlice
    
    type BaselineAction =
        | Camera of CameraAction
        | Mouse of MouseAction
        | FpsCounter of FpsCounterAction
        | Window of WindowAction
    
    type BaselineState =
        { Camera: CameraState 
        ; Mouse: MouseState
        ; FpsCounter: FpsCounterState
        ; Window: WindowState
        ;}
        static member createDefault (winTitle, winResolution) =
            { Camera = CameraState.Default 
            ; Mouse = MouseState.Default
            ; FpsCounter = FpsCounterState.Default
            ; Window = WindowState.createDefault (winTitle, winResolution)
            ;}

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
        