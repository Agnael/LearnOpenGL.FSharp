#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D uTexture;

void main()
{    
	vec4 diffMapColor = texture(uTexture, TexCoords);

	// This example is the first one to actually use the alpha value of the
	// diffuse map, that's why this time it's a vec4 type instead of a vec3.
	// If the alpha value is too low, the fragment can totally be discarded,
	// since this exercise focuses on full transparencies
	if (diffMapColor.a < 0.1)
		discard;

    FragColor = diffMapColor;
}