#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUv;
layout (location = 3) in vec4 joints;
layout (location = 4) in vec4 weights;

out vec2 uv;
out vec3 normal;
out vec3 fragpos;
out vec3 viewpos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

uniform mat4 jointMatrices[100];

void main()
{
    vec4 animatedPosition = vec4(0);
    vec3 animatedNormal = vec3(0);
    for (int i = 0 ; i < 4; i++)
    {
        int index = int(joints[i]);
        float weight = weights[i];

        if (index == -1 || weight == -1 || index >= 100) continue;

        mat4 matrix = jointMatrices[index];
        vec4 local = matrix * vec4(aPosition, 1.0);
        animatedPosition += local * weight;
        animatedNormal = mat3(matrix) * aNormal;
    }

    uv = aUv;
    normal = normalize(mat3(transpose(inverse(model))) * animatedNormal);
    fragpos = vec3(model * animatedPosition);
    viewpos = vec3(inverse(view)[3]);
    gl_Position = proj * view * model * animatedPosition;
}