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

out VS_OUT {
   vec3 Normal;
} vs_out;

uniform mat4 uModel;

void main()
{
   // The projection matrix is not being used this time!!
    gl_Position = uView * uModel * vec4(aPos, 1.0);

    // This time we're creating a geometry shader that uses the vertex normals
    // supplied by the model instead of generating it ourself. To accommodate 
    // for scaling and rotations (due to the view and model matrix) we'll 
    // transform the normals with a normal matrix. The geometry shader 
    // receives its position vectors as view-space coordinates so we should 
    // also transform the normal vectors to the same space. 
    mat3 normalMatrix = mat3(transpose(inverse(uView * uModel)));
    vs_out.Normal = normalize(vec3(vec4(normalMatrix * aNormal, 0.0)));
}
