module Quad
   open GalanteMath
   open System.Numerics

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

   // It's alwaays the same normal since it's a single face
   let normal = [| 0.0f; 0.0f; 1.0f |]

   let normals = [|
      normal
      normal
      normal
      normal
   |]   

   let vertexPositionsAndNormalsAndTextureCoordsAndTangentsBitangents =
      // Intentionally index mismatches because the LearnOpenGL example's
      // vertices are different but I wanted to reuse my local quad vertex positions
      let pos1 = v3FromArray vertexPositions.[0]
      let pos2 = v3FromArray vertexPositions.[2]
      let pos3 = v3FromArray vertexPositions.[3]
      let pos4 = v3FromArray vertexPositions.[1]

      let uv1 = v2FromArray textureCoords.[0]
      let uv2 = v2FromArray textureCoords.[2]
      let uv3 = v2FromArray textureCoords.[3]
      let uv4 = v2FromArray textureCoords.[1]

      let calculateTangentBitangent (p1: Vector3) p2 p3 (uv1: Vector2) uv2 uv3 =
         let edge1 = p2 - p1
         let edge2 = p3 - p1
   
         let deltaUv1 = uv2 - uv1
         let deltaUv2 = uv3 - uv1

         let f = 1.0f / (deltaUv1.X * deltaUv2.Y - deltaUv2.X * deltaUv1.Y)

         let tan1x = f * (deltaUv2.Y * edge1.X - deltaUv1.Y * edge2.X)
         let tan1y = f * (deltaUv2.Y * edge1.Y - deltaUv1.Y * edge2.Y)
         let tan1z = f * (deltaUv2.Y * edge1.Z - deltaUv1.Y * edge2.Z)
         
         let bitan1x = f * (-deltaUv2.X * edge1.X + deltaUv1.X * edge2.X)
         let bitan1y = f * (-deltaUv2.X * edge1.Y + deltaUv1.X * edge2.Y)
         let bitan1z = f * (-deltaUv2.X * edge1.Z + deltaUv1.X * edge2.Z)
         
         (normalize <| v3 tan1x tan1y tan1z, normalize <| v3 bitan1x bitan1y bitan1z)
      
      // Triangle 1
      let (tan1, bitan1) = calculateTangentBitangent pos1 pos2 pos3 uv1 uv2 uv3
      let (tan2, bitan2) = calculateTangentBitangent pos1 pos3 pos4 uv1 uv3 uv4

      [|
         [| v3AsArrayF pos1; normal; v2AsArrayF uv1; v3AsArrayF tan1; v3AsArrayF bitan1 |]
         [| v3AsArrayF pos2; normal; v2AsArrayF uv2; v3AsArrayF tan1; v3AsArrayF bitan1 |]
         [| v3AsArrayF pos3; normal; v2AsArrayF uv3; v3AsArrayF tan1; v3AsArrayF bitan1 |]

         [| v3AsArrayF pos1; normal; v2AsArrayF uv1; v3AsArrayF tan2; v3AsArrayF bitan2 |]
         [| v3AsArrayF pos3; normal; v2AsArrayF uv3; v3AsArrayF tan2; v3AsArrayF bitan2 |]
         [| v3AsArrayF pos4; normal; v2AsArrayF uv4; v3AsArrayF tan2; v3AsArrayF bitan2 |]
      |]

   let vertexPositionsAndTextureCoords: single [][][] = 
      vertexPositions
      |> Array.indexed
      |> Array.map (fun (idx, position) ->
         [| position; textureCoords.[idx] |]
      )