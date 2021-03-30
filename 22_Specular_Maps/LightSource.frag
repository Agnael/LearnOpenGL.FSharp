#version 330 core
out vec4 FragColor;
 
uniform vec3 uEmittedLightColor;

void main()
{    
    FragColor = vec4(uEmittedLightColor, 1.0);
}