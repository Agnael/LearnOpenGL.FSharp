#version 330 core

in VS_OUT {
   vec3 FragPos;
   vec2 TexCoords;
   vec3 Normal;
   vec3 TangentLightPos;
   vec3 TangentViewPos;
   vec3 TangentFragPos;
} fs_in;

out vec4 FragColor;

uniform vec3 uLightPosition;
uniform vec3 uViewerPos;

uniform samplerCube uShadowMap;
uniform sampler2D uTexture;
uniform sampler2D uNormalMap;

uniform float uFarPlane;

uniform bool uUseNormalMapping;
uniform bool uUseReverseNormals;

// Array of offset direction for sampling
vec3 gridSamplingDisk[20] = vec3[]
(
   vec3(1, 1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1, 1,  1), 
   vec3(1, 1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1, 1, -1),
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0),
   vec3(1, 0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1, 0, -1),
   vec3(0, 1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0, 1, -1)
);

float ShadowCalculation(vec3 fragPos) {
   vec3 fragToLight = fragPos - uLightPosition;
   float closestDepth = texture(uShadowMap, fragToLight).r;
   float currentDepth = length(fragToLight);

   float shadow  = 0.0;
   float bias    = 0.07; 
   float samples = 4.0;
   float offset  = 0.1;
   for(float x = -offset; x < offset; x += offset / (samples * 0.5))
   {
       for(float y = -offset; y < offset; y += offset / (samples * 0.5))
       {
           for(float z = -offset; z < offset; z += offset / (samples * 0.5))
           {
               float closestDepth = texture(uShadowMap, fragToLight + vec3(x, y, z)).r; 
               closestDepth *= uFarPlane;   // undo mapping [0;1]
               if(currentDepth - bias > closestDepth)
                   shadow += 1.0;
           }
       }
   }
   shadow /= (samples * samples * samples);

   
   return shadow;
}

vec3 GetNormal() {
   if (!uUseNormalMapping) {
      return normalize(fs_in.Normal);
   }

   // Gets normal from the map in the range [0;1]
   vec3 normal = texture(uNormalMap, fs_in.TexCoords).rgb;

   // Transforms the normal value from [0;1] to the normalized [-1;1] value range
   normal = normalize(normal * 2.0 - 1.0);

   return normal;
}

void main()
{
   vec3 lightPos = 
      uUseNormalMapping
      ? fs_in.TangentLightPos
      : uLightPosition;

   vec3 fragPos =
      uUseNormalMapping
      ? fs_in.TangentFragPos
      : fs_in.FragPos;

   vec3 viewerPos =
      uUseNormalMapping
      ? fs_in.TangentViewPos
      : uViewerPos;

   vec3 color = texture(uTexture, fs_in.TexCoords).rgb;
   vec3 normal = GetNormal();
   vec3 lightColor = vec3(0.6);

   // Ambient
   vec3 ambient = 0.3 * color;
   
   // Diffuse
   vec3 lightDir = normalize(lightPos - fragPos);
   float diff = max(dot(lightDir, normal), 0.0);
   vec3 diffuse = diff * lightColor;

   // Specular
   vec3 viewDir = normalize(viewerPos - fragPos);
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = 0.0;
   vec3 halfwayDir = normalize(lightDir + viewDir);
   spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
   vec3 specular = spec * lightColor;

   // Calculates shadow
   float shadow = ShadowCalculation(fs_in.FragPos);

   vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;
   FragColor = vec4(lighting, 1.0);
}