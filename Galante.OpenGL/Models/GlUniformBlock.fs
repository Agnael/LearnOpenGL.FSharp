namespace Galante.OpenGL

open Silk.NET.OpenGL

// Data types we can use within a shader. Each one of them has a number
// assigned, which represents the bytes of base alignment it needs ().
[<RequireQualifiedAccess>]
type GlslDataType =
   | Uint
   | Int
   | Float
   | Double
   | Bool
   | Vec3
   | Vec4

[<Struct>]
type GlUniformBlock = {
   // Block indexes are actually shader specific
   //GlUniformBlockIndex: uint32
   Name: string
}