module GlWin

    open Silk.NET.Windowing
    open Silk.NET.OpenGL
    open Galante
    open System

    let create (options: GlWindowOptions): (IWindow * GL) =    
        let windowOptions =
            new WindowOptions
                ( options.IsVisible
                , new Silk.NET.Maths.Vector2D<int>(
                    options.Position.X, 
                    options.Position.Y)
                , new Silk.NET.Maths.Vector2D<int>(
                    options.Size.Width, 
                    options.Size.Height)
                , options.FramesPerSecond
                , options.UpdatesPerSecond
                , options.Api
                , options.Title
                , options.WindowState
                , options.WindowBorder
                , options.IsVsync
                , options.ShouldSwapAutomatically
                , options.VideoMode
                , (match options.PreferredDepthBufferBits with
                    | Some x -> new Nullable<int>(x) | None -> new Nullable<int>())
                , (match options.PreferredStencilBufferBits with
                    | Some x -> new Nullable<int>(x) | None -> new Nullable<int>())
                , (match options.PreferredBitDepth with
                    | Some x -> new Nullable<Silk.NET.Maths.Vector4D<int>>(x) 
                    | None -> new Nullable<Silk.NET.Maths.Vector4D<int>>())
                , options.TransparentFrameBuffer
                , options.IsEventDriven
                , (match options.SharedContext with
                    | Some c -> c | None -> null))

        let window = Window.Create(windowOptions)
        let gl = GL.GetApi(window)
        (window, gl)