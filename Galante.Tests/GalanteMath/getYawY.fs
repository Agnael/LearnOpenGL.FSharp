module Galante.Tests.GalanteMath.getYawY
    open Xunit
    open FsUnit.Xunit
    open GalanteMath
    open System.Numerics

    [<Theory>]
    [<InlineData(-0.22922294f, -0.36110625f, -0.9039132f, -104.2296524f)>]
    [<InlineData(0.74661785f, 0.55385435f, 0.36852035f, 26.270315f)>]
    let ``Pass this snapshot-like checks with fixed origin`` (tx, ty, tz, expected) =
        let actual = 
            getYawY (new Vector3(1.0f, 0.0f, 0.0f)) (new Vector3(tx, ty, tz))
            |> Degrees.value

        actual |> should (equalWithin 1.0e-7) expected