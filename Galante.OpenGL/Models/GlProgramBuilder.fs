namespace Galante.OpenGL

open Silk.NET.OpenGL

// Implementación, junto con los métodos que lo complementan 
// (que están en GL) robados de: https://stackoverflow.com/a/2267467/6466245
type GlProgramBuilder<'t, 'n, 's, 'u, 'ub> =
    internal {
      Name: 'n
      ShaderDefinitions: 's
      UniformNames: 'u
      UniformBlocks: 'ub
    }