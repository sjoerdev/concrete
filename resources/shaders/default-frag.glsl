#version 330 core

in vec3 normal;
in vec2 uv;

out vec4 color;

void main()
{
    color = vec4(normal, 1.0);
}