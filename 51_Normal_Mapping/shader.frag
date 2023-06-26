﻿#version 330 core

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
   closestDepth *= uFarPlane;
   float currentDepth = length(fragToLight);
   float bias = 0.05;

   float shadow =
      currentDepth - bias > closestDepth 
      ? 1.0 
      : 0.0;

   return shadow;

//   // Get vector between fragment position and light position
//   vec3 fragToLight = fragPos - uLightPosition;
//
//   // Gets current linear depth as the length between the fragment and light position
//   float currentDepth = length(fragToLight);
//
//   float shadow = 0.0;
//   float bias = 0.15;
//   int samples = 20;
//   float viewDistance = length(uViewerPos - fragPos);
//   float diskRadius = (1.0 + (viewDistance / uFarPlane)) / 25.0;
//
//   for (int i = 0; i < samples; ++i) {
//      float closestDepth = texture(uShadowMap, fragToLight + gridSamplingDisk[i] * diskRadius).r;
//      closestDepth *= uFarPlane; // Undoes mapping [0;1]
//
//      if (currentDepth - bias > closestDepth) {
//         shadow += 1.0;
//      }
//   }
//
//   return shadow;
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
   if (uUseNormalMapping) {   
      vec3 color = texture(uTexture, fs_in.TexCoords).rgb;
      vec3 normal = GetNormal();
      vec3 lightColor = vec3(0.3);

      // Ambient
      vec3 ambient = 0.3 * color;

      // Diffuse
      vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
      float diff = max(dot(lightDir, normal), 0.0);
      vec3 diffuse = diff * lightColor;

      // Specular
      vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
      vec3 reflectDir = reflect(-lightDir, normal);
      float spec = 0.0;
      vec3 halfwayDir = normalize(lightDir + viewDir);
      spec = pow(max(dot(normal, halfwayDir), 0.0), 128.0);
      vec3 specular = spec * lightColor;

      // Calculates shadow
      float shadow = ShadowCalculation(fs_in.FragPos);

      // Calculates final light color
      vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;
      FragColor = vec4(lighting, 1.0);
   }
   else {
      vec3 color = texture(uTexture, fs_in.TexCoords).rgb;
      vec3 normal = GetNormal();
      vec3 lightColor = vec3(0.3);

      // Ambient
      vec3 ambient = 0.3 * color;

      // Diffuse
      vec3 lightDir = normalize(uLightPosition - fs_in.FragPos);
      float diff = max(dot(lightDir, normal), 0.0);
      vec3 diffuse = diff * lightColor;

      // Specular
      vec3 viewDir = normalize(uViewerPos - fs_in.FragPos);
      vec3 reflectDir = reflect(-lightDir, normal);
      float spec = 0.0;
      vec3 halfwayDir = normalize(lightDir + viewDir);
      spec = pow(max(dot(normal, halfwayDir), 0.0), 128.0);
      vec3 specular = spec * lightColor;

      // Calculates shadow
      float shadow = ShadowCalculation(fs_in.FragPos);

      // Calculates final light color
      vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;
      FragColor = vec4(lighting, 1.0);
   }
}