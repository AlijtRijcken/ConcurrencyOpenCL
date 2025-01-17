uint GetBit(uint x, uint y, uint pw, __global uint* _in_);

__kernel void device_function(__global uint* _in, __global uint* _out, uint pw, uint ph, uint breedte, int Wrap)
{
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	bool Edge = true;
	
	_out[y * pw + (x >> 5)] = 0; 

	if (Wrap == 0)
		if (x == 0 || y == 0 || x == breedte || y == ph)
			Edge = false;
		

	// count active neighbors
    uint n = GetBit(((x + breedte - 1)% breedte), ((y + ph - 1) % ph), pw, _in) + GetBit(x, ((y + ph - 1) % ph), pw, _in) + GetBit(((x + breedte + 1) % breedte), ((y + ph - 1) % ph), pw, _in) + GetBit(((x + breedte - 1) % breedte), y, pw, _in) +
			 GetBit(((x + breedte + 1) % breedte), y, pw, _in) + GetBit(((x + breedte - 1) % breedte), ((y + ph + 1) % ph), pw, _in) + GetBit(x, ((y + ph + 1) % ph), pw, _in) + GetBit(((x + breedte+ 1) % breedte), ((y + ph + 1) % ph), pw, _in);
    if ((GetBit(x, y, pw, _in) == 1 && n == 2) || (n == 3 && Edge))
	{
		atomic_or(&_out[y * pw + (x >> 5)], 1U << (int)(x & 31));
	}
	
}

uint GetBit(uint x, uint y, uint pw, __global uint* _in_)
{
    return (_in_[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U;
}
