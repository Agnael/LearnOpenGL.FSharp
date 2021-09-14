[<RequireQualifiedAccess>]
module CubeCCW
   open System.Numerics
   open Vertex

   // Remember: to specify vertices in a counter-clockwise winding order you need to visualize the triangle
   // as if you're in front of the triangle and from that point of view, is where you set their order.
        
   // To define the order of a triangle on the right side of the cube for example, you'd imagine yourself looking
   // straight at the right side of the cube, and then visualize the triangle and make sure their order is specified
   // in a counter-clockwise order. This takes some practice, but try visualizing this yourself and see that this
   // is correct.
   let private positions = [|
      // Back face
      [| -0.5f; -0.5f; -0.5f |]
      [|  0.5f;  0.5f; -0.5f |]
      [|  0.5f; -0.5f; -0.5f |]
      [|  0.5f;  0.5f; -0.5f |]
      [| -0.5f; -0.5f; -0.5f |]
      [| -0.5f;  0.5f; -0.5f |]
            
      // Front face
      [| -0.5f; -0.5f;  0.5f |]
      [|  0.5f; -0.5f;  0.5f |]
      [|  0.5f;  0.5f;  0.5f |]
      [|  0.5f;  0.5f;  0.5f |]
      [| -0.5f;  0.5f;  0.5f |]
      [| -0.5f; -0.5f;  0.5f |]
            
      // Left face
      [| -0.5f;  0.5f;  0.5f |]
      [| -0.5f;  0.5f; -0.5f |]
      [| -0.5f; -0.5f; -0.5f |]
      [| -0.5f; -0.5f; -0.5f |]
      [| -0.5f; -0.5f;  0.5f |]
      [| -0.5f;  0.5f;  0.5f |]
            
      // Right face
      [|  0.5f;  0.5f;  0.5f |]
      [|  0.5f; -0.5f; -0.5f |]
      [|  0.5f;  0.5f; -0.5f |]
      [|  0.5f; -0.5f; -0.5f |]
      [|  0.5f;  0.5f;  0.5f |]
      [|  0.5f; -0.5f;  0.5f |]

      // Bottom face
      [| -0.5f; -0.5f; -0.5f |]
      [|  0.5f; -0.5f; -0.5f |]
      [|  0.5f; -0.5f;  0.5f |]
      [|  0.5f; -0.5f;  0.5f |]
      [| -0.5f; -0.5f;  0.5f |]
      [| -0.5f; -0.5f; -0.5f |]
       
      // Top face     
      [| -0.5f;  0.5f; -0.5f |]
      [|  0.5f;  0.5f;  0.5f |]
      [|  0.5f;  0.5f; -0.5f |]
      [|  0.5f;  0.5f;  0.5f |]
      [| -0.5f;  0.5f; -0.5f |]
      [| -0.5f;  0.5f;  0.5f |]
   |]

   let private normals = [|
      [|  0.0f; 0.0f; -1.0f |]
      [|  0.0f;  0.0f; -1.0f |]
      [|  0.0f;  0.0f; -1.0f |]
      [|  0.0f;  0.0f; -1.0f |]
      [|  0.0f;  0.0f; -1.0f |]
      [|  0.0f;  0.0f; -1.0f |]
                       
      [|  0.0f;  0.0f; 1.0f |]
      [|  0.0f;  0.0f; 1.0f |]
      [|  0.0f;  0.0f; 1.0f |]
      [|  0.0f;  0.0f; 1.0f |]
      [|  0.0f;  0.0f; 1.0f |]
      [|  0.0f;  0.0f; 1.0f |]
                       
      [| -1.0f;  0.0f;  0.0f |]
      [| -1.0f;  0.0f;  0.0f |]
      [| -1.0f;  0.0f;  0.0f |]
      [| -1.0f;  0.0f;  0.0f |]
      [| -1.0f;  0.0f;  0.0f |]
      [| -1.0f;  0.0f;  0.0f |]
                       
      [|  1.0f;  0.0f;  0.0f |]
      [|  1.0f;  0.0f;  0.0f |]
      [|  1.0f;  0.0f;  0.0f |]
      [|  1.0f;  0.0f;  0.0f |]
      [|  1.0f;  0.0f;  0.0f |]
      [|  1.0f;  0.0f;  0.0f |]
                       
      [|  0.0f; -1.0f;  0.0f |]
      [|  0.0f; -1.0f;  0.0f |]
      [|  0.0f; -1.0f;  0.0f |]
      [|  0.0f; -1.0f;  0.0f |]
      [|  0.0f; -1.0f;  0.0f |]
      [|  0.0f; -1.0f;  0.0f |]
                       
      [|  0.0f;  1.0f;  0.0f |]
      [|  0.0f;  1.0f;  0.0f |]
      [|  0.0f;  1.0f;  0.0f |]
      [|  0.0f;  1.0f;  0.0f |]
      [|  0.0f;  1.0f;  0.0f |]
      [|  0.0f;  1.0f;  0.0f |]
   |]

   let private textureCoords = [| 
      // Back face
      [| 0.0f; 0.0f |]
      [| 1.0f; 1.0f |]
      [| 1.0f; 0.0f |]
      [| 1.0f; 1.0f |]
      [| 0.0f; 0.0f |]
      [| 0.0f; 1.0f |]
           
      // Front face
      [| 0.0f; 0.0f |]
      [| 1.0f; 0.0f |]
      [| 1.0f; 1.0f |]
      [| 1.0f; 1.0f |]
      [| 0.0f; 1.0f |]
      [| 0.0f; 0.0f |]
      
      // Left face     
      [| 1.0f; 0.0f |]
      [| 1.0f; 1.0f |]
      [| 0.0f; 1.0f |]
      [| 0.0f; 1.0f |]
      [| 0.0f; 0.0f |]
      [| 1.0f; 0.0f |]
      
      // Right face     
      [| 1.0f; 0.0f |]
      [| 0.0f; 1.0f |]
      [| 1.0f; 1.0f |]
      [| 0.0f; 1.0f |]
      [| 1.0f; 0.0f |]
      [| 0.0f; 0.0f |]
      
      // Bottom face     
      [| 0.0f; 1.0f |]
      [| 1.0f; 1.0f |]
      [| 1.0f; 0.0f |]
      [| 1.0f; 0.0f |]
      [| 0.0f; 0.0f |]
      [| 0.0f; 1.0f |]
      
      // Top face     
      [| 0.0f; 1.0f |]
      [| 1.0f; 0.0f |]
      [| 1.0f; 1.0f |]
      [| 1.0f; 0.0f |]
      [| 0.0f; 1.0f |]
      [| 0.0f; 0.0f |]
   |]

   let vertexPositions = Array.map (fun pos -> [| pos |]) positions
    
   let vertexPositionsAndTextureCoords = 
      positions
      |> Array.indexed
      |> Array.map (fun (idx, position) ->
         [| position; textureCoords.[idx] |]
      )
    
   let vertexPositionsAndNormals =
      positions
      |> Array.indexed
      |> Array.map (fun (idx, position) ->
         [| position; normals.[idx] |]
      )

   let vertexPositionsAndNormalsAndTextureCoords =
      positions
      |> Array.indexed
      |> Array.map (fun (idx, position) ->
         [| position; normals.[idx]; textureCoords.[idx] |]
      )

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
        