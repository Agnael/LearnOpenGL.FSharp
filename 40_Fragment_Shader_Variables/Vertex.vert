#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTextureCoords;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 TextureCoords;

void main()
{
   TextureCoords = aTextureCoords;
   gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
}