#version 330 core
layout (points) in;
layout (triangle_strip, max_vertices = 5) out;

// Because the geometry shader acts on a set of vertices as its input, it's
// input data from the vertex shader is always represented as arrays of vertex
// data even though we only have a single vertex right now.
in VS_OUT {
   vec3 color;
} gs_in[];

// Because the fragment shader expects only a single (interpolated) color it
// doesn't make sense to forward multiple colors. The fColor vector is thus
// not an array, but a single vector. When emitting a vertex, that vertex will
// store the last stored value in fColor as that vertex's output value. For
// the houses, we can fill fColor once with the color from the vertex shader
// before the first vertex is emitted to color the entire house.
out vec3 fColor;

void main()
{    
   // Each emitted vertex will be assigned the current fColor value. So they'll
   // have all the same color until this variable changes, between emitions.
   fColor = gs_in[0].color;

   // Square's bottom left
   gl_Position = gl_in[0].gl_Position + vec4(-0.2, -0.2, 0.0, 0.0);
   EmitVertex();

   // Square's bottom right
   gl_Position = gl_in[0].gl_Position + vec4(0.2, -0.2, 0.0, 0.0);
   EmitVertex();

   // Square's top left
   gl_Position = gl_in[0].gl_Position + vec4(-0.2, 0.2, 0.0, 0.0);
   EmitVertex();

   // Square's top right
   gl_Position = gl_in[0].gl_Position + vec4(0.2, 0.2, 0.0, 0.0);
   EmitVertex();

   // House's top
   gl_Position = gl_in[0].gl_Position + vec4(0.0, 0.4, 0.0, 0.0);
   fColor = vec3(1.0, 1.0, 1.0);
   EmitVertex();

   EndPrimitive();
}