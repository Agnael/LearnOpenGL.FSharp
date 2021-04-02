#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTextureCoords;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 Normal;
out vec3 CurrentFragPos;
out vec2 TextureCoords;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);    
    CurrentFragPos = vec3(uModel * vec4(aPos, 1.0));
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    TextureCoords = aTextureCoords;
}