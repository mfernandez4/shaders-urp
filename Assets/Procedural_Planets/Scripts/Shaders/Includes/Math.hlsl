static const float PI = 3.14159265359;
static const float TAU = PI * 2;
static const float maxFloat = 3.402823466e+38;


// Returns vector (distToSphere, distThroughSphere)
// If ray origin is inside sphere, distToSphere = 0
// If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
float2 raySphere(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir)
{
    float3 offset = rayOrigin - sphereCenter;
    float a = 1; // Set to dot(rayDir, rayDir) if rayDir is not normalized
    float b = 2 * dot(offset, rayDir);
    float c = dot (offset, offset) - sphereRadius * sphereRadius;
    float discriminant = b * b - 4 * a * c; // b^2 - 4ac : Discriminant of quadratic formula

    // Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
    if (discriminant > 0 )
    {
        float s = sqrt(discriminant);
        float dstToSphereNear = max(0, (-b - s) / (2 * a));
        float dstToSphereFar = (-b + s) / (2 * a);

        // Ignore intersections that occur behind the ray
        if (dstToSphereFar >= 0)
        {
            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
        }
    }
    // Ray did not intersect sphere
    return float2(maxFloat, 0);
}