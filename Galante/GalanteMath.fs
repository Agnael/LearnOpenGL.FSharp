module GalanteMath
   open System
   open Galante.OpenGL
   open System.Numerics

   type Radians = 
      | Radians of float32
      static member make value = Radians(value)
      static member value (Radians r) = r

   type Degrees = 
      | Degrees of float32
      static member make value = Degrees(value)
      static member value (Degrees d) = d

   let inline v2 x y = new Vector2(x, y)
   let inline v3 x y z = new Vector3(x, y, z)

   let normalizeCross (v1, v2) = 
      Vector3.Cross(v1, v2) 
      |> Vector3.Normalize
        
   let toRadians (Degrees degrees) : Radians = 
      degrees * MathF.PI / 180.0f
      |> Radians
        
   let toDegrees (Radians radians) : Degrees =
      radians * 180.0f / MathF.PI
      |> Degrees
        
   let cosF degrees = 
      toRadians degrees
      |> Radians.value
      |> MathF.Cos
        
   let sinF degrees = 
      toRadians degrees
      |> Radians.value
      |> MathF.Sin

   /// <summary>
   ///     Returns the unsigned degrees of the shortest angle formed by the 
   ///     provided vectors. Values will always range from 0 to 180 degrees.
   /// </summary>
   let getAngleAbs (v1: Vector3) (v2: Vector3) =
      // Formula: v1.v2 = |v1|.|v2|.cos(angle)
      // then ==> angle = acos((v1.v2) / (|v1|.|v2|))
      Vector3.Dot (v1, v2) / (v1.Length() * v2.Length())
      |> MathF.Acos
      |> Radians.make
      |> toDegrees

   let getYaw (target: Vector3) =     
      let yawOrigin = new Vector3(1.0f, 0.0f, 0.0f)

      // If the camera is facing towards the positive Z, then the 
      // absolute angle is the correct one.
      // If the camera is facing towards the NEGATIVE Z AXIS, the final 
      // positive angle must be calculated assuming that the 
      // absolute angle is actually a negative angle in this 3d world.
      getAngleAbs yawOrigin (new Vector3(target.X, 0.0f, target.Z))
      |> fun (Degrees deg) -> 
         if MathF.Sign target.Z < 0 then 360.0f - deg
         else deg
      |> Degrees.make

   /// <summary>
   ///     The X-Axis is taken as the rotation center and, given an origin
   ///     vector to take as a reference for the angle's start and another 
   ///     vector as the target of the camera, the angle between 
   ///     them is returned, adjusted with a hacky -90.0f degree start
   ///     for the value.
   ///     TODO: The -90.0f adjustment is a hack that just happen to make
   ///     this work but it must be understood and removed.
   /// </summary>
   let getPitch (target: Vector3) =
      let zSign = single <| MathF.Sign target.Z
      let ySign = single <| MathF.Sign target.Y
      //let pitchOrigin = new Vector3(0.0f, 0.0f, zSign)
      let pitchOrigin = new Vector3(target.X, 0.0f, target.Z)

      let angle = 
         //getAngleAbs pitchOrigin (new Vector3(target.X, target.Y, 0.0f))
         getAngleAbs pitchOrigin target
        
      angle
      |> fun (Degrees v) -> if v > 89.9f then 89.9f else v
      |> fun v -> if MathF.Sign target.Y < 0 then -v else v
      |> Degrees.make