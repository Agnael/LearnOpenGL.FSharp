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

    let toRadF (Degrees degrees) : Radians = 
        degrees * MathF.PI / 180.0f
        |> Radians

    let cosF degrees = 
        toRadF degrees
        |> Radians.value
        |> MathF.Cos

    let sinF degrees = 
        toRadF degrees
        |> Radians.value
        |> MathF.Sin