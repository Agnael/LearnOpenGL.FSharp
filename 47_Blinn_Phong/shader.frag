#version 330 core

in vec3 FragPos;
in vec2 TexCoords;
in vec3 Normal;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec3 uLightPos;
uniform vec3 uViewerPos;
uniform bool uUseBlinn;

void main()
{
    vec3 color = texture(uTexture, TexCoords).rgb;

    // ambient
    vec3 ambient = 0.05 * color;

    // diffuse
    vec3 lightDir = normalize(uLightPos - FragPos);
    vec3 normal = normalize(Normal);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;

    // specular
    vec3 viewDir = normalize(uViewerPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = 0.0;

    if(uUseBlinn)
    {
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    }
    else
    {
        vec3 reflectDir = reflect(-lightDir, normal);
        spec = pow(max(dot(viewDir, reflectDir), 0.0), 8.0);
    }

    vec3 specular = vec3(0.3) * spec; // assuming bright white light color
    FragColor = vec4(ambient + diffuse + specular, 1.0);
}