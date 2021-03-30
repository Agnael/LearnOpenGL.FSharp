#version 330 core
out vec4 FragColor;
  
uniform vec3 uLightColor;
uniform vec3 uLightSourcePos;
uniform vec3 uViewerPos;

in vec3 Normal;
in vec3 CurrentFragPos;

struct Material {
	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;
	float shininess;
};

uniform Material uMaterial;

void main()
{
	// AMBIENT
    vec3 ambient = uLightColor * uMaterial.ambientColor;

	// DIFFUSE ---------------------------------------------------------------
	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(uLightSourcePos - CurrentFragPos);	
	float diffuseImpact = dot(normal, lightDir);	
	float diff = max(diffuseImpact, 0.0);
	vec3 diffuse = uLightColor * (diff * uMaterial.diffuseColor);
	
	// SPECULAR --------------------------------------------------------------
	vec3 viewDir = normalize(uViewerPos - CurrentFragPos);
	vec3 reflectionDir = reflect(-lightDir, normal);	
	float spec = pow(max(dot(viewDir, reflectionDir), 0.0), uMaterial.shininess);
	vec3 specular = uLightColor * (spec * uMaterial.specularColor);

	vec3 result = ambient + diffuse + specular;
	FragColor = vec4(result, 1.0);
}  