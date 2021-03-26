module BaseWindowSlice
    open System.Drawing
    open Silk.NET.Input
        
    type GameInputs =
        | Uninitialized
        | Initialized of IInputContext

    type WindowAction =
        | Close
        | ResolutionUpdate of Size
        | ResolutionUpdated
        | ToggleFullscreen
        | InitializeInputContext of IInputContext

    type WindowState =
        { Title: string
        ; ShouldClose: bool
        ; ShouldUpdateResolution: bool
        ; IsFullscreen: bool
        ; Resolution: Size
        ; InputContext: GameInputs
        ;}
        static member createDefault (winTitle, winResolution) =
            { Title = winTitle
            ; ShouldClose = false
            ; ShouldUpdateResolution = false
            ; Resolution = winResolution
            ; IsFullscreen = false
            ; InputContext = Uninitialized
            ;}

    let reduce a s =
        match a with
        | Close -> { s with ShouldClose = true }
        | ResolutionUpdate newRes ->
            { s with 
                ShouldUpdateResolution = true 
                Resolution = newRes }
        | ResolutionUpdated ->
            { s with ShouldUpdateResolution = false }
        | ToggleFullscreen ->
            { s with IsFullscreen = not(s.IsFullscreen) }
        | InitializeInputContext ctx ->
            { s with InputContext = Initialized ctx }