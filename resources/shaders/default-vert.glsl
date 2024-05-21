#version 330 core

layout (location = 0) in vec3 vertex;

uniform mat4 view;
uniform mat4 proj;

void main()
{
    gl_Position = proj * view * vec4(vertex, 1.0);
}