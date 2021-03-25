module Galante.Tests.GalanteMath.getAngle
    open Xunit
    open GalanteMath
    open System.Numerics
    open FsUnit.Xunit

    [<Theory>]
    [<InlineData(3.0f, 4.0f, 7.0f, -2.0f, 3.0f, -5.0f, 123.1530f)>]
    [<InlineData(3.0f, 4.0f, -7.0f, -2.0f, 3.0f, -5.0f, 39.36039f)>]
    [<InlineData(-3.0f, 4.0f, 7.0f, -2.0f, 3.0f, -5.0f, 108.69822f)>]
    [<InlineData(3.0f, -4.0f, 7.0f, -2.0f, 3.0f, -5.0f, 178.12822f)>]
    let ``Should pass this snapshot-like checks`` 
        (v1X, v1Y, v1Z, v2X, v2Y, v2Z, expectedDegrees) =
        let v1 = new Vector3(v1X, v1Y, v1Z)
        let v2 = new Vector3(v2X, v2Y, v2Z)

        let actual = Degrees.value <| getAngleAbs v1 v2

        actual |> should (equalWithin 1.0e-4) expectedDegrees



