#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D uTexture;

float near = 0.1; 
float far  = 100.0; 
  
float LinearizeDepth(float depth) 
{
    // Back to NDC 
    float z = depth * 2.0 - 1.0; 

    // INVERSE for the NON-linear depth Z value ecuation that follows:
    // Fdepth = (1/z - 1/near) / (1/far - 1/near)
    return (2.0 * near * far) / (far + near - z * (far - near));	
}

void main()
{    
    // Divide by far for demonstration
    float depth = LinearizeDepth(gl_FragCoord.z) / far; 
    FragColor = vec4(vec3(depth), 1.0);
}