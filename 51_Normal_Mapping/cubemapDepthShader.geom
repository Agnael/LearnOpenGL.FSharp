#version 330 core
layout (triangles) in;
layout (triangle_strip, max_vertices = 18) out;

uniform mat4 uLightSpaceMatrices[6];

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
   for (int face = 0; face < 6; ++face) {
      gl_Layer = face;

      for (int i = 0; i < 3; ++i) {
         FragPos = gl_in[i].gl_Position;
         gl_Position = uLightSpaceMatrices[face] * FragPos;
         EmitVertex();
      }
      EndPrimitive();
   }
}