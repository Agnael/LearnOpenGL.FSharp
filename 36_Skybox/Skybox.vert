#version 330 core
layout (location = 0) in vec3 aPos;

// Vec3 instead of Vec2!!! this is a 3d cubebox!!
out vec3 TexCoords;

// Note that there is no need for a model matrix, since the skybox won't move
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    TexCoords = aPos;

    mat4 trimmedViewMatrix = mat4(mat3(uView));
    gl_Position = uProjection * trimmedViewMatrix * vec4(aPos, 1.0);
}