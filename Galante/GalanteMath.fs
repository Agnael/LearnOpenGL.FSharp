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

    let getAngleRadians (v1: Vector3) (v2: Vector3) =
        //let dot = Vector3.do
        let dot = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z
        let crossX = v1.Y * v2.Z - v1.Z * v2.Y
        let crossY = v1.X * v2.Z - v1.Z * v2.X
        let crossZ = v1.X * v2.Y - v1.Y * v2.X
        let crossLen = MathF.Sqrt(crossX*crossX + crossY*crossY + crossZ*crossZ)

        MathF.Atan2(dot, crossLen)
        |> Radians.make
        |> toDegrees