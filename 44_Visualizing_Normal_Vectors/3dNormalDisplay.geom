#version 330 core
layout (triangles) in;
layout (line_strip, max_vertices = 6) out;

const float MAGNITUDE = 0.4;

// Because the geometry shader acts on a set of vertices as its input, it's
// input data from the vertex shader is always represented as arrays of vertex
// data even though we only have a single vertex right now.
in VS_OUT {
   vec3 Normal;
} gs_in[];

layout (shared)
uniform Matrices {
   uniform mat4 uProjection;
   uniform mat4 uView;
};

void GenerateLine(int index) {
   gl_Position = uProjection * gl_in[index].gl_Position;
   EmitVertex();

   gl_Position =
      uProjection *
      (gl_in[index].gl_Position + vec4(gs_in[index].Normal, 0.0) * MAGNITUDE);
   EmitVertex();

   EndPrimitive();
}

void main()
{    
   GenerateLine(0);
   GenerateLine(1);
   GenerateLine(2);
}