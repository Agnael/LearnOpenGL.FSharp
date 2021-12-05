#version 330 core
in vec3 FragPos;
in vec2 TexCoords;

uniform sampler2D uShadowMap;
uniform float uNearPlane;
uniform float uFarPlane;

out vec4 FragColor;

// Required when using a perspective projection matrix
float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // Back to NDC 

    return 
       (2.0 * uNearPlane * uFarPlane) / 
       (uFarPlane + uNearPlane - z * (uFarPlane - uNearPlane));
}

void main()
{             
    float depthValue = texture(uShadowMap, TexCoords).r;

    // Perspective
//    FragColor = vec4(vec3(LinearizeDepth(depthValue) / uFarPlane), 1.0); 

    // Orthographic
    FragColor = vec4(vec3(depthValue), 1.0);
}