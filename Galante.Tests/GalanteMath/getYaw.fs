module Galante.Tests.GalanteMath.getYaw
    open Xunit
    open FsUnit.Xunit
    open GalanteMath
    open System.Numerics

    [<Theory>]
    [<InlineData(-0.22922294f, -0.36110625f, -0.9039132f, 255.7703476f)>]
    [<InlineData(0.74661785f, 0.55385435f, 0.36852035f, 26.270306f)>]
    [<InlineData(0.56020176f, 0.038942274f, -0.8274403f, 304.099152f)>]
    [<InlineData(0.17095698f, -0.6751809f, 0.71756846f, 76.599388f)>]
    [<InlineData(0.16175973f, -0.5845076f, -0.79510045f, 281.499634f)>]
    [<InlineData(-0.067831725f, 0.80728817f, 0.58624625f, 96.600060f)>]
    let ``Should pass this snapshot-like checks`` (tx, ty, tz, expectedYaw) =
        let actual = 
            getYaw (new Vector3(tx, ty, tz))
            |> Degrees.value

        actual |> should (equalWithin 1.0e-4) expectedYaw