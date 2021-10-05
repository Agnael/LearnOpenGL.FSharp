module GlUbo

open Galante.OpenGL
open Silk.NET.OpenGL

let matricesUniformDef: GlUniformBlockDefinition = {
   Name = "Matrices"
   UniformNames = ["uProjection"; "uView"]
}

type UboCandidate = 
   | DefinitionOnly of GlUniformBlockDefinition
   | Ubo of GlUniformBlock

// I´m just learning so i´ll be incrementing this as much as i need, and no
// UBO will be ever removed. Their slots will remain reserved forever.
let mutable uboDefMap = Map.empty<string, UboCandidate>

let uboCreate ctx =
   let handle = ctx.Gl.GenBuffer()
   ctx.Gl.BindBuffer(BufferTargetARB.UniformBuffer, handle)

// Not really functional programming, innit
let uboRegisterUsage shader (uboDef: GlUniformBlockDefinition) ctx =
   uboDefMap.TryFind uboDef.Name
   |> function
      | Some candidate -> ()
      | None -> ()