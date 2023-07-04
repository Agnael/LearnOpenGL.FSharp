#version 330 core
in VS_OUT {
   vec3 FragPos;
   vec2 TexCoords;
   vec3 TangentLightPos;
   vec3 TangentViewPos;
   vec3 TangentFragPos;
} fs_in;

uniform sampler2D uTexture;
uniform sampler2D uNormalMap;
uniform sampler2D uDisplacementMap;

uniform float uHeightScale;

out vec4 FragColor;

vec2 GetDisplacedTexCoords(vec2 texCoords, vec3 viewDir) {
   const float minLayers = 8;
   const float maxLayers = 32;

   float numLayers = mix(maxLayers, minLayers, abs(dot(vec3(0.0, 0.0, 1.0), viewDir)));

   // Calculate the size of each layer
   float layerDepth = 1.0 / numLayers;

   // Depth of the current layer
   float currentLayerDepth = 0.0;

   // The amount to shift the texture coordinates per layer (from vector P)
   vec2 P = viewDir.xy / viewDir.z * uHeightScale;
   vec2 deltaTexCoords = P / numLayers;

   // Get initial values
   vec2 currentTexCoords = texCoords;
   float currentDepthMapValue = texture(uDisplacementMap, currentTexCoords).r;

   while (currentLayerDepth < currentDepthMapValue) {
      // Shift texture coordinates along direction of P
      currentTexCoords -= deltaTexCoords;

      // Get displacement map value at the current texture coordinates
      currentDepthMapValue = texture(uDisplacementMap, currentTexCoords).r;

      // Get the depth of the next layer
      currentLayerDepth += layerDepth;
   }

   // Get texture coordinates before collision (reverse operations)
   vec2 prevTexCoords = currentTexCoords + deltaTexCoords;

   // Get depth after and before collision for linear interpolation
   float afterDepth = currentDepthMapValue - currentLayerDepth;
   float beforeDepth = texture(uDisplacementMap, prevTexCoords).r - currentLayerDepth + layerDepth;

   // Interpolation of texture coordinates
   float weight = afterDepth / (afterDepth - beforeDepth);

   vec2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

   return finalTexCoords;
}

void main()
{
   // Offset texture coordinates with parallax mapping
   vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
   vec2 texCoords = fs_in.TexCoords;

   texCoords = GetDisplacedTexCoords(fs_in.TexCoords, viewDir);
   
   if (texCoords.x > 1.0 || texCoords.x < 0.0 || texCoords.y > 1.0 || texCoords.y < 0.0) {
      discard;
   }

   // Obtain normal from normalmap
   vec3 normal = texture(uNormalMap, texCoords).rgb;
   normal = normalize(normal * 2.0 - 1.0);

   // Get diffuse color
   vec3 color = texture(uTexture, texCoords).rgb;

   // Ambient
   vec3 ambient = 0.1 * color;

   // Diffuse
   vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
   float diff = max(dot(lightDir, normal), 0.0);
   vec3 diffuse = diff * color;

   // Specular
   vec3 reflectDir = reflect(-lightDir, normal);
   vec3 halfwayDir = normalize(lightDir + viewDir);
   float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
   vec3 specular = vec3(0.2) * spec;

   FragColor = vec4(ambient + diffuse + specular, 1.0);
}