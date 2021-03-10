#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    // ClipVector = projectionMatrix * viewMatrix * modelMatrix * localVector
    // NOTE: we read the multiplication from right to left
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);

    TexCoord = aTexCoord;
}