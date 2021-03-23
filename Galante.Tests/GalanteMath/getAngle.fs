module Galante.Tests.GalanteMath.getAngle
    open Xunit
    open GalanteMath
    open System.Numerics

    [<Fact>]
    let ```Should pass this hardcoded check from a solved exercise I found lol`` () = 
        let v1 = new Vector3(3.0f, 4.0f, 7.0f)
        let v2 = new Vector3(-2.0f, 3.0f, -5.0f)
        
        let angle = getAngle v1 v2
        let rounded = System.MathF.Round(Degrees.value angle, 1)
        Assert.Equal (123.2f, rounded)
