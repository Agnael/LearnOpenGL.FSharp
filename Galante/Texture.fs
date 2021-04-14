module Texture
    open Galante.OpenGL

    type Texture =
        | DiffuseMap of texturePath: string
        | SpecularMap of texturePath: string
        | NormalMap of texturePath: string
        | EmissionMap of texturePath: string