module Mesh
   open SixLabors.ImageSharp
   open SixLabors.ImageSharp.PixelFormats
   open Silk.NET.OpenGL
   open Galante.OpenGL
   open Vertex
   open Texture
   open System

   let defaultDiffuseMapUniformName = "uMaterial.diffuseMap";
   let defaultSpecularMapUniformName = "uMaterial.specularMap";
   let defaultNormalMapUniformName = "uMaterial.normalMap";
   let defaultShininessUniformName = "uMaterial.shininess";    

   [<Struct>]
   type Mesh =
      { Vertices: Vertex array
      ; Textures: Texture array
      ; Indices: uint array
      ; Vao: GlVertexArrayObject
      ;}
      static member preDraw 
         mesh 
         program 
         (diffuseMapUniformName: string option)
         (specularMapUniformName: string option)
         (normalMapUniformName: string option)
         (shininessMapUniformName: string option)
         (getTexture: string->GlTexture) 
         ctx =
         // Temporal renderer, will only bind the last diffuse and specular
         // maps it finds.
         mesh.Textures
         |> Array.iter (fun texture ->
            match texture with
            | DiffuseMap texFilePath ->
               match diffuseMapUniformName with
               | Some diffuseMapUniformName ->
                  let glTexture = getTexture texFilePath
        
                  // Because of my poor API design, I end up having binding conflicts between manually set
                  // bindings and this automated ones for models, so I'll just use binding ports with "high"
                  // numbers starting from 20 to avoid conflicts.
                  (mesh.Vao, ctx)
                  |> GlTex.setActive GLEnum.Texture20 glTexture
                  |> ignore
        
                  (program, ctx)
                  |> GlProg.setUniformI diffuseMapUniformName 20
                  |> ignore
                       
                  ctx.Gl.BindTexture (GLEnum.Texture2D, glTexture.GlTexHandle)
               | None -> ()
            | SpecularMap texFilePath ->
               match specularMapUniformName with
               | Some specularMapUniformName ->
                  let glTexture = getTexture texFilePath
        
                  (mesh.Vao, ctx)
                  |> GlTex.setActive GLEnum.Texture21 glTexture
                  |> ignore
        
                  (program, ctx)
                  |> GlProg.setUniformI specularMapUniformName 21 
                  |> ignore
                   
                  ctx.Gl.BindTexture (GLEnum.Texture2D, glTexture.GlTexHandle)
               | None -> ()
            | NormalMap texFilePath ->
               match normalMapUniformName with
               | Some normalMapUniformName ->
                  let glTexture = getTexture texFilePath
        
                  (mesh.Vao, ctx)
                  |> GlTex.setActive GLEnum.Texture22 glTexture
                  |> ignore
        
                  
                  (program, ctx)
                  |> GlProg.setUniformI normalMapUniformName 22
                  |> ignore
                   
                  ctx.Gl.BindTexture (GLEnum.Texture2D, glTexture.GlTexHandle)
               | None -> ()
            | _ ->
               ()
         )
         
         match shininessMapUniformName with
         | Some shininessMapUniformName ->
            (program, ctx)
            |> GlProg.setUniformF shininessMapUniformName 64.0f
            |> ignore
         | None -> ()

      static member draw mesh program (getTexture: string->GlTexture) ctx =
         GlVao.bind (mesh.Vao, ctx)
         |> ignore

         Mesh.preDraw 
            mesh 
            program 
            (Some defaultDiffuseMapUniformName)
            (Some defaultSpecularMapUniformName)
            None
            (Some defaultShininessUniformName)
            getTexture 
            ctx

         GlVao.bind (mesh.Vao, ctx)
         |> ignore
            
         ctx.Gl.DrawElements 
            ( GLEnum.Triangles
            , uint32 mesh.Indices.Length
            , GLEnum.UnsignedInt
            , IntPtr.Zero.ToPointer()
            )            

      static member drawWithUniformNames 
         mesh 
         program 
         diffuseMapUniformName
         specularMapUniformName
         normalMapUniformName
         shininessUniformName
         (getTexture: string->GlTexture) 
         ctx =
         GlVao.bind (mesh.Vao, ctx)
         |> ignore

         Mesh.preDraw 
            mesh 
            program 
            diffuseMapUniformName
            specularMapUniformName
            normalMapUniformName
            shininessUniformName
            getTexture 
            ctx

         GlVao.bind (mesh.Vao, ctx)
         |> ignore
            
         ctx.Gl.DrawElements 
            ( GLEnum.Triangles
            , uint32 mesh.Indices.Length
            , GLEnum.UnsignedInt
            , IntPtr.Zero.ToPointer()
            )

      static member drawInstanced 
         mesh program (getTexture: string->GlTexture) instanceCount ctx =
            GlVao.bind (mesh.Vao, ctx)
            |> ignore

            Mesh.preDraw 
               mesh 
               program 
               (Some defaultDiffuseMapUniformName)
               (Some defaultSpecularMapUniformName)
               None
               (Some defaultShininessUniformName)
               getTexture 
               ctx

            GlVao.bind (mesh.Vao, ctx)
            |> ignore
            
            ctx.Gl.DrawElementsInstanced 
               ( GLEnum.Triangles
               , uint32 mesh.Indices.Length
               , GLEnum.UnsignedInt
               , IntPtr.Zero.ToPointer()
               , instanceCount
               )

      static member drawWireframe mesh shader ctx =   
         GlVao.bind (mesh.Vao, ctx)
         |> ignore
            
         ctx.Gl.DrawElements 
            ( GLEnum.Lines
            , uint32 mesh.Indices.Length
            , GLEnum.UnsignedInt
            , IntPtr.Zero.ToPointer()
            )

      static member drawTextureless mesh shader ctx =   
         GlVao.bind (mesh.Vao, ctx)
         |> ignore
            
         ctx.Gl.DrawElements 
            ( GLEnum.Triangles
            , uint32 mesh.Indices.Length
            , GLEnum.UnsignedInt
            , IntPtr.Zero.ToPointer()
            )