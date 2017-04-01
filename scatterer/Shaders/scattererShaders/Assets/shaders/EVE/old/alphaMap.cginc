#ifndef ALPHA_MAP_CG_INC
#define ALPHA_MAP_CG_INC

#ifndef ALPHAMAP_N_1
#define ALPHAMAP_N_1 0
#endif

#ifndef ALPHAMAP_R_1
#define ALPHAMAP_R_1 0
#endif

#ifndef ALPHAMAP_G_1
#define ALPHAMAP_G_1 0
#endif

#ifndef ALPHAMAP_B_1
#define ALPHAMAP_B_1 0
#endif

#ifndef ALPHAMAP_A_1
#define ALPHAMAP_A_1 0
#endif

inline half vectorSum(half4 v) 
{
	return (v.x + v.y + v.z + v.w);
}

#define ALPHA_VALUE_1(color) \
	vectorSum( color * half4((ALPHAMAP_R_1), (ALPHAMAP_G_1), (ALPHAMAP_B_1), (ALPHAMAP_A_1)) )

#if ALPHAMAP_R_1 == 1 || ALPHAMAP_G_1 == 1 || ALPHAMAP_B_1 == 1 || ALPHAMAP_A_1 == 1
#define ALPHA_COLOR_1(color) half4(1, 1, 1, ALPHA_VALUE_1(color))
#else
#define ALPHA_COLOR_1(color) color
#endif

#endif