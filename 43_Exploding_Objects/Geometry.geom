#version 330 core
layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

const float explosionMagnitude = 2.0;

// Because the geometry shader acts on a set of vertices as its input, it's
// input data from the vertex shader is always represented as arrays of vertex
// data even though we only have a single vertex right now.
in VS_OUT {
   vec2 TexCoords;
} gs_in[];

out GS_OUT {
   vec2 TexCoords;
} gs_out;

uniform float uTime;

vec3 GetNormal() {
   vec3 a = vec3(gl_in[0].gl_Position) - vec3(gl_in[1].gl_Position);
   vec3 b = vec3(gl_in[2].gl_Position) - vec3(gl_in[1].gl_Position);

   return normalize(cross(a, b));
}

vec4 Explode(vec4 vertexPosition, vec3 primitiveNormal) {
   vec3 direction =
      primitiveNormal *
      ((sin(uTime) + 1.0) / 2.0) *
      explosionMagnitude;

   return vertexPosition + vec4(direction, 0.0);
}

void main()
{    
   vec3 normal = GetNormal();

   gl_Position = Explode(gl_in[0].gl_Position, normal);
   gs_out.TexCoords = gs_in[0].TexCoords;
   EmitVertex();

   gl_Position = Explode(gl_in[1].gl_Position, normal);
   gs_out.TexCoords = gs_in[1].TexCoords;
   EmitVertex();

   gl_Position = Explode(gl_in[2].gl_Position, normal);
   gs_out.TexCoords = gs_in[2].TexCoords;
   EmitVertex();

   EndPrimitive();
}