#version 330 core
in vec4 FragPos;

uniform vec3 uLightPosition;
uniform float uFarPlane;

void main()
{
   float lightDistance = length(FragPos.xyz - uLightPosition);

   // Map to [0;1] ramge by dividing by uFarPlane
   lightDistance = lightDistance / uFarPlane;

   // Write this as modified depth
   gl_FragDepth = lightDistance;
}