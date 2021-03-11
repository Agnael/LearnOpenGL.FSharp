module GlVbo

open Microsoft.FSharp.NativeInterop
open System

#nowarn "9"
open Galante.OpenGL
open Silk.NET.OpenGL

let emptyVboBuilder =
    { GlVboBuilder.AttrNames = []; AttrDefinitions = []; }

let withAttrNames names builder =
    names
    |> List.distinct
    |> List.length
    |> fun distinctLen ->
        if distinctLen <> names.Length then
            invalidArg "AttrNames" "Attr names must be unique"
        else 
            { builder with AttrNames = names }

let withAttrDefinitions attrDefinitions builder =
    { builder with AttrDefinitions = attrDefinitions; }

let build (vao, ctx) builder = 
            
    GlVao.bind (vao, ctx) |> ignore
    
    let reducer i1 i2 = List.concat [i1; i2]
    let data =
        builder.AttrDefinitions
        |> List.reduce reducer
        |> List.reduce reducer
        |> List.toArray
    
    use dataIntPtr = fixed data
    
    // El len de un stride completo es el producto entre la cantidad de attrs distintos 
    // que tenga, y la cantidad de singles que cada uno de esos attrs tenga internamente     
    let dataStrideSize =
        List.head builder.AttrDefinitions
        |> List.map (fun x -> x.Length)
        |> List.sum

    let vbo = 
        { GlVertexBufferObject.GlVboHandle = ctx.Gl.GenBuffer ()
        ; Data = data
        ; DataByteSize = uint32 <| data.Length * sizeof<single>
        ; DataPtr = NativePtr.toVoidPtr dataIntPtr
        ; StrideSize = dataStrideSize
        ; StrideByteSize = uint32 <| dataStrideSize * sizeof<single>
        ; VertexAttributes =
            builder.AttrNames
            |> List.indexed
            |> List.map
                (fun (attrIdx, attrName) ->
                    let attributesStride = List.head builder.AttrDefinitions

                    attributesStride
                    |> fun stride -> stride.[attrIdx]
                    |> fun currAttrArr ->
                        let offsetByteSize =
                            attributesStride
                            |> List.map (fun x -> x.Length)
                            |> List.take attrIdx
                            |> List.sum
                            |> fun offsetSingles -> offsetSingles * sizeof<single>
                                
                        let offsetPtr = (new IntPtr(offsetByteSize)).ToPointer()

                        { GlVboAttribute.AttrIdx = uint32 attrIdx
                        ; Name = attrName
                        ; DataLength = currAttrArr.Length
                        ; StrideByteSize =  uint32 <| dataStrideSize * sizeof<single>
                        ; OffsetInStridePtr = offsetPtr
                        ;}
                )
            |> List.toArray
        ;}

    ctx.Gl.BindBuffer (BufferTargetARB.ArrayBuffer, vbo.GlVboHandle)

    let dataVoidPtr = NativePtr.toVoidPtr dataIntPtr
    let dataBytesSize = unativeint <| data.Length * sizeof<single>

    ctx.Gl.BufferData 
        ( BufferTargetARB.ArrayBuffer
        , dataBytesSize
        , dataVoidPtr
        , BufferUsageARB.StaticDraw
        )

    vbo.VertexAttributes
    |> Array.map
        (fun attr ->
            ctx.Gl.VertexAttribPointer 
                ( attr.AttrIdx
                , attr.DataLength
                , GLEnum.Float
                , false
                , attr.StrideByteSize
                , attr.OffsetInStridePtr
                )
            ctx.Gl.EnableVertexAttribArray attr.AttrIdx
        )
    |> ignore
    vbo