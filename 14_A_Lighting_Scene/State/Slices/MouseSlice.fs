module MouseSlice
    open System.Numerics

    type MouseAction =
        | NewPosition of Vector2
        | FirstMoveReceived

    type MouseState =
        { X: single
        ; Y: single
        ; IsFirstMoveReceived: bool
        ;}
        static member Default =
            { X = 0.0f
            ; Y = 0.0f
            ; IsFirstMoveReceived = false 
            ;}

    let reduce (action: MouseAction) (state: MouseState) =
        match action with
        | NewPosition pos ->
            { state with 
                X = pos.X
                Y = pos.Y }
        | FirstMoveReceived -> 
            { state with IsFirstMoveReceived = true }