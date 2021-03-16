#version 330 core
out vec4 FragColor;
  
in vec2 TexCoord;

uniform sampler2D uTex1;
uniform sampler2D uTex2;

void main()
{    
    FragColor = mix(texture(uTex1, TexCoord), texture(uTex2, TexCoord), 0.2);
}