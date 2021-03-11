[<RequireQualifiedAccess>]
module GlProg
    #nowarn "9"
    open Silk.NET.OpenGL
    open Galante.OpenGL
    open System.IO
    open Microsoft.Extensions.Logging
    open System.Numerics
    open Microsoft.FSharp.NativeInterop

    let rec private loadProgramShaders pending added (ctx: GlWindowContext) =
        match pending with
        | [] -> added
        | h::t -> 
            let (shaderType, srcFilePath) = h
            GlShad.load shaderType srcFilePath ctx
            |> fun shader ->
                if shader.GlStatus <> GlShaderCompiled
                then invalidOp "Can't add a non-compiled shader."
                else shader
            |> fun shader ->
                loadProgramShaders t <| shader::added <| ctx
           
    let rec private attachProgramShaders pending attached (program, ctx) =
        match pending with
        | [] -> attached
        | h::t -> 
            let h, _ = GlShad.attach program (h, ctx)
            attachProgramShaders t <| h::attached <| (program, ctx) 

    let rec private getUniforms names uniformRecords (program, ctx) =
        match names with
        | [] -> uniformRecords
        | (h: string)::t -> 
            let newUniformRecord =
                ctx.Gl.GetUniformLocation (program.GlProgramHandle, h)
                |> fun uniformHandle -> 
                    { GlProgramUniform.UniformName = h
                    ; GlUniformHandle = uniformHandle
                    ;}
            getUniforms t <| newUniformRecord::uniformRecords <| (program, ctx)

    let rec private unloadProgramShaders pending deleted (program, ctx) =
        match pending with
        | [] -> deleted
        | h::t ->
            let h, _ = (h, ctx) |> GlShad.detach program |> GlShad.unload
            unloadProgramShaders t <| h::deleted <| (program, ctx)

    let setAsCurrent (program, ctx) =
           ctx.Gl.UseProgram program.GlProgramHandle
           (program, ctx)
           
    let setUniformF name (value: single) (program, ctx) =    
        program.Uniforms
        |> List.find (fun x -> x.UniformName = name)
        |> fun uniform -> ctx.Gl.Uniform1 (uniform.GlUniformHandle, value)
        (program, ctx)
        
    let setUniformI name (value: int) (program, ctx) =    
        program.Uniforms
        |> List.find (fun x -> x.UniformName = name)
        |> fun uniform -> ctx.Gl.Uniform1(uniform.GlUniformHandle, value)
        (program, ctx)
        
    let setUniformM4x4 name (value: Matrix4x4) (program, ctx) =         
        // This block is the equivalent of the C# code:
        // "_gl.UniformMatrix4(location, 1, false, (float*) &value)"
        // where value is of type System.Numerics.Matrix4x4.
        let mutable auxVal = value
        let valPtr = NativePtr.toNativeInt<Matrix4x4> &&auxVal
        let valFloatPtr: nativeptr<float32> = NativePtr.ofNativeInt<float32> valPtr 

        program.Uniforms
        |> List.find (fun x -> x.UniformName = name)
        |> fun uniform -> ctx.Gl.UniformMatrix4(uniform.GlUniformHandle, 1ul, false, valFloatPtr)
        (program, ctx)

    let setUniformV4 name (x: single, y, z, w) (program: GlProgram, ctx) =             
        program.Uniforms
        |> List.find (fun x -> x.UniformName = name)
        |> fun uniform -> ctx.Gl.Uniform4 (uniform.GlUniformHandle, x, y, z, w)
        (program, ctx)
       
    let emptyBuilder =
        { Name = (); ShaderDefinitions = (); UniformNames = (); }

    let withShaders (s: (ShaderType * string) list) (b: GlProgramBuilder<'t,_,unit,_>) : GlProgramBuilder<'t,_,_,_> =
        { Name = b.Name
        ; ShaderDefinitions = s
        ; UniformNames = b.UniformNames 
        ;}

    let withName (n: string) (b: GlProgramBuilder<'t,unit,_,_>) : GlProgramBuilder<'t,_,_,_> =
        { Name = n
        ; ShaderDefinitions = b.ShaderDefinitions
        ; UniformNames = b.UniformNames 
        ;}

    let withUniforms (u: string list) (b: GlProgramBuilder<'t,_,_,unit>) : GlProgramBuilder<'t,_,_,_> =
        { Name = b.Name
        ; ShaderDefinitions = b.ShaderDefinitions
        ; UniformNames = u
        ;}

    let build ctx (b: GlProgramBuilder<'t, _, _, _>) =
        let program =
            { GlProgramHandle = ctx.Gl.CreateProgram ()
            ; Name = b.Name
            ; Shaders = loadProgramShaders b.ShaderDefinitions [] ctx
            ; LinkingStatus = GlProgramLinkingPending
            ; Uniforms = []
            ;}
        
        let program =
            { program with
                Shaders = attachProgramShaders program.Shaders [] (program, ctx); }
        
        ctx.Gl.LinkProgram program.GlProgramHandle
                    
        ctx.Gl.GetProgram (program.GlProgramHandle, ProgramPropertyARB.LinkStatus)
        |> fun statusCode ->
            if statusCode = 0 
            then 
                ctx.Gl.GetProgramInfoLog program.GlProgramHandle
                |> fun linkingLog -> 
                    ctx.Logger.LogCritical
                        <| sprintf
                            "[Gl shader program '%s' LINKING ERROR] Log: %s"
                            program.Name
                            linkingLog 
                    { program with LinkingStatus = GlProgramLinkingError linkingLog; }
            else 
                ctx.Logger.LogCritical <| sprintf "[Gl shader program '%s' LINKED]" program.Name
        
                { program with
                    LinkingStatus = GlProgramLinkingSuccesful;
                    Shaders = unloadProgramShaders program.Shaders [] (program, ctx);
                    Uniforms = getUniforms b.UniformNames [] (program, ctx); }