#version 330 core
struct Material {
	sampler2D diffuseMap;
	sampler2D specularMap;
	float shininess;
};

struct Light {
	vec3 position;
	vec3 direction;
	float innerCutOffAngleCos;
	float outerCutOffAngleCos;

	// Attenuation components
	float constantComponent;
	float linearComponent;
	float quadraticComponent;

	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;
};

out vec4 FragColor;
  
in vec3 Normal;
in vec3 CurrentFragPos;
in vec2 TextureCoords;

uniform vec3 uViewerPos;
uniform Material uMaterial;
uniform Light uLight;

float getAttenuation(float d, float Kc, float Kl, float Kq)
{	
	return 1.0f / (Kc + Kl * d + Kq * (d*d));
};

void main()
{
	vec3 diffuseMapColor = vec3(texture(uMaterial.diffuseMap, TextureCoords));
	vec3 lightDir = normalize(uLight.position - CurrentFragPos);	

	// Check if lighting is inside the spotlight cone
	float theta = dot(lightDir, normalize(-uLight.direction));

	if (theta > uLight.outerCutOffAngleCos)
	{
		float epsilon = uLight.innerCutOffAngleCos - uLight.outerCutOffAngleCos;
		float intensity = clamp((theta - uLight.outerCutOffAngleCos) / epsilon, 0.0, 1.0);

		vec3 specularMapColor = vec3(texture(uMaterial.specularMap, TextureCoords));
		
		// ATTENUATION
		float distanceToLightSrc = length(uLight.position - CurrentFragPos);
		float attenuation = 
			getAttenuation(
				distanceToLightSrc, 
				uLight.constantComponent, 
				uLight.linearComponent, 
				uLight.quadraticComponent);

		// AMBIENT
		vec3 ambient = uLight.ambientColor * diffuseMapColor;
		
		// DIFFUSE ---------------------------------------------------------------
		vec3 normal = normalize(Normal);
		float diffuseImpact = dot(normal, lightDir);
		float diff = max(diffuseImpact, 0.0);
		vec3 diffuse = uLight.diffuseColor * diff * diffuseMapColor;

		// SPECULAR --------------------------------------------------------------
		vec3 viewDir = normalize(uViewerPos - CurrentFragPos);
		vec3 reflectionDir = reflect(-lightDir, normal);	
		float spec = pow(max(dot(viewDir, reflectionDir), 0.0), uMaterial.shininess);
		vec3 specular = uLight.specularColor * spec * specularMapColor;
		
		diffuse *= intensity;
		specular *= intensity;

		// Remove attenuation from ambient, as otherwise at large distances the light 
		// would be darker inside than outside the spotlight due the ambient term in 
		// the else branch.
//		ambient *= attenuation;
		diffuse *= attenuation;
		specular *= attenuation;

		vec3 result = ambient + diffuse + specular;
		FragColor = vec4(result, 1.0);
	}
	else
	{
		FragColor = vec4(uLight.ambientColor * diffuseMapColor, 1.0);
	}
}  