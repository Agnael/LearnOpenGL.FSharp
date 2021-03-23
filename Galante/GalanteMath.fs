module GalanteMath
    open System
    open Galante.OpenGL
    open System.Numerics

    type Radians = 
        | Radians of float32
        static member make value = Radians(value)
        static member value (Radians r) = r

    type Degrees = 
        | Degrees of float32
        static member make value = Degrees(value)
        static member value (Degrees d) = d
        
    let normalizeCross (v1, v2) = 
        Vector3.Cross(v1, v2) 
        |> Vector3.Normalize
        
    let toRadians (Degrees degrees) : Radians = 
        degrees * MathF.PI / 180.0f
        |> Radians
        
    let toDegrees (Radians radians) : Degrees =
        radians * 180.0f / MathF.PI
        |> Degrees
        
    let cosF degrees = 
        toRadians degrees
        |> Radians.value
        |> MathF.Cos
        
    let sinF degrees = 
        toRadians degrees
        |> Radians.value
        |> MathF.Sin

    let getAngle (v1: Vector3) (v2: Vector3) =
        // Formula: v1.v2 = |v1|.|v2|.cos(angle)
        // then ==> angle = acos((v1.v2) / (|v1|.|v2|))
        Vector3.Dot (v1, v2) / (v1.Length() * v2.Length())
        |> MathF.Acos
        |> Radians.make
        |> toDegrees

    /// <summary>
    ///     The Y-Axis is taken as the rotation center and, given an origin 
    ///     vector to take as a reference for the angle's start and another 
    ///     vector as the target of the camera, the negated angle between 
    ///     them is returned, expressing the degrees of a 
    ///     counter-clockwise movement.
    /// </summary>
    let getYawY (yawOrigin: Vector3) (target: Vector3) =        
        //getAngle yawOrigin (new Vector3(target.X, 0.0f, target.Z))
        getAngle yawOrigin (new Vector3(target.X, 0.0f, target.Z))
        // Inverts value for a counter-clockwise yaw angle
        |> fun (Degrees deg) -> -deg
        |> Degrees.make

    /// <summary>
    ///     The X-Axis is taken as the rotation center and, given an origin
    ///     vector to take as a reference for the angle's start and another 
    ///     vector as the target of the camera, the angle between 
    ///     them is returned, adjusted with a hacky -90.0f degree start
    ///     for the value.
    ///     TODO: The -90.0f adjustment is a hack that just happen to make
    ///     this work but it must be understood and removed.
    /// </summary>
    let getPitchX (pitchOrigin: Vector3) (target: Vector3) =
        getAngle pitchOrigin target
        |> fun (Degrees x) -> 
            -90.0f + x
        |> fun v -> 
            if v < -89.9f then -89.9f
            elif v > 89.9f then 89.9f
            else v
        |> Degrees.make