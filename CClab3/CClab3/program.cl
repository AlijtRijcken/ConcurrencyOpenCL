uint GetBit(uint x, uint y, __global uint* _second);

__kernel void device_function(__global uint* second)
{
	uint x = get_global_id(0) + 1;
	uint y = get_global_id(1) + 1;
	uint id = y * 512 + (x >> 5);
	printf("%u", second[y + 512 * (x >> 5)]);
	uint pattern = 0;

	// count active neighbors
    uint n = GetBit(x - 1, y - 1, second) + GetBit(x, y - 1, second) + GetBit(x + 1, y - 1, second) + GetBit(x - 1, y, second) +
    GetBit(x + 1, y, second) + GetBit(x - 1, y + 1, second) + GetBit(x, y + 1, second) + GetBit(x + 1, y + 1, second);
    if ((GetBit(x, y, second) == 1 && n == 2) || n == 3) 
		//BitSet(x, y);
		pattern = 1;

	second[id] = pattern;

}

// helper function for setting one bit in the pattern buffer
//void BitSet(uint x, uint y)
//{
    //pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31);
//}
// helper function for getting one bit from the secondary pattern buffer
uint GetBit(uint x, uint y, __global uint* _second)
{
    return (_second[y * 512 + (x >> 5)] >> (int)(x & 31)) & 1U;
}