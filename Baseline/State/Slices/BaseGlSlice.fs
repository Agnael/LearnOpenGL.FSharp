module BaseGlSlice
   #nowarn "9"
   #nowarn "51"
   open GalanteMath
   open System.Numerics
   open Galante.OpenGL
   open System
   open Silk.NET.OpenGL
   open Microsoft.FSharp.NativeInterop
   open Gl

   type SharedUboShaderRef = {
      Shader: GlProgram
      UniformBlockIndex: uint32
   }

   type SharedUbo = {
      Ubo: GlUniformBlock
      BoundShaders: SharedUboShaderRef list
   }

   type GlAction =  
      | BindShaderToUbo of GlUniformBlockDefinition * SharedUboShaderRef
      | AddSharedUbo of GlUniformBlock

   type GlState = 
      { SharedUbos: SharedUbo list
      ;}
    
      static member Default = {
         SharedUbos = []
      }
   
   let listen (state: GlState) action dispatch (ctx: GlWindowCtx) =
       match action with
       | BindShaderToUbo (uboDef, shader) -> ()
       | _ -> ()

   let reduce action state =
      match action with
      | AddSharedUbo ubo -> 
         let newSharedUbo = { Ubo = ubo; BoundShaders = [] }
         { state with SharedUbos = newSharedUbo::state.SharedUbos }
      | BindShaderToUbo (uboDef, shaderRef) ->
         let updatedSharedUbo =
            state.SharedUbos
            |> List.find (fun x -> x.Ubo.Definition.Name = uboDef.Name)
            |> fun sharedUbo -> {
                  sharedUbo with BoundShaders = shaderRef::sharedUbo.BoundShaders
               }

         {
            state with
               SharedUbos =
                  state.SharedUbos
                  |> List.filter (
                     fun x -> not (x.Ubo.Definition.Name = uboDef.Name)
                  )
                  |> fun newList -> updatedSharedUbo::newList
         }

         // Filters the current uboDef out
      | _ -> state