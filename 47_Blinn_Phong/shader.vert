#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

layout (shared)
uniform Matrices {
   uniform mat4 uProjection;
   uniform mat4 uView;
};

uniform mat4 uModel;

out vec3 FragPos;
out vec2 TexCoords;
out vec3 Normal;

void main()
{
    TexCoords = aTexCoords;
    FragPos = aPos;
    Normal = aNormal;

    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
}