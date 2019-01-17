uint GetBit(uint x, uint y);

__kernel void device_function(__global uint* second)
{
	uint x = get_global_id(0) + 1;
	uint y = get_global_id(1) + 1;
	uint pattern;


	//BitSet(x,y) = 0;
	pattern = 0;

	// count active neighbors
    uint n = GetBit(x - 1, y - 1) + GetBit(x, y - 1) + GetBit(x + 1, y - 1) + GetBit(x - 1, y) +
    GetBit(x + 1, y) + GetBit(x - 1, y + 1) + GetBit(x, y + 1) + GetBit(x + 1, y + 1);
    if ((GetBit(x, y) == 1 && n == 2) || n == 3) 
		//BitSet(x, y);
		pattern = 1;

	second[y * 512 + (x >> 5)] = pattern;
}

// helper function for setting one bit in the pattern buffer
//void BitSet(uint x, uint y)
//{
    //pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31);
//}
// helper function for getting one bit from the secondary pattern buffer
uint GetBit(uint x, uint y)
{
    return (second[y * 512 + (x >> 5)] >> (int)(x & 31)) & 1U;
}




