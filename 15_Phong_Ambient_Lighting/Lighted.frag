#version 330 core
out vec4 FragColor;
  
uniform vec3 uObjectColor;
uniform vec3 uLightColor;

void main()
{
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * uLightColor;

    vec3 result = ambient * uObjectColor;
    FragColor = vec4(result, 1.0);
}  