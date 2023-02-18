#version 330 core

#define MAX_POINT_LIGHTS 5
#define MAX_SPOTLIGHTS 5

// structure for only one directional light
struct DirLight {
	vec3 direction;
	
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

// structure for point lights
struct PointLight {
	vec3 position;

	float constant;
	float linear;
	float quadratic;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

// structure for spotlight
struct SpotLight {
	vec3 position;
	vec3 direction;
	float cutOff;
	float outerCutOff;

	float constant;
	float linear;
	float quadratic;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};


// Functions prototypes
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 color, float shadow);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 color);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 color);
vec3 CaclFog(DirLight light, vec3 color, vec3 worldPos);

in vec2 TexCoords;
out vec4 FragColor;

// G-Buffer uniforms
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

// Camera position
uniform vec3 viewPos;

// Lights
uniform DirLight dirLight;
//.
uniform PointLight pointLights[MAX_POINT_LIGHTS];
uniform int nrPointLights;
//.
uniform SpotLight spotLights[MAX_SPOTLIGHTS];
uniform int nrSpotlights;

// Material constant values
const int shininess = 32;
const vec3 specular = vec3(0.5f, 0.5f, 0.5f);

// Fog
const float fogGradient = 8.6; // <= (fogIntensity * fogIntensity - 50 * fogIntensity + 60)
const vec3 fogColor = vec3(0.2, 0.3, 0.3);


void main()
{
	vec3 fragPos = texture(gPosition, TexCoords).rgb;
	vec3 normal = texture(gNormal, TexCoords).rgb;
	vec3 color = texture(gAlbedoSpec, TexCoords).rgb;
	vec3 viewDir = normalize(viewPos - fragPos);

	// 1) Phase : Directional lighting
	vec3 result = CalcDirLight(dirLight, normal, viewDir, color, 0);

	// 2) Phase : Point lights
	for (int i = 0; i < nrPointLights; i++) {
		result += CalcPointLight(pointLights[i], normal, fragPos, viewDir, color);
	}

	// 3) Phase : Spotlights
	for (int i = 0; i < nrSpotlights; i++) {
		result += CalcSpotLight(spotLights[i], normal, fragPos, viewDir, color);
	}

	// 4) Phase : Fog
	result = CaclFog(dirLight, result, fragPos);

	FragColor = vec4(result, 1);
}


vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 color, float shadow)
{
	vec3 lightDir = normalize(-light.direction);

	// for diffusion
	float diff = max(dot(normal, lightDir), 0.0);

	// for specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

	// Mixing :::
	vec3 ambient = light.ambient * color;
	vec3 diffuse = light.diffuse * diff * color;
	vec3 specular = light.specular * spec * specular;

	// for night
	lightDir = normalize(viewPos);

	// for diffuse night
	diff = max(dot(normal, lightDir), 0.0);

	// for specular night
	reflectDir = reflect(-lightDir, normal);
	spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

	// Mixing night :::
	vec3 diffuseNight = light.diffuse * diff * color * light.ambient * 0.6f;
	vec3 specularNight = light.specular * spec * specular * light.ambient * 0.125;
	vec3 nightAmbient = diffuseNight + specularNight;

	// for shadow of the sunset
	float height = light.direction.y;
	if (height > 0) {
		shadow = min(shadow + height / 4, 1);
		ambient = mix(ambient, nightAmbient, shadow);
	}

	// ---------- RETURN -----------------------------------
	return (ambient + (1.0 - shadow) * (diffuse + specular));
}


vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 color)
{
	vec3 lightDir = normalize(light.position - fragPos);

	// for diffuse
	float diff = max(dot(normal, lightDir), 0.0);

	// for specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

	// for attenuation
	float distance = length(light.position - fragPos);
	float attenuation = 1.0 / (light.constant + light.linear * distance + 
  							  light.quadratic * (distance * distance));

	// Mixing :::
	vec3 ambient = light.ambient * color;
	vec3 diffuse = light.diffuse * diff * color;
	vec3 specular = light.specular * spec * specular;

	ambient  *= attenuation;
	diffuse  *= attenuation;
	specular *= attenuation;

	// ---------- RETURN ----------------
	return (ambient + diffuse + specular);
}


vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 color)
{
	vec3 lightDir = normalize(light.position - fragPos);

	// for diffuse
	float diff = max(dot(normal, lightDir), 0.0);

	// for specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

	// for attenuation
	float distance = length(light.position - fragPos);
	float attenuation = 1.0 / (light.constant + light.linear * distance +
  							  light.quadratic * (distance * distance));

	// for spotlight
	float theta     = dot(lightDir, normalize(-light.direction));
	float epsilon   = light.cutOff - light.outerCutOff;
	float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

	// Mixing :::
	vec3 ambient = light.ambient * color;;
	vec3 diffuse = light.diffuse * diff * color;;
	vec3 specular = light.specular * spec * specular;

	ambient  *= attenuation;
	diffuse  *= attenuation * intensity;
	specular *= attenuation * intensity;

	// ---------- RETURN ----------------
	return (ambient + diffuse + specular);
}


vec3 CaclFog(DirLight light, vec3 color, vec3 worldPos)
{
	// for disntace
	float distance = length(viewPos - worldPos);

	// for fog factor
	float fog = exp(-pow((distance / fogGradient), 4));
	fog = 1 -  clamp(fog, 0.0, 1.0);

	float shadow = 0;
	float height = light.direction.y;

	// for sunset hegith >= 0
	if (height >= 0) {
		shadow = min(shadow + height * height / 3, 0.8);
	}

	// ---------- RETURN --------------------------
	return mix(color, (1 - shadow) * fogColor, fog);
}