[<RequireQualifiedAccess>]
module Cube
    open System.Numerics

    let vertexPositionsAndNormals = [| 
        [| [| -0.5f; -0.5f; -0.5f |]; [|  0.0f;  0.0f; -1.0f |] |]
        [| [|  0.5f; -0.5f; -0.5f |]; [|  0.0f;  0.0f; -1.0f |] |]
        [| [|  0.5f;  0.5f; -0.5f |]; [|  0.0f;  0.0f; -1.0f |] |]
        [| [|  0.5f;  0.5f; -0.5f |]; [|  0.0f;  0.0f; -1.0f |] |]
        [| [| -0.5f;  0.5f; -0.5f |]; [|  0.0f;  0.0f; -1.0f |] |]
        [| [| -0.5f; -0.5f; -0.5f |]; [|  0.0f;  0.0f; -1.0f |] |]
                       
        [| [| -0.5f; -0.5f;  0.5f |]; [|  0.0f;  0.0f; 1.0f |] |]
        [| [|  0.5f; -0.5f;  0.5f |]; [|  0.0f;  0.0f; 1.0f |] |]
        [| [|  0.5f;  0.5f;  0.5f |]; [|  0.0f;  0.0f; 1.0f |] |]
        [| [|  0.5f;  0.5f;  0.5f |]; [|  0.0f;  0.0f; 1.0f |] |]
        [| [| -0.5f;  0.5f;  0.5f |]; [|  0.0f;  0.0f; 1.0f |] |]
        [| [| -0.5f; -0.5f;  0.5f |]; [|  0.0f;  0.0f; 1.0f |] |]
                       
        [| [| -0.5f;  0.5f;  0.5f |]; [| -1.0f;  0.0f;  0.0f |] |]
        [| [| -0.5f;  0.5f; -0.5f |]; [| -1.0f;  0.0f;  0.0f |] |]
        [| [| -0.5f; -0.5f; -0.5f |]; [| -1.0f;  0.0f;  0.0f |] |]
        [| [| -0.5f; -0.5f; -0.5f |]; [| -1.0f;  0.0f;  0.0f |] |]
        [| [| -0.5f; -0.5f;  0.5f |]; [| -1.0f;  0.0f;  0.0f |] |]
        [| [| -0.5f;  0.5f;  0.5f |]; [| -1.0f;  0.0f;  0.0f |] |]
                       
        [| [|  0.5f;  0.5f;  0.5f |]; [|  1.0f;  0.0f;  0.0f |] |]
        [| [|  0.5f;  0.5f; -0.5f |]; [|  1.0f;  0.0f;  0.0f |] |]
        [| [|  0.5f; -0.5f; -0.5f |]; [|  1.0f;  0.0f;  0.0f |] |]
        [| [|  0.5f; -0.5f; -0.5f |]; [|  1.0f;  0.0f;  0.0f |] |]
        [| [|  0.5f; -0.5f;  0.5f |]; [|  1.0f;  0.0f;  0.0f |] |]
        [| [|  0.5f;  0.5f;  0.5f |]; [|  1.0f;  0.0f;  0.0f |] |]
                       
        [| [| -0.5f; -0.5f; -0.5f |]; [|  0.0f; -1.0f;  0.0f |] |]
        [| [|  0.5f; -0.5f; -0.5f |]; [|  0.0f; -1.0f;  0.0f |] |]
        [| [|  0.5f; -0.5f;  0.5f |]; [|  0.0f; -1.0f;  0.0f |] |]
        [| [|  0.5f; -0.5f;  0.5f |]; [|  0.0f; -1.0f;  0.0f |] |]
        [| [| -0.5f; -0.5f;  0.5f |]; [|  0.0f; -1.0f;  0.0f |] |]
        [| [| -0.5f; -0.5f; -0.5f |]; [|  0.0f; -1.0f;  0.0f |] |]
                       
        [| [| -0.5f;  0.5f; -0.5f |]; [|  0.0f;  1.0f;  0.0f |] |]
        [| [|  0.5f;  0.5f; -0.5f |]; [|  0.0f;  1.0f;  0.0f |] |]
        [| [|  0.5f;  0.5f;  0.5f |]; [|  0.0f;  1.0f;  0.0f |] |]
        [| [|  0.5f;  0.5f;  0.5f |]; [|  0.0f;  1.0f;  0.0f |] |]
        [| [| -0.5f;  0.5f;  0.5f |]; [|  0.0f;  1.0f;  0.0f |] |]
        [| [| -0.5f;  0.5f; -0.5f |]; [|  0.0f;  1.0f;  0.0f |] |]
     |]

    type CubeTransformation = 
        { Translation: Vector3
        ; RotationX: single
        ; RotationY: single
        ; RotationZ: single
        ;}
        static member create (translation, rotationX, rotationY, rotationZ) =
            { Translation = translation
            ; RotationX = rotationX
            ; RotationY = rotationY
            ; RotationZ = rotationZ 
            ;}
    
    // List of cube copies that will be created, expressed as transformation 
    // of one thats standing on the origin.
    // NOTE: This is just to add some dynamism without being TOO distracting
    // from the actual spotlight of the example, which is the camera and it's
    // movement.
    let transformations = [
        CubeTransformation.create 
            (new Vector3(0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f)

        CubeTransformation.create 
            (new Vector3(2.0f, 5.0f, -15.0f), 43.0f, 12.0f, 0.0f)

        CubeTransformation.create 
            (new Vector3(-1.5f, -2.2f, -2.5f), 12.0f, 98.0f, 40.0f)

        CubeTransformation.create 
            (new Vector3(-3.8f, -2.0f, -12.3f), 45.0f, 32.0f, 0.0f)

        CubeTransformation.create 
            (new Vector3(2.4f, -0.4f, -3.5f), 0.0f, 0.0f, 43.0f)

        CubeTransformation.create 
            (new Vector3(-1.7f, 3.0f, -7.5f), 0.0f, 54.0f, 0.0f)

        CubeTransformation.create 
            (new Vector3(1.3f, -2.0f, -2.5f), 14.0f, 54.0f, 12.0f)

        CubeTransformation.create 
            (new Vector3(1.5f, 2.0f, -2.5f), 76.5f, 0.56f, 12.0f)

        CubeTransformation.create 
            (new Vector3(1.5f, 0.2f, -1.5f), 54.0f, 0.0f, 125.0f)

        CubeTransformation.create 
            (new Vector3(-1.3f, 1.0f, -1.5f), 246.0f, 122.0f, 243.0f)
    ] 
        