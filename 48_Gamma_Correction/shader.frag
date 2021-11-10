#version 330 core

in vec3 FragPos;
in vec2 TexCoords;
in vec3 Normal;

out vec4 FragColor;

uniform sampler2D uTexture;

uniform vec3[4] uLightPositions;
uniform vec3[4] uLightColors;

uniform vec3 uViewerPos;
uniform float uGamma;

vec3 BlinnPhong(vec3 normal, vec3 fragPos, vec3 lightPos, vec3 lightColor)
{
   // diffuse
   vec3 lightDir = normalize(lightPos - fragPos);
   float diff = max(dot(lightDir, normal), 0.0);
   vec3 diffuse = diff * lightColor;
   // specular
   vec3 viewDir = normalize(uViewerPos - FragPos);
   vec3 reflectDir = reflect(-lightDir, normal);
   float spec = 0.0;
   vec3 halfwayDir = normalize(lightDir + viewDir);  
   spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
   vec3 specular = spec * lightColor;    
   // simple attenuation
   float max_distance = 1.5;
   float distance = length(lightPos - fragPos);
   float attenuation = 1.0 / (uGamma != 1.0 ? distance * distance : distance);
    
   diffuse *= attenuation;
   specular *= attenuation;
    
   return diffuse + specular;
}

void main()
{
   vec3 color = texture(uTexture, TexCoords).rgb;
   vec3 lighting = vec3(0.0);

   for(int i = 0; i < 4; ++i) {
      vec3 lightContribution = 
         BlinnPhong(
            normalize(Normal), 
            FragPos, 
            uLightPositions[i], 
            uLightColors[i]
         );

      lighting += lightContribution;         
   }

   color *= lighting;

   if(uGamma != 1.0)
      color = pow(color, vec3(1.0 / uGamma));

   FragColor = vec4(color, 1.0);
}