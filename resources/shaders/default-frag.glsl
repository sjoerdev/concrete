#version 330 core

in vec2 uv;
in vec3 normal;
in vec3 fragpos;
in vec3 viewpos;

out vec4 color;

uniform sampler2D tex;

struct DirectionalLight
{
    float brightness;
    vec3 color;
    vec3 direction;
};

struct PointLight
{
    float brightness;
    vec3 color;
    vec3 position;
    float range;
};

struct SpotLight
{
    float brightness;
    vec3 color;
    vec3 position;
    vec3 direction;
    float range;
    float angle;
    float softness;
};

#define MAX_LIGHTS 16

uniform DirectionalLight dirLights[MAX_LIGHTS];
uniform PointLight pointLights[MAX_LIGHTS];
uniform SpotLight spotLights[MAX_LIGHTS];

vec3 CalcDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewdir)
{
    vec3 lightDir = normalize(-light.direction);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewdir, reflectDir), 0.0), 32.0);
    vec3 ambient = light.color * 0.1;
    vec3 diffuse = light.color * diff;
    vec3 specular = light.color * spec;
    return light.brightness * (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragpos, vec3 viewdir)
{
    vec3 lightDir = normalize(light.position - fragpos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewdir, reflectDir), 0.0), 32.0);
    float distance = length(light.position - fragpos);
    float attenuation = 1.0 / (1.0 + (distance / light.range));
    vec3 ambient = light.color * 0.1 * attenuation;
    vec3 diffuse = light.color * diff * attenuation;
    vec3 specular = light.color * spec * attenuation;
    return light.brightness * (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragpos, vec3 viewdir)
{
    vec3 lightDir = normalize(light.position - fragpos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewdir, reflectDir), 0.0), 32.0);
    float distance = length(light.position - fragpos);
    float attenuation = 1.0 / (1.0 + (distance / light.range));
    float theta = dot(lightDir, normalize(-light.direction));
    float angle = radians(light.angle);
    float softness = clamp(light.softness, 0.0, 1.0);
    float epsilon = angle * (1.0 - softness);
    float intensity = smoothstep(cos(angle + epsilon), cos(angle), theta);
    vec3 ambient = light.color * 0.1 * attenuation * intensity;
    vec3 diffuse = light.color * diff * attenuation * intensity;
    vec3 specular = light.color * spec * attenuation * intensity;
    return light.brightness * (ambient + diffuse + specular);
}

void main()
{
    vec3 viewdir = normalize(viewpos - fragpos);

    vec3 albedo = texture(tex, uv).rgb;

    vec3 light = vec3(0);

    // Directional lights
    for(int i = 0; i < dirLights.length(); i++)
        light += CalcDirectionalLight(dirLights[i], normal, viewdir);
    
    // Point lights
    for(int i = 0; i < pointLights.length(); i++)
        light += CalcPointLight(pointLights[i], normal, fragpos, viewdir);
    
    // Spot lights
    for(int i = 0; i < spotLights.length(); i++)
        light += CalcSpotLight(spotLights[i], normal, fragpos, viewdir);

    color = vec4(albedo * light, 1.0);
}