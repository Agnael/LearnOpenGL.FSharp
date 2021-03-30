#version 330 core
struct Material {
	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;
	float shininess;
};

struct Light {
	vec3 position;

	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;
};

out vec4 FragColor;
  
in vec3 Normal;
in vec3 CurrentFragPos;

uniform vec3 uViewerPos;
uniform Material uMaterial;
uniform Light uLight;

void main()
{
	// AMBIENT
    vec3 ambient = uLight.ambientColor * uMaterial.ambientColor;

	// DIFFUSE ---------------------------------------------------------------
	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(uLight.position - CurrentFragPos);	
	float diffuseImpact = dot(normal, lightDir);
	float diff = max(diffuseImpact, 0.0);
	vec3 diffuse = uLight.diffuseColor * (diff * uMaterial.diffuseColor);
	
	// SPECULAR --------------------------------------------------------------
	vec3 viewDir = normalize(uViewerPos - CurrentFragPos);
	vec3 reflectionDir = reflect(-lightDir, normal);	
	float spec = pow(max(dot(viewDir, reflectionDir), 0.0), uMaterial.shininess);
	vec3 specular = uLight.specularColor * (spec * uMaterial.specularColor);

	vec3 result = ambient + diffuse + specular;
	FragColor = vec4(result, 1.0);
}  