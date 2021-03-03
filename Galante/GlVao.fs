module GlVao

open Galante

let create ctx = 
    ctx.Gl.GenVertexArray ()
    |> fun handle -> { GlVertexArrayObject.GlVaoHandle = handle }
    |> fun vao -> (vao, ctx)

let bind (vao, ctx) = 
    ctx.Gl.BindVertexArray vao.GlVaoHandle
    (vao, ctx)