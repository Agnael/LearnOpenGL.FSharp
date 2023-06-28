module Vertex
    open System.Numerics

    [<Struct>]
    type Vertex =
        { Position: Vector3
        ; Normal: Vector3
        ; TextureCoordinates: Vector2
        ; Tangent: Vector3
        ; Bitangent: Vector3
        ;}