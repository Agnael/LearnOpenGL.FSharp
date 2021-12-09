module Quad

   let vertexPositions = [|
      [| -1.0f;  1.0f; 0.0f |]
      [| -1.0f; -1.0f; 0.0f |]
      [|  1.0f;  1.0f; 0.0f |]
      [|  1.0f; -1.0f; 0.0f |]
   |]

   let textureCoords = [|
      [| 0.0f; 1.0f |]
      [| 0.0f; 0.0f |]
      [| 1.0f; 1.0f |]
      [| 1.0f; 0.0f |]
   |]

   let vertexPositionsAndTextureCoords: single [][][] = 
      vertexPositions
      |> Array.indexed
      |> Array.map (fun (idx, position) ->
         [| position; textureCoords.[idx] |]
      )