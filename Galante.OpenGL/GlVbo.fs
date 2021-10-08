module GlVbo

#nowarn "9"
open Microsoft.FSharp.NativeInterop
open System
open System.Numerics
open Galante.OpenGL
open Silk.NET.OpenGL
open Gl

let emptyVboBuilder = {
   GlVboBuilder.AttrNames = []
   AttrDefinitions = [||]
}

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
    
   let data =
      builder.AttrDefinitions
      |> Array.concat
      |> Array.concat

   use dataIntPtr = fixed data
    
   // El len de un stride completo es el producto entre la cantidad de attrs 
   // distintos que tenga, y la cantidad de singles que cada uno de esos attrs
   // tenga internamente.
   let dataStrideSize =
      Array.head builder.AttrDefinitions
      |> Array.map (fun x -> x.Length)
      |> Array.sum

   let vbo = {
      GlVertexBufferObject.GlVboHandle = ctx.Gl.GenBuffer ()
      DataByteSize = uint32 <| data.Length * sizeof<single>
      DataPtr = NativePtr.toVoidPtr dataIntPtr
      StrideSize = dataStrideSize
      StrideByteSize = uint32 <| dataStrideSize * sizeof<single>
   }
    
   ctx.Gl.BindBuffer (BufferTargetARB.ArrayBuffer, vbo.GlVboHandle)

   let dataVoidPtr = NativePtr.toVoidPtr dataIntPtr
   let dataBytesSize = unativeint <| data.Length * sizeof<single>

   ctx.Gl.BufferData 
      ( BufferTargetARB.ArrayBuffer
      , dataBytesSize
      , dataVoidPtr
      , BufferUsageARB.StaticDraw
      )

   builder.AttrNames
   |> List.indexed
   |> List.map
      (fun (attrIdx, attrName) ->
         let attributesStride = Array.head builder.AttrDefinitions

         attributesStride
         |> fun stride -> stride.[attrIdx]
         |> fun currAttrArr ->
               let offsetByteSize =
                  attributesStride
                  |> Array.map (fun x -> x.Length)
                  |> Array.take attrIdx
                  |> Array.sum
                  |> fun offsetSingles -> offsetSingles * sizeof<single>
                                
               let offsetPtr = (new IntPtr(offsetByteSize)).ToPointer()

               { 
                  GlVboAttribute.AttrIdx = uint32 attrIdx
                  Name = attrName
                  DataLength = currAttrArr.Length
                  StrideByteSize =  uint32 <| dataStrideSize * sizeof<single>
                  OffsetInStridePtr = offsetPtr
               }
      )
   |> List.toArray
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

let vboCreateInstancedAttributeV2Array idx array ctx =
   glVertexAttribPointer idx ctx