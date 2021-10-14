module Mesh
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.PixelFormats
    open Silk.NET.OpenGL
    open Galante.OpenGL
    open Vertex
    open Texture
    open System

    [<Struct>]
    type Mesh =
        { Vertices: Vertex array
        ; Textures: Texture array
        ; Indices: uint array
        ; Vao: GlVertexArrayObject
        ;}
         static member preDraw mesh program (getTexture: string->GlTexture) ctx =
            // Temporal renderer, will only bind the last diffuse and specular
            // maps it finds.
            mesh.Textures
            |> Array.iter (fun texture ->
                  match texture with
                  | DiffuseMap texFilePath ->
                     let glTexture = getTexture texFilePath
        
                     (mesh.Vao, ctx)
                     |> GlTex.setActive GLEnum.Texture0 glTexture
                     |> ignore
        
                     (program, ctx)
                     |> GlProg.setUniformI "uMaterial.diffuseMap" 0 
                     |> ignore
                       
                     ctx.Gl.BindTexture (GLEnum.Texture2D, glTexture.GlTexHandle)
                  | SpecularMap texFilePath ->
                     let glTexture = getTexture texFilePath
        
                     (mesh.Vao, ctx)
                     |> GlTex.setActive GLEnum.Texture1 glTexture
                     |> ignore
        
                     (program, ctx)
                     |> GlProg.setUniformI "uMaterial.specularMap" 1 
                     |> ignore
                   
                     ctx.Gl.BindTexture (GLEnum.Texture2D, glTexture.GlTexHandle)
                  //| NormalMap texFilePath ->
                  //    let glTexture = getTexture texFilePath
        
                  //    (mesh.Vao, ctx)
                  //    |> GlTex.setActive GLEnum.Texture2 glTexture
                  //    |> ignore
        
                  //    (program, ctx)
                  //    |> GlProg.setUniformI "uMaterial.normalMap" 2 
                  //    |> ignore
                   
                  //    ctx.Gl.BindTexture (GLEnum.Texture2D, glTexture.GlTexHandle)
                  | _ ->
                     ()
            )
               
            (program, ctx)
            |> GlProg.setUniformF "uMaterial.shininess" 64.0f
            |> ignore

        static member draw mesh program (getTexture: string->GlTexture) ctx =
            GlVao.bind (mesh.Vao, ctx)
            |> ignore

            Mesh.preDraw mesh program getTexture ctx

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

               Mesh.preDraw mesh program getTexture ctx

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