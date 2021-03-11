module GalanteMath
    open System
    open Galante.OpenGL

    type Radians = 
        | Radians of float32
        static member make value = Radians(value)
        static member value (Radians r) = r

    type Degrees = 
        | Degrees of float32
        static member make value = Degrees(value)
        static member value (Degrees d) = d

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