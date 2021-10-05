namespace Galante.OpenGL

open Silk.NET.OpenGL

[<Struct>]
type GlUniformBlock = {
   GlUboHandle: uint32
   UniformBlockBindingIndex: uint32
   Definition: GlUniformBlockDefinition
}