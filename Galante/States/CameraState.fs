namespace Galante

open System.Numerics
open GalanteMath

type CameraState = 
    { Position: Vector3
    ; TargetDirection: Vector3
    ; UpDirection: Vector3
    ; Speed: single
    ; Sensitivity: single
    ; Pitch: Degrees
    ; Yaw: Degrees
    ; Fov: Degrees
    ;}

    static member Default = {
        Position = new Vector3(0.0f, 0.0f, 0.0f)
        TargetDirection = new Vector3(0.0f, 0.0f, -1.0f)
        UpDirection = new Vector3(0.0f, 1.0f, 0.0f)
        Speed = 2.5f
        Sensitivity = 0.1f
        Pitch = Degrees.make 0.0f
        Yaw = Degrees.make -90.0f
        Fov = Degrees.make 45.0f
    }