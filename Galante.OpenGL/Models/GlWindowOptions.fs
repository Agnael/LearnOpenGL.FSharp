namespace Galante.OpenGL

    open System.Drawing
    open Silk.NET.Windowing
    open Silk.NET.Windowing.Glfw
    open Microsoft.Extensions.Logging
    open Microsoft.Extensions.FileProviders

    type GlWindowOptions = 
        { FileProvider: IFileProvider option
        ; Logger: ILogger option
        ; IsVisible: bool
        ; UseSingleThreadedWindow: bool
        ; Position: Point
        ; Size: Size
        ; FramesPerSecond: double
        ; UpdatesPerSecond: double
        ; Api: GraphicsAPI
        ; Title: string
        ; WindowState: WindowState
        ; WindowBorder: WindowBorder
        ; IsVsync: bool
        ; RunningSlowTolerance: int
        ; ShouldSwapAutomatically: bool
        ; VideoMode: VideoMode
        ; PreferredDepthBufferBits: int option
        ; PreferredStencilBufferBits: int option
        ; PreferredBitDepth: Silk.NET.Maths.Vector4D<int> option
        ; TransparentFrameBuffer: bool
        ; IsEventDriven: bool
        ; SharedContext: Silk.NET.Core.Contexts.IGLContext option 
        ;}

        static member Default =
            { FileProvider = None
            ; Logger = None
            ; IsVisible = true
            ; UseSingleThreadedWindow = false
            ; Position = new Point (50, 50) 
            ; Size = new Size (600, 400)
            ; FramesPerSecond = 0.0
            ; UpdatesPerSecond = 0.0
            ; Api = GraphicsAPI.Default
            ; Title = "Default window title"
            ; WindowState = WindowState.Normal
            ; WindowBorder = WindowBorder.Resizable
            ; IsVsync = false
            ; RunningSlowTolerance = 5 // TODO: 5 qué??? lo tomé así del fuente de SILK
            ; ShouldSwapAutomatically = true
            ; VideoMode = VideoMode.Default
            ; PreferredDepthBufferBits = Some 24
            ; PreferredStencilBufferBits = Some 8
            ; PreferredBitDepth = None
            ; TransparentFrameBuffer = false
            ; IsEventDriven = false
            ; SharedContext = None
            ;}