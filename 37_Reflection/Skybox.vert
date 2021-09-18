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

    /*
      Tricks the depth buffer into believing that the skybox has the maximum 
      depth value of 1.0 so that it fails the depth test wherever there's a 
      different object in front of it.

      In the coordinate systems chapter we said that perspective division is 
      performed after the vertex shader has run, dividing the gl_Position's 
      xyz coordinates by its w component. We also know from the depth testing 
      chapter that the z component of the resulting division is equal to that 
      vertex's depth value. Using this information we can set the z component 
      of the output position equal to its w component which will result in a z 
      component that is always equal to 1.0, because when the perspective 
      division is applied its z component translates to w / w = 1.0
    */
    vec4 position = uProjection * trimmedViewMatrix * vec4(aPos, 1.0);
    gl_Position = position.xyww;
}