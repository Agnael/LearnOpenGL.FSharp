#version 330 core
out vec4 FragColor;
  
uniform vec3 uObjectColor;
uniform vec3 uLightColor;

void main()
{    
    FragColor = vec4(uLightColor * uObjectColor, 1.0);
}