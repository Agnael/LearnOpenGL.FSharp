module BaseAssetSlice
    open GalanteMath
    open System.Numerics
    open Galante.OpenGL
    open StoreFactory
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.PixelFormats
    open System

    type ImageAsset =
        { Image: Image<Rgba32>
        ; FilePath: string
        ; GlTexture: GlTexture option
        ;}
        
    type AssetAction =
        | LoadImageStart of string
        | LoadImageStarted of string
        | LoadImageSuccess of string * ImageAsset
        | BindedImageAsTexture of string * GlTexture
        | TaskExecuted of Guid

    type AssetState = 
        { ImagesLoading: Set<string>
        ; ImagesLoaded: Map<string, ImageAsset>
        ; Tasks: (Guid * (AssetState -> (AssetAction->unit) -> GlWindowCtx -> unit)) list
        ;}
        static member Default =
            { ImagesLoading = Set.empty
            ; ImagesLoaded = Map.empty<string, ImageAsset>
            ; Tasks = List.Empty
            ;}

    let filterAction 
        (action: AssetAction) 
        (prevState: AssetState) 
        (dispatchA: ActionDispatcher<AssetAction>) 
        (dispatchIo: IoDispatcher<AssetAction>) 
        (ctx: GlWindowCtx) 
        : AssetAction option =

        match action with
        | LoadImageStart imgPath -> 
            let isAlreadyLoadingOrLoaded =
                prevState.ImagesLoaded
                |> Map.containsKey imgPath
                |> fun isLoaded ->
                    if isLoaded then 
                        true
                    else
                        prevState.ImagesLoading
                        |> Set.contains imgPath

            if isAlreadyLoadingOrLoaded then
                None
            else
                //dispatchIo (fun () ->
                //)
                Some (LoadImageStarted imgPath)
        | _ -> 
            None
    
    let listen (state) action dispatch (ctx: GlWindowCtx) =
        //printfn "Listening on thread [%i]" Threading.Thread.CurrentThread.ManagedThreadId
        match action with
        | LoadImageStart imgPath ->
            let isAlreadyLoadingOrLoaded =
                state.ImagesLoaded
                |> Map.containsKey imgPath
                |> fun isLoaded ->
                    if isLoaded then 
                        true
                    else
                        state.ImagesLoading
                        |> Set.contains imgPath

            if not isAlreadyLoadingOrLoaded then
                async {
                    //Threading.Thread.Sleep(5000)
                    //printfn "LOADING IMAGE PATH [%s] on thread [%i]" imgPath Threading.Thread.CurrentThread.ManagedThreadId
                    let img = GlTex.loadImage imgPath ctx
                    let imgAsset = 
                        { FilePath = imgPath
                        ; Image = img
                        ; GlTexture = None
                        ;}

                    dispatch (LoadImageSuccess (imgPath, imgAsset))
                }
                |> Async.Start
                dispatch (LoadImageStarted imgPath)
        | _ -> ()

    let reduce action prevState =
        //printfn "Reducing on thread [%i]" Threading.Thread.CurrentThread.ManagedThreadId
        match action with
        | LoadImageStarted texturePath ->
            { prevState with 
                ImagesLoading = 
                    prevState.ImagesLoading 
                    |> Set.add texturePath 
            }
        | LoadImageSuccess (texPath, texAsset) ->
            { prevState with 
                ImagesLoading = 
                    prevState.ImagesLoading
                    |> Set.filter (fun x -> not(x = texPath))
                ImagesLoaded =
                    prevState.ImagesLoaded
                    |> Map.add texPath texAsset
            }
        | TaskExecuted taskId ->
            { prevState with 
                Tasks =
                    prevState.Tasks
                    |> List.filter (fun (id, task) -> not(id = taskId)) }
        | BindedImageAsTexture (imgPath, glTexture) ->
            let updatedAsset = 
                Map.find imgPath prevState.ImagesLoaded
                |> fun x -> { x with GlTexture = Some glTexture }
            
            { prevState with
                ImagesLoaded =
                    prevState.ImagesLoaded
                    |> Map.filter (fun k v -> not(k = imgPath))
                    |> Map.add imgPath updatedAsset
            }
        | _ -> 
            prevState