#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

layout (shared)
uniform Matrices {
   uniform mat4 uProjection;
   uniform mat4 uView;
};

uniform mat4 uModel;

uniform vec3 uLightPosition;
uniform vec3 uViewerPos;

out VS_OUT {
   vec3 FragPos;
   vec2 TexCoords;
   vec3 TangentLightPos;
   vec3 TangentViewPos;
   vec3 TangentFragPos;
} vs_out;

void main()
{
   vs_out.FragPos = vec3(uModel * vec4(aPos, 1.0));
   vs_out.TexCoords = aTexCoords;

   mat3 modelMatrix = mat3(uModel);

   vec3 T = normalize(vec3(uModel * vec4(aTangent, 0.0)));
   vec3 N = normalize(vec3(uModel * vec4(normalize(aNormal), 0.0))); 
   
   // Re-ortogonize T with respect to N
   T = normalize(T - dot(T, N) * N);

   vec3 B = cross(N, T);

   mat3 TBN = transpose(mat3(T, B, N));

   vs_out.TangentLightPos = TBN * uLightPosition;
   vs_out.TangentViewPos = TBN * uViewerPos;
   vs_out.TangentFragPos = TBN * vs_out.FragPos;
      
   gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
}