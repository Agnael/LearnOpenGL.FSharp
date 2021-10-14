#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in mat4 aInstancedModel;

out VS_OUT {
   vec2 TexCoords;
} vs_out;

layout (shared)
uniform Matrices {
   uniform mat4 uProjection;
   uniform mat4 uView;
};

void main()
{
    gl_Position = uProjection * uView * aInstancedModel * vec4(aPos, 1.0);
    vs_out.TexCoords = aTexCoords;
}