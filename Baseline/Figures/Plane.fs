module Plane

    let vertexPositions = [|
        [|  5.0f; -0.5f;  5.0f |]
        [| -5.0f; -0.5f;  5.0f |]
        [| -5.0f; -0.5f; -5.0f |]

        [|  5.0f; -0.5f;  5.0f |]
        [| -5.0f; -0.5f; -5.0f |]
        [|  5.0f; -0.5f; -5.0f |]
    |]

    let textureCoords = [|
        [| 2.0f; 0.0f |]
        [| 0.0f; 0.0f |]
        [| 0.0f; 2.0f |]

        [| 2.0f; 0.0f |]
        [| 0.0f; 2.0f |]
        [| 2.0f; 2.0f |]
    |]

    let vertexPositionsAndTextureCoords = 
        vertexPositions
        |> Array.indexed
        |> Array.map (fun (idx, position) ->
            [| position; textureCoords.[idx] |]
        )