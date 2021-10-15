module GlWin
   open Silk.NET.Windowing
   open Silk.NET.OpenGL
   open Galante.OpenGL
   open System
   open Microsoft.Extensions.Logging.Abstractions
   open Microsoft.Extensions.FileProviders
   open System.IO
   open Silk.NET.Input
   open Silk.NET.Windowing.Glfw
   open Silk.NET.GLFW
   open Microsoft.Extensions.Logging

   let create (options: GlWindowOptions): GlWindowCtx =    
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

      let logger = 
         match options.Logger with
         | Some logger -> logger
         | None -> 
            new NullLogger<obj>() 
            :> Microsoft.Extensions.Logging.ILogger

      let window = Window.Create(windowOptions)
      
      // Applies the antialiasing configs
      let defaultSampleCount = 
         GlWindowOptions.Default.MainFramebufferSampleCount

      if not (options.MainFramebufferSampleCount = defaultSampleCount) then
         if GlfwWindowing.IsViewGlfw(window) then
            GlfwWindowing
               .GetExistingApi(window)
               .WindowHint(WindowHintInt.Samples, 16)
         else
            logger.LogError 
               $"Can't set '{options.MainFramebufferSampleCount}' as a custom
               sampler count for then mail framebuffer because the current
               execution is not using GLFW under then hood, and this code is
               limited to only work for GLFW. The default sampler count will
               be used for whatever windowing system is being used."

      let windowContext = 
         { Gl = GL.GetApi(window) 
         ; Window = window
         ; Logger = logger
         ; FileProvider = 
            new PhysicalFileProvider(Directory.GetCurrentDirectory())
         ;}
                 
      windowContext