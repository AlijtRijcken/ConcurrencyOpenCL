uint GetBit(uint x, uint y, uint pw, __global uint* _second);

__kernel void device_function(__global uint* second, __global uint* pattern, uint pw)
{
	uint x = get_global_id(0) + 1;
	uint y = get_global_id(1) + 1;

	// count active neighbors
    uint n = GetBit(x - 1, y - 1, pw, second) + GetBit(x, y - 1, pw, second) + GetBit(x + 1, y - 1, pw, second) + GetBit(x - 1, y, pw, second) +
    GetBit(x + 1, y, pw, second) + GetBit(x - 1, y + 1, pw, second) + GetBit(x, y + 1, pw, second) + GetBit(x + 1, y + 1, pw, second);
    if ((GetBit(x, y, pw, second) == 1 && n == 2) || n == 3)
	{
		pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31);
	}
	
}

uint GetBit(uint x, uint y, uint pw, __global uint* _second)
{
    return (_second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U;
}