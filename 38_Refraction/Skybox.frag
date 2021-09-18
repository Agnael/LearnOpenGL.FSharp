#version 330 core
out vec4 FragColor;

in vec3 TexCoords;

// Note that this is no longer a sampler2D, but a samplerCube
uniform samplerCube uSkybox;

void main()
{    
    FragColor = texture(uSkybox, TexCoords);
}