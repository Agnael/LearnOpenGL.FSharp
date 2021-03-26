module BaseFpsCounterSlice    
    type FpsCounterAction =
        | FrameRenderCompleted of float

    type FpsCounterState =
        { CurrentFps: int
        ; CurrentTimeLeft: float
        ; CurrentSecondFrames: int
        ;}
    
        static member Default =
            { CurrentFps = 0
            ; CurrentTimeLeft = 1.0
            ; CurrentSecondFrames = 0
            ;}

    let reduce action state =
        match action with
        | FrameRenderCompleted timeDelta ->
            { state with 
                CurrentTimeLeft = state.CurrentTimeLeft - timeDelta
                CurrentSecondFrames = state.CurrentSecondFrames + 1 }
            |> fun newState ->
                if newState.CurrentTimeLeft <= 0.0 then
                    { FpsCounterState.Default with 
                        CurrentFps = newState.CurrentSecondFrames }
                else newState
            