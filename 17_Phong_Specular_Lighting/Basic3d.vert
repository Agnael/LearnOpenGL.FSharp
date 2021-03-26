#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 Normal;
out vec3 CurrentFragPos;

void main()
{
    // [ClipVector = projectionMatrix * viewMatrix * modelMatrix * localVector]
    // NOTE: we read the multiplication from right to left.
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
    
	/*************************************************************************
      We're going to do all the lighting calculations in world space so we 
      want a vertex position that is in world space first. We can accomplish 
      this by multiplying the vertex position attribute with the model matrix 
      only (not the view and projection matrix) to transform it to world
      space coordinates.
    *************************************************************************/
    CurrentFragPos = vec3(uModel * vec4(aPos, 1.0));
    
	/*************************************************************************
        Shouldn't we transform the normal vectors to world space coordinates 
     as well? Basically yes, but it's not as simple as simply multiplying it 
     with a model matrix.
        First of all, normal vectors are only direction vectors and do not 
     represent a specific position in space. Also, normal vectors do not have a 
     homogeneous coordinate (the w component of a vertex position). This means 
     that translations should not have any effect on the normal vectors. So if 
     we want to multiply the normal vectors with a model matrix we want to 
     remove the translation part of the matrix by taking the upper-left 3x3 
     matrix of the model matrix (note that we could also set the w component of 
     a normal vector to 0 and multiply with the 4x4 matrix).
        Second, if the model matrix performs a non-uniform scale, the vertices 
     would be changed in such a way that the normal vector is not perpendicular 
     to the surface anymore.
        The trick of fixing this behavior is to use a different model matrix 
     specifically tailored for normal vectors. This matrix is called the normal 
     matrix and uses a few linear algebraic operations to remove the effect of 
     wrongly scaling the normal vectors. If you want to know how this matrix is 
     calculated I suggest the following article (https://bit.ly/3vY1AXn).
        The normal matrix is defined as the transpose of the inverse of the 
     upper-left 3x3 part of the model matrix. Note that most resources define 
     the normal matrix as derived from the model-view matrix, but since we're 
     working in world space (and not in view space) we will derive it from the 
     model matrix
        In the vertex shader we can generate the normal matrix by using the 
     inverse and transpose functions in the vertex shader that work on any 
     matrix type. Note that we cast the matrix to a 3x3 matrix to ensure it 
     loses its translation properties and that it can multiply with the vec3 
     normal vector:
    __________________________________________________________________________
    | ATTENTION:                                                              |
    | Inversing matrices is a costly operation for shaders, so wherever       |
    | possible try to avoid doing inverse operations since they have to be    |
    | done on each vertex of your scene. For learning purposes this is fine,  |
    | but for an efficient application you'll likely want to calculate the    |
    | normal matrix on the CPU and send it to the shaders via a uniform       |
    | before drawing (just like the model matrix).                            |
    |_________________________________________________________________________|
    *************************************************************************/    
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
}