[<RequireQualifiedAccess>]
module GlShad

    open Silk.NET.OpenGL
    open Galante.OpenGL
    open System.IO
    open Microsoft.Extensions.Logging

    let load (shaderType: ShaderType) srcFilePath (ctx: GlWindowCtx) =
          let sourceGlslCode = 
              ctx.FileProvider.GetFileInfo(srcFilePath)
              |> fun fileInfo ->
                  if not fileInfo.Exists
                  then failwith <| sprintf "GLSL source file '%s' was not found." srcFilePath
                  else fileInfo
              |> fun fileInfo -> fileInfo.CreateReadStream()
              |> fun fileStream -> new StreamReader(fileStream)
              |> fun streamReader -> streamReader.ReadToEndAsync()
              |> Async.AwaitTask
              |> Async.RunSynchronously
          
          let shaderHandle =  ctx.Gl.CreateShader shaderType
          ctx.Gl.ShaderSource (shaderHandle, sourceGlslCode)
          ctx.Gl.CompileShader shaderHandle
          
          let glStatus =
              let compileStatusCode = ctx.Gl.GetShader (shaderHandle, ShaderParameterName.CompileStatus)

              if compileStatusCode = 0 then
                  ctx.Gl.GetShaderInfoLog shaderHandle
                  |> fun errorLog ->
                      ctx.Logger.LogCritical 
                      <| sprintf 
                          "[SHADER '%s' COMPILATION ERROR] Log: %s" 
                          srcFilePath 
                          errorLog
                      GlShaderCompilationError errorLog
              else
                  ctx.Logger.LogInformation 
                  <| sprintf "[Shader COMPILED] ShaderPath '%s'" srcFilePath 
                  GlShaderCompiled

          { GlShaderHandle = shaderHandle
          ; Type = shaderType
          ; SourceFilePath = srcFilePath
          ; GlStatus = glStatus
          ;}

    let attach program (shader, ctx) =
        if shader.GlStatus <> GlShaderCompiled then
            invalidOp "The shader must be in GlShaderCompiled status to attach it to a program."  
        else
            ctx.Gl.AttachShader (program.GlProgramHandle, shader.GlShaderHandle)
            let updatedShader = { shader with GlStatus = GlShaderAttached; }
            (updatedShader, ctx)

    let detach program (shader, ctx) =
        if shader.GlStatus <> GlShaderAttached then
            invalidOp "Can't detach an already dettached shader."
        else
            ctx.Gl.DetachShader (program.GlProgramHandle, shader.GlShaderHandle)
            let updatedShader = { shader with GlStatus = GlShaderCompiled; }
            (updatedShader, ctx)
        
    let unload (shader, ctx) =
        ctx.Gl.DeleteShader shader.GlShaderHandle
        let updatedShader = { shader with GlStatus = GlShaderDeleted; }
        (updatedShader, ctx)