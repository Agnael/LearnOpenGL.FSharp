#version 330 core
layout (triangles) in;
layout (triangle_strip, max_vertices = 18) out;

uniform mat4 uLightSpaceMatrix_right;
uniform mat4 uLightSpaceMatrix_left;
uniform mat4 uLightSpaceMatrix_top;
uniform mat4 uLightSpaceMatrix_bottom;
uniform mat4 uLightSpaceMatrix_near;
uniform mat4 uLightSpaceMatrix_far;

// FragPos from GS (output per emitvertex)
out vec4 FragPos;

void RenderFace(int face, mat4 faceLightMatrix) {
   gl_Layer = face;

   for (int i = 0; i < 3; ++i) {
      FragPos = gl_in[i].gl_Position;
      gl_Position = faceLightMatrix * FragPos;
      EmitVertex();
   }

   EndPrimitive();
}

void main()
{    
   RenderFace(0, uLightSpaceMatrix_right);
   RenderFace(1, uLightSpaceMatrix_left);
   RenderFace(2, uLightSpaceMatrix_top);
   RenderFace(3, uLightSpaceMatrix_bottom);
   RenderFace(4, uLightSpaceMatrix_near);
   RenderFace(5, uLightSpaceMatrix_far);
}