module BaseMouseSlice
    open System.Numerics

    type MouseAction =
        | NewPosition of Vector2
        | FirstMoveReceived
        | UseCursorNormal
        | UseCursorRaw

    type MouseState =
        { X: single
        ; Y: single
        ; IsFirstMoveReceived: bool
        ; CursorMode: Silk.NET.Input.CursorMode
        ;}
        static member Default =
            { X = 0.0f
            ; Y = 0.0f
            ; IsFirstMoveReceived = false 
            ; CursorMode = Silk.NET.Input.CursorMode.Normal
            ;}

    let reduce (action: MouseAction) (state: MouseState) =
        match action with
        | NewPosition pos ->
            { state with 
                X = pos.X
                Y = pos.Y }
        | FirstMoveReceived -> 
            { state with IsFirstMoveReceived = true }
        | UseCursorNormal ->
            { state with CursorMode = Silk.NET.Input.CursorMode.Normal }
        | UseCursorRaw ->
            { state with CursorMode = Silk.NET.Input.CursorMode.Raw }