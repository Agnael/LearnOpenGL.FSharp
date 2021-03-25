module CameraSlice
    open Galante
    open GalanteMath
    open System.Numerics
    open System

    type CameraOffset = 
        { X: single
        ; Y: single
        ;}

    type ZoomOffset =
        | ZoomOffset of single
        static member make v = ZoomOffset v
        static member value (ZoomOffset z) = z

    type CameraSpeed =
        | CameraSpeed of single
        static member make v = CameraSpeed v
        static member value (CameraSpeed s)  = s

    type CameraAction =  
        | AngularChange of CameraOffset
        | ZoomChange of ZoomOffset
        | UpdatePosition of CameraSpeed
        | ForcePosition of Vector3
        | ForceTarget of Vector3
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
        | Lock
        | Unlock

    type CameraState = 
        { Position: Vector3
        ; TargetDirection: Vector3
        ; UpDirection: Vector3
        ; MoveSpeed: single
        ; ZoomSpeed: single
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
        ; IsAngleLocked: bool
        ;}
    
        static member Default = {
            Position = new Vector3(0.0f, 0.0f, 0.0f)
            TargetDirection = new Vector3(0.0f, -1.0f, 0.0f)
            UpDirection = new Vector3(0.0f, 1.0f, 0.0f)
            MoveSpeed = 2.5f
            ZoomSpeed = 3.0f
            Sensitivity = 0.1f
            Fov = Degrees.make 45.0f
            Pitch = Degrees.make 0.0f
            // To make sure the camera points towards the negative z-axis by 
            // default we can give the yaw a default value of a 90 degree clockwise 
            // rotation. Positive degrees rotate counter-clockwise so we set the 
            // default yaw value to:
            Yaw = Degrees.make 0.0f
            IsMovingForward = false
            IsMovingBack = false
            IsMovingLeft = false
            IsMovingRight = false
            IsMovingUp = false
            IsMovingDown = false
            IsAngleLocked = false
        }

    let createViewMatrix state = 
        Matrix4x4.CreateLookAt(
            state.Position, 
            state.Position + state.TargetDirection, 
            state.UpDirection)
    
    let reduce action state =
        match action with
        | UpdatePosition (CameraSpeed camSpeed) ->
            let currTargetDir = state.TargetDirection
            let currUpDir = state.UpDirection

            let moveForward pos = pos + currTargetDir * camSpeed
            let moveBack pos = pos - currTargetDir * camSpeed
            let moveLeft pos = 
                pos - normalizeCross(currTargetDir, currUpDir) * camSpeed
            let moveRight pos = 
                pos + normalizeCross(currTargetDir, currUpDir) * camSpeed
            let moveUp pos = pos + currUpDir * camSpeed
            let moveDown pos = pos - currUpDir * camSpeed

            { state with
                Position =
                    state.Position
                    |> fun pos -> 
                        if state.IsMovingForward then moveForward pos else pos
                    |> fun pos -> 
                        if state.IsMovingBack then moveBack pos else pos
                    |> fun pos -> 
                        if state.IsMovingLeft then moveLeft pos else pos
                    |> fun pos -> 
                        if state.IsMovingRight then moveRight pos else pos
                    |> fun pos -> 
                        if state.IsMovingUp then moveUp pos else pos
                    |> fun pos -> 
                        if state.IsMovingDown then moveDown pos else pos
            }

        | AngularChange offset -> 
            if state.IsAngleLocked then 
                state
            else
                let yawDegrees = Degrees.value state.Yaw
                let newYaw = 
                    yawDegrees + (offset.X * state.Sensitivity)
                    |> fun yaw -> 
                        if yaw >= 360.0f then 0.0f 
                        elif yaw <= 0.0f then 360.0f
                        else yaw
                    |> Degrees.make 

                let pitchDegrees = Degrees.value state.Pitch
                let newPitch =
                    pitchDegrees + (offset.Y * state.Sensitivity)
                    |> fun newPitch ->
                        if newPitch > 89.9f then 89.9f
                        else if newPitch < -89.9f then -89.9f
                        else newPitch
                    |> Degrees.make

                let newTarget =
                    Vector3.Normalize (
                        new Vector3 (
                            cosF(newYaw) * cosF(newPitch),
                            sinF(newPitch),
                            sinF(newYaw) * cosF(newPitch)))                
                            
                { state with 
                    Yaw = newYaw 
                    Pitch = newPitch
                    TargetDirection = newTarget }

        | ZoomChange (ZoomOffset offset) -> 
            { state with 
                Fov =
                    (Degrees.value state.Fov) - offset
                    |> fun newFov ->
                        if newFov < 20.0f then 20.0f
                        elif newFov > 45.0f then 45.0f
                        else newFov
                        |> Degrees.make
            }
            
        | ForcePosition newPos ->
            { state with Position = newPos }
        | ForceTarget target ->     
            { state with 
                Yaw = getYaw target 
                Pitch = getPitch target
                TargetDirection = target }

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
        
        | Lock -> 
            { state with IsAngleLocked = true }
        | Unlock ->
            { state with IsAngleLocked = false }