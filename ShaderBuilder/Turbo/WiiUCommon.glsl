#if (IS_WII_U == 1)

// Due to using version 330 glsl, add extra functions 

float fma(float a, float b, float c)
{
    return a * b + c;
}
vec2 fma(vec2 a, vec2 b, vec2 c)
{
    return a * b + c;
}
vec3 fma(vec3 a, vec3 b, vec3 c)
{
    return a * b + c;
}
vec4 fma(vec4 a, vec4 b, vec4 c)
{
    return a * b + c;
}

#endif // IS_WII_U