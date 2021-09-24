#version 330 core
out vec4 FragColor;

uniform sampler2D uBackFacingTexture;
uniform sampler2D uFrontFacingTexture;

in vec2 TextureCoords;

void main()
{  
   if (gl_FragCoord.x > 200)
   {
      if (gl_FrontFacing)
      {
         FragColor = texture(uFrontFacingTexture, TextureCoords);
      }
      else
      {
         FragColor = texture(uBackFacingTexture, TextureCoords);
      }
   }
   else
   {
      FragColor = vec4(1.0, 0.0, 0.0, 1.0);
   }
}