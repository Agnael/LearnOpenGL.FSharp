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
uniform bool uUseReverseNormals;
uniform bool uUseNormalMapping;

uniform vec3 uLightPosition;
uniform vec3 uViewerPos;

out VS_OUT {
   vec3 FragPos;
   vec2 TexCoords;

   // The point of this chapter is to NOT use this anymore but I keep it
   // so that I can have the toggle on/off feature for normal mapping
   vec3 Normal;

   // This are the variables for normal mapping
   vec3 TangentLightPos;
   vec3 TangentViewPos;
   vec3 TangentFragPos;
} vs_out;

void main()
{
   vs_out.FragPos = vec3(uModel * vec4(aPos, 1.0));
   vs_out.TexCoords = aTexCoords;

   mat3 normalMatrix = transpose(inverse(mat3(uModel)));

   if (uUseNormalMapping) {
      vec3 normal =
         uUseReverseNormals
         ? normalize(-1.0 * aNormal)
         : normalize(aNormal);

      vec3 T = normalize(vec3(uModel * vec4(aTangent, 0.0)));      
      vec3 N = normalize(vec3(uModel * vec4(normal, 0.0))); 
      
      // Re-ortogonize T with respect to N
      T = normalize(T - dot(T, N) * N);

      vec3 B = cross(N, T);

      mat3 TBN = transpose(mat3(T, B, N));

      vs_out.TangentLightPos = TBN * uLightPosition;
      vs_out.TangentViewPos = TBN * uViewerPos;
      vs_out.TangentFragPos = TBN * vs_out.FragPos;
   }
   else {
      vec3 normal =
         uUseReverseNormals
         ? normalize(normalMatrix * (-1.0 * aNormal))
         : normalize(normalMatrix * aNormal);

      vs_out.Normal = normal;
   }
      
   gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
}