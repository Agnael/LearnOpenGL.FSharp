module Model
    open System.Numerics
    open Galante.OpenGL
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.PixelFormats

    #nowarn "9"
    open Assimp
    open System.IO
    open Mesh
    open System
    open Vertex
    
    let toSystemV3 (assimpVector3d: Vector3D) =
        new Vector3(assimpVector3d.X, assimpVector3d.Y, assimpVector3d.Z)
                    
    let rec getMeshes (aiModel: Scene) (currAiNode: Node) (textureContainingDir: string) (ctx: GlWindowCtx) (textureLoadHandler: string -> unit) =
        
        //printfn "> Loading Assimp Scene Node"

        Array.empty<Mesh.Mesh>
        |> fun meshes ->
            if currAiNode.HasMeshes then
                currAiNode.MeshIndices
                |> Array.ofSeq
                |> Array.map (
                    fun meshIdx -> 
                        let aiMesh = aiModel.Meshes.[meshIdx]
                        let vertices =
                            aiMesh.Vertices
                            |> Array.ofSeq
                            |> Array.indexed
                            |> Array.map 
                                (fun (idx, currAiVector3d) -> 
                                    //let position = toSystemV3 currAiVector3d
                                    let position = toSystemV3 aiMesh.Vertices.[idx]
                                    let normal =
                                        if aiMesh.HasNormals then
                                            toSystemV3 aiMesh.Normals.[idx]
                                        else 
                                            new Vector3(0.0f, 0.0f, 0.0f)

                                    // A vertex can contain up to 8 different 
                                    // texture coordinates. We assume that we 
                                    // won't use models where a vertex can 
                                    // have multiple texture coordinates so we 
                                    // always take the first set (0).
                                    // TODO: Research about multi-coords usage.
                                    let textureCoords =
                                        aiMesh.TextureCoordinateChannels.[0].[idx]
                                        |> toSystemV3
                                        |> fun v3 -> new Vector2(v3.X, v3.Y)

                                    { Vertex.Position = position
                                    ; Vertex.Normal = normal
                                    ; Vertex.TextureCoordinates = textureCoords
                                    ;}
                                )

                        let indices =
                            aiMesh.Faces
                            |> Array.ofSeq
                            |> Array.collect 
                                (fun face -> 
                                    face.Indices
                                    |> Array.ofSeq
                                    |> Array.map (fun faceIdx -> uint faceIdx)
                                )
                            // This is invented on the go, i probably shouldn't 
                            // be reversing the list.
                            |> Array.rev

                        let meshMaterial = 
                            aiModel.Materials.[aiMesh.MaterialIndex]

                        let vao = 
                            GlVao.create ctx 
                            |> GlVao.bind
                            |> fst

                        //let vboAttrDefinitions =
                        //    vertices
                        //    |> Array.map 
                        //        (fun v -> 
                        //            let attrDefArray = Array.create 8 [||]
                                    
                        //            let positionArr = Array.create 3 0.0f
                        //            let normalArr = Array.create 3 0.0f
                        //            let textureCoordsArr = Array.create 2 0.0f

                        //            Array.set positionArr 0 v.Position.X
                        //            Array.set positionArr 1 v.Position.Y
                        //            Array.set positionArr 2 v.Position.Z

                        //            Array.set normalArr 0 v.Normal.X
                        //            Array.set normalArr 1 v.Normal.Y
                        //            Array.set normalArr 2 v.Normal.Z

                        //            Array.set textureCoordsArr 0 v.TextureCoordinates.X
                        //            Array.set textureCoordsArr 1 v.TextureCoordinates.Y

                        //            // Attr set
                        //            Array.set attrDefArray 0 positionArr
                        //            Array.set attrDefArray 1 normalArr
                        //            Array.set attrDefArray 2 textureCoordsArr

                        //            attrDefArray
                        //        )

                        let vboAttrDefinitions =
                            vertices
                            |> Array.map (fun v ->
                                [| 
                                    [| v.Position.X; v.Position.Y; v.Position.Z |] 
                                    [| v.Normal.X; v.Normal.Y; v.Normal.Z |] 
                                    [| v.TextureCoordinates.X; v.TextureCoordinates.Y |]  
                                |]
                            )

                        // Create VBO
                        GlVbo.emptyVboBuilder
                        |> GlVbo.withAttrNames
                            ["Positions"; "Normals"; "TextureCoordinates"]
                        |> GlVbo.withAttrDefinitions vboAttrDefinitions
                        |> GlVbo.build (vao, ctx)
                        |> ignore

                        // Create EBO
                        GlEbo.create ctx indices 
                        |> ignore
                        
                        let textures = 
                            meshMaterial.GetAllMaterialTextures()
                            |> Array.map 
                                (fun aiTexture -> 
                                    let textureFilePath =
                                        Path.Combine(textureContainingDir, aiTexture.FilePath)

                                    textureLoadHandler textureFilePath
                                                                        
                                    // TODO: Assimp seems to be confussing 
                                    // normal maps as height maps. After some
                                    // googling I see this is a known issue
                                    // so I should rely on something different
                                    // than the Assimg.TextureType to create
                                    // my textures, but I should decide that 
                                    // later, after finishing the course and
                                    // having a better understanding of how
                                    // should I structure my directories and
                                    // projects.
                                    match aiTexture.TextureType with
                                    | TextureType.Normals -> 
                                        Some <| Texture.NormalMap textureFilePath 
                                    | TextureType.Height ->
                                        Some <| Texture.NormalMap textureFilePath 

                                    | TextureType.Diffuse -> 
                                        Some <| Texture.DiffuseMap textureFilePath 
                                    | TextureType.Specular -> 
                                        Some <| Texture.SpecularMap textureFilePath 
                                    | TextureType.Emissive -> 
                                        Some <| Texture.EmissionMap textureFilePath 

                                    | _ -> None
                                )
                            |> Array.filter (fun maybeTex -> maybeTex.IsSome)
                            |> Array.map (fun x -> x.Value)
                                                    
                        { Mesh.Vertices = vertices
                        ; Mesh.Indices = indices
                        ; Mesh.Textures = textures
                        ; Mesh.Vao = vao
                        ;}
                    )
                |> fun mapped -> 
                    Array.concat [meshes; mapped]
            else meshes
        |> fun meshes ->
            if currAiNode.HasChildren then
                //printfn "   -> This node has children, so they are used as used as nodes to load their meshes as well."
                currAiNode.Children
                |> Array.ofSeq
                |> Array.indexed
                |> Array.collect 
                    (fun (i, childNode) ->
                        //printfn "      => Loading child [idx=%i]" i
                        let childMeshes = getMeshes aiModel childNode textureContainingDir ctx textureLoadHandler
                        Array.concat [meshes; childMeshes]
                    )
            else meshes
    
    let BasicPostProcessSteps = 
        PostProcessSteps.Triangulate |||
        PostProcessSteps.GenerateSmoothNormals |||
        PostProcessSteps.CalculateTangentSpace

    let private aiLoad additionalPostProcessSteps path : Assimp.Scene =
        let aiImporter = new AssimpContext()

        let postLoadSteps = 
            BasicPostProcessSteps ||| 
            PostProcessSteps.FlipUVs

        let postLoadSteps =
            BasicPostProcessSteps::additionalPostProcessSteps
            |> List.reduce (fun s1 s2 -> s1 ||| s2)

        // Load Assimp Model type (AI - AssImp - AsssetImporter)
        aiImporter.ImportFile(path, postLoadSteps)

    type Model =
        { Directory: string
        ; Meshes: Mesh array
        //; Nodes: ModelNode
        //; Materials: Material list
        //; Textures: Texture list
        //; Animations: Animation list
        //; Lights: Light list
        //; Cameras: Camera list
        ;}

        static member load getAiMdl (path:  string) ctx (loadTex: string->unit) =         
            let aiModel = getAiMdl path

            let dirSeparatorLastIdx = 
                path.LastIndexOf(Path.DirectorySeparatorChar);

            let directory = path.Substring(0, dirSeparatorLastIdx);            
            let meshes = 
                getMeshes aiModel aiModel.RootNode directory ctx loadTex
            
            { Directory = directory
            ; Meshes = meshes
            ;}

        /// <summary>
        /// Load Flipped: Loads the model and flips it's UVs over the Y-axis.
        /// </summary>
        static member loadF = Model.load (aiLoad [PostProcessSteps.FlipUVs])
        
        /// <summary>
        /// Load Unlipped: Loads the model with it's unmodified UV coordinates.
        /// </summary>
        static member loadU = Model.load (aiLoad [])

        static member draw mdl program (getTexture: string->GlTexture) ctx =
            mdl.Meshes
            |> Array.iter (fun mesh ->
                Mesh.draw mesh program getTexture ctx
            ) 