module GameState    
    open CameraSlice
    open MouseSlice
    open FpsCounterSlice
    open GalanteMath
    open Sodium.Frp
    open WindowSlice
    open System.Drawing
    open StoreFactory
    open Silk.NET.Input
    
    type GameAction =
        | Camera of CameraAction
        | Mouse of MouseAction
        | FpsCounter of FpsCounterAction
        | Window of WindowAction
    
    type GameState =
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
            { prevState with Camera = CameraSlice.reduce action prevState.Camera }
        | Mouse action ->
            { prevState with Mouse = MouseSlice.reduce action prevState.Mouse }
        | FpsCounter action ->
            { prevState with FpsCounter = FpsCounterSlice.reduce action prevState.FpsCounter }
        | Window action -> 
            { prevState with Window = WindowSlice.reduce action prevState.Window }
        