module WindowSlice
    open System.Drawing
    open Silk.NET.Input
    open System.Numerics
    
    type WindowAction =
        | Close
        | ResolutionUpdate of Size
        | ResolutionUpdated
        | ToggleFullscreen

    type WindowState =
        { Title: string
        ; ShouldClose: bool
        ; ShouldUpdateResolution: bool
        ; IsFullscreen: bool
        ; Resolution: Size
        ;}
        static member createDefault (winTitle, winResolution) =
            { Title = winTitle
            ; ShouldClose = false
            ; ShouldUpdateResolution = false
            ; Resolution = winResolution
            ; IsFullscreen = false
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