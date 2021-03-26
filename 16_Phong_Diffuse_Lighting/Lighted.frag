#version 330 core
out vec4 FragColor;
  
uniform vec3 uObjectColor;
uniform vec3 uLightColor;
uniform vec3 uLightSourcePos;

in vec3 Normal;
in vec3 CurrentFragPos;

void main()
{
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * uLightColor;

	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(uLightSourcePos - CurrentFragPos);
	
	// Calculate the diffuse impact of the light on the current fragment by 
	// taking the dot product between the norm and lightDir vectors.
	float dotProduct = dot(normal, lightDir);
	
	/*************************************************************************
		The  resulting value is then multiplied with the light's color to 
	 get the diffuse component, resulting in a darker diffuse component the 
	 greater the angle between both vectors.
		If the angle between both vectors is greater than 90 degrees then 
	 the result of the dot product will actually become negative and we end 
	 up with a negative diffuse component. For that reason we use the max 
	 function that returns the highest of both its parameters to make sure 
	 the diffuse component (and thus the colors) never become negative. 
		Lighting for negative colors is not really defined so it's best to 
	 stay away from that, unless you're one of those eccentric artists.
    *************************************************************************/
	float diff = max(dotProduct, 0.0);
	vec3 diffuse = diff * uLightColor;
	
	/*************************************************************************
	 Now that we have both an ambient and a diffuse component we add both 
	 colors to each other and then multiply the result with the color of 
	 the object to get the resulting fragment's output color:
    *************************************************************************/
	vec3 result = (ambient + diffuse) * uObjectColor;
	FragColor = vec4(result, 1.0);
}  