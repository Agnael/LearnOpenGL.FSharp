#version 330 core

in vec3 FragPos;
in vec2 TexCoords;
in vec3 Normal;
in vec4 FragPosLightSpace;

out vec4 FragColor;

uniform float uGamma;
uniform vec3 uLightPosition;
uniform vec3 uViewerPos;

uniform sampler2D uTexture;
uniform sampler2D uShadowMap;

float GetShadow(vec4 fragPosLightSpace, vec3 lightDir, vec3 normalizedNormal)
{
   vec2 texelSize = 1.0 / textureSize(uShadowMap, 0);

   // The first thing to do to check whether a fragment is in shadow, is 
   // transform the light-space fragment position in clip-space to normalized 
   // device coordinates. When we output a clip-space vertex position to 
   // gl_Position in the vertex shader, OpenGL automatically does a 
   // perspective divide e.g. transform clip-space coordinates in the range 
   // [-w,w] to [-1,1] by dividing the x, y and z component by the vector's 
   // w component. As the clip-space FragPosLightSpace is not passed to the fragment shader through gl_Position, we have to do this perspective 
   // divide ourselves. This returns the fragment's light-space position in
   // the range [-1,1].
   // NOTE: When using an orthographic projection matrix the w component of a
   // vertex remains untouched so this step is actually quite meaningless.
   // However, it is necessary when using perspective projection so keeping
   // this line ensures it works with both projection matrices.
   vec3 projCoords = vec3(0.0);
   projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

   // Because the depth from the depth map is in the range [0,1] and we also
   // want to use projCoords to sample from the depth map, we transform the
   // NDC coordinates to the range [0,1]:
   projCoords = projCoords * 0.5 + 0.5;

   // With these projected coordinates we can sample the depth map as the
   // resulting [0,1] coordinates from projCoords directly correspond to the
   // transformed NDC coordinates from the first render pass. This gives us
   // the closest depth from the light's point of view.
   float closestDepth = texture(uShadowMap, projCoords.xy).r;

   // To get the current depth at this fragment we simply retrieve the
   // projected vector's z coordinate which equals the depth of this fragment
   // from the light's perspective.
   float currentDepth = projCoords.z;

   // A shadow bias of 0.005 solves the issues of our scene by a large extent,
   // but you can imagine the bias value is highly dependent on the angle
   // between the light source and the surface. If the surface would have a
   // steep angle to the light source, the shadows may still display shadow
   // acne.
   // A more solid approach would be to change the amount of bias based on the
   // surface angle towards the light: something we can solve with the dot 
   // product.
   // Slope scaled depth bias (https://gamedev.stackexchange.com/a/66999)
   float minDepthBias = 0.0025;
   float lightDotNormal = dot(normalizedNormal, lightDir);
   float bias = 0.002 * sqrt(1 - pow(lightDotNormal, 2)) / lightDotNormal;
   bias = max(bias, minDepthBias);

   // PCF
   float shadow = 0.0;
   for(int x = -1; x <= 1; ++x)
   {
      for(int y = -1; y <= 1; ++y)
      {
         vec2 currShadowMapTexelPos = projCoords.xy + vec2(x, y) * texelSize;
         float pcfDepth = texture(uShadowMap, currShadowMapTexelPos).r; 
         
         // The actual comparison is then simply a check whether currentDepth
         // is higher than closestDepth and if so, the fragment is in shadow.
         float currShadowValue = currentDepth - bias > pcfDepth  ? 1.0 : 0.0;
         shadow += currShadowValue;
      }    
   }
   shadow /= 9.0;

   // Over-sampling
   // The shadow map's texture was configured to have a clamp-to-border wrap
   // mode for both coordinates, and a border color of Vec4(1, 1, 1, 1) 
   // was set. Therefore, we can check for tha current value to be higher than
   // 1.0 and, in that case, assume it's actually outside of the shadowmap and
   // hence not apply any shadow to the fragment.
   if (projCoords.z > 1.0)
      shadow = 0.0;

   return shadow;
}

void main()
{   
   vec3 color = texture(uTexture, TexCoords).rgb;

   vec3 normal = normalize(Normal);
   vec3 lightColor = vec3(1.0);

   // Ambient
   vec3 ambient = 0.15 * lightColor;

   // Diffuse
   vec3 lightDir = normalize(uLightPosition - FragPos);
   float diff = max(dot(lightDir, normal), 0.0);
   vec3 diffuse = diff * lightColor;

   // Specular
   vec3 viewDir = normalize(uViewerPos - FragPos);
   float spec = 0.0;
   vec3 halfwayDir = normalize(lightDir + viewDir);  
   spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
   vec3 specular = spec * lightColor;    

   // Calculates shadow
   float shadow = GetShadow(FragPosLightSpace, lightDir, normal);

   // No shadow if this is the back face of a model (relative to the 
   // directional light source)
   shadow = dot(uLightPosition, normal) <= 0.0 ? 0.0 : shadow;

   vec3 result = (ambient + (1.0 - shadow) * (diffuse + specular)) * color; 

   if(uGamma != 1.0)
      result = pow(result, vec3(1.0 / uGamma));

   FragColor = vec4(result, 1.0);
}