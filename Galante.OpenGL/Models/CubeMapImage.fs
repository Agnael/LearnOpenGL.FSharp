namespace Galante.OpenGL

open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

type CubeMapImage = {
   Back: Image<Rgba32>
   Bottom: Image<Rgba32>
   Front: Image<Rgba32>
   Left: Image<Rgba32>
   Right: Image<Rgba32>
   Top: Image<Rgba32>
}