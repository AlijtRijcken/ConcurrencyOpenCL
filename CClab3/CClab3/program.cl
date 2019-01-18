uint GetBit(uint x, uint y, uint pw, __global uint* _in_);

__kernel void device_function(__global uint* _in, __global uint* _out, uint pw)
{
	uint x = get_global_id(0) + 1;
	uint y = get_global_id(1) + 1;

	_out[y * pw + (x >> 5)] = 0;

	// count active neighbors
    uint n = GetBit(x - 1, y - 1, pw, _in) + GetBit(x, y - 1, pw, _in) + GetBit(x + 1, y - 1, pw, _in) + GetBit(x - 1, y, pw, _in) +
    GetBit(x + 1, y, pw, _in) + GetBit(x - 1, y + 1, pw, _in) + GetBit(x, y + 1, pw, _in) + GetBit(x + 1, y + 1, pw, _in);
    if ((GetBit(x, y, pw, _in) == 1 && n == 2) || n == 3)
	{
		_out[y * pw + (x >> 5)] |= 1U << (int)(x & 31);
	}
}

uint GetBit(uint x, uint y, uint pw, __global uint* _in_)
{
    return (_in_[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U;
}