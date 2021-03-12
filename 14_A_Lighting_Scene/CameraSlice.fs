module CameraSlice
    open Galante
    open GalanteMath
    open System.Numerics

    type CameraOffset = 
        { X: single
        ; Y: single
        ;}

    type CameraSpeed =
        | CameraSpeed of float32
        static member make v = CameraSpeed v
        static member value (CameraSpeed s)  = s

    type CameraAction =  
        | AngularChange of CameraOffset
        | MouseWheelNewPosition of Vector2
        | UpdatePosition of CameraSpeed
        | MoveForwardStart
        | MoveForwardStop
        | MoveLeftStart
        | MoveLeftStop
        | MoveBackStart
        | MoveBackStop
        | MoveRightStart
        | MoveRightStop
        | MoveUpStart
        | MoveUpStop
        | MoveDownStart
        | MoveDownStop

    type CameraState = 
        { Position: Vector3
        ; TargetDirection: Vector3
        ; UpDirection: Vector3
        ; Speed: single
        ; Sensitivity: single
        ; Pitch: Degrees
        ; Yaw: Degrees
        ; Fov: Degrees
        ; IsMovingForward: bool
        ; IsMovingBack: bool
        ; IsMovingLeft: bool
        ; IsMovingRight: bool
        ; IsMovingUp: bool
        ; IsMovingDown: bool
        ;}
    
        static member Default = {
            Position = new Vector3(0.0f, 0.0f, 3.0f)
            TargetDirection = new Vector3(0.0f, 0.0f, -1.0f)
            UpDirection = new Vector3(0.0f, 1.0f, 0.0f)
            Speed = 2.5f
            Sensitivity = 0.1f
            Fov = Degrees.make 45.0f
            Pitch = Degrees.make 0.0f
            // To make sure the camera points towards the negative z-axis by 
            // default we can give the yaw a default value of a 90 degree clockwise 
            // rotation. Positive degrees rotate counter-clockwise so we set the 
            // default yaw value to:
            Yaw = Degrees.make -90.0f
            IsMovingForward = false
            IsMovingBack = false
            IsMovingLeft = false
            IsMovingRight = false
            IsMovingUp = false
            IsMovingDown = false
        }

        // Setters
        static member setPosition v r  = { r with Position = v }
        static member setYaw v r = { r with Yaw = v }
        static member setPitch v r = { r with Pitch = v }

        // Optics
        static member Position_ = 
            (fun r -> r.Position), CameraState.setPosition

        static member TargetDirection_ =
            (fun r -> r.TargetDirection), (fun v r -> { r with TargetDirection = v })

        static member Pitch_ =
            (fun r -> r.Pitch), (fun v r -> { r with Pitch = v })

        static member Yaw_ =
            (fun r -> r.Yaw), (fun v r -> { r with Yaw = v })

    //let camHandleKeyboard (keyboard: KeyboardState) (cam: CameraState) =
    //    cam

    //let camUpdate (cam: CameraState) (mouse: MouseState) =
    //    cam

    let createViewMatrix state = 
        Matrix4x4.CreateLookAt(state.Position, state.Position + state.TargetDirection, state.UpDirection)
    
    let reduce action state =
        match action with
        | UpdatePosition (CameraSpeed camSpeed) ->
            let handleMoveForward oldPos =
                if state.IsMovingForward then
                    oldPos + state.TargetDirection * camSpeed
                else oldPos

            let handleMoveBack oldPos =
                if state.IsMovingBack then
                    oldPos - state.TargetDirection * camSpeed
                else oldPos
                
            let handleMoveLeft oldPos =
                if state.IsMovingLeft then
                    oldPos - normalizeCross(state.TargetDirection, state.UpDirection) * camSpeed
                else oldPos
                
            let handleMoveRight oldPos =
                if state.IsMovingRight then
                    oldPos + normalizeCross(state.TargetDirection, state.UpDirection) * camSpeed
                else oldPos
                
            let handleMoveUp oldPos =
                if state.IsMovingUp then
                    oldPos + (new Vector3(0.0f, 1.0f, 0.0f) * camSpeed)
                else oldPos
                
            let handleMoveDown oldPos =
                if state.IsMovingDown then
                    oldPos + (new Vector3(0.0f, -1.0f, 0.0f) * camSpeed)
                else oldPos

            { state with
                Position =
                    state.Position
                    |> handleMoveForward
                    |> handleMoveBack
                    |> handleMoveLeft
                    |> handleMoveRight
                    |> handleMoveDown
                    |> handleMoveUp 
            }

        | AngularChange offset -> 
            let newYaw = 
                (Degrees.value state.Yaw) + (offset.X * state.Sensitivity)
                |> Degrees.make 

            let newPitch =
                (Degrees.value state.Pitch) + (offset.Y * state.Sensitivity)
                |> fun newPitch ->
                    if newPitch > 89.9f then 89.0f
                    else if newPitch < -89.0f then -89.0f
                    else newPitch
                |> Degrees.make

            let newTargetDirection =
                Vector3.Normalize (
                    new Vector3 (
                        cosF(newYaw) * cosF(newPitch),
                        sinF(newPitch),
                        sinF(newYaw) * cosF(newPitch)))                

            { state with 
                Yaw = newYaw 
                Pitch = newPitch
                TargetDirection = newTargetDirection }


        | MouseWheelNewPosition newPos -> state

        | MoveForwardStart -> 
            { state with IsMovingForward = true }
        | MoveForwardStop -> 
            { state with IsMovingForward = false }

        | MoveLeftStart ->
            { state with IsMovingLeft = true }
        | MoveLeftStop ->
            { state with IsMovingLeft = false }

        | MoveBackStart -> 
            { state with IsMovingBack = true }
        | MoveBackStop -> 
            { state with IsMovingBack = false }

        | MoveRightStart -> 
            { state with IsMovingRight = true }
        | MoveRightStop ->
            { state with IsMovingRight = false }

        | MoveUpStart -> 
            { state with IsMovingUp = true }
        | MoveUpStop -> 
            { state with IsMovingUp = false }

        | MoveDownStart -> 
            { state with IsMovingDown = true }
        | MoveDownStop -> 
            { state with IsMovingDown = false }