#ifndef COMPARISON_INCLUDED
#define COMPARISON_INCLUDED

void LessThanEqual_float(float3 a, float3 b, out bool value)
{
    value = all(a <= b);
}

void GreaterThanEqual_float(float3 a, float3 b, out bool value)
{
    value = all(a >= b);
}

#endif