#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUv;

out vec2 uv;
out vec3 normal;
out vec3 fragpos;
out vec3 viewpos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
    uv = aUv;
    normal = normalize(mat3(transpose(inverse(model))) * aNormal);
    fragpos = vec3(model * vec4(aPosition, 1.0));
    viewpos = vec3(inverse(view)[3]);
    gl_Position = proj * view * model * vec4(aPosition, 1.0);
}