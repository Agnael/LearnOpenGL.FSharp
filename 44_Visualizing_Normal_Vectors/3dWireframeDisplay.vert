#version 330 core
layout (location = 0)
in vec3 aPos;

layout (location = 1)
in vec3 aNormal;

// Texture coordinates are actually useless here but my already existing model
// loader loads it so i´ll just let it be.
layout (location = 2)
in vec2 aTexCoords;

layout (shared)
uniform Matrices {
   uniform mat4 uProjection;
   uniform mat4 uView;
};

uniform mat4 uModel;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
}
