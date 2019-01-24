uint OutOfBoundGetBit(uint x, uint y, uint pw, uint ph, uint breedte, __global uint* _in_);
uint GetBit(uint x, uint y, uint pw, __global uint* _in_);

__kernel void device_function(__global uint* _in, __global uint* _out, uint pw, uint ph, uint breedte)
{
	uint x = get_global_id(0);
	uint y = get_global_id(1);
	
	//aparte kernel voor de rand? en een aparte voor het midden

	_out[y * pw + (x >> 5)] = 0; 

	// count active neighbors
    uint n = OutOfBoundGetBit(x - 1, y - 1, pw, ph, breedte, _in) + OutOfBoundGetBit(x, y - 1, pw, ph, breedte, _in) + OutOfBoundGetBit(x + 1, y - 1, pw, ph, breedte, _in) 
		+ OutOfBoundGetBit(x - 1, y, pw, ph, breedte, _in) + OutOfBoundGetBit(x + 1, y, pw, ph, breedte, _in) + OutOfBoundGetBit(x - 1, y + 1, pw, ph, breedte, _in)
		+ OutOfBoundGetBit(x, y + 1, pw, ph, breedte, _in) + OutOfBoundGetBit(x + 1, y + 1, pw, ph, breedte, _in);
    if ((OutOfBoundGetBit(x, y, pw, ph, breedte, _in) == 1 && n == 2) || n == 3)
	{
		atomic_or(&_out[y * pw + (x >> 5)], 1U << (int)(x & 31));
	}
}

uint GetBit(uint x, uint y, uint pw, __global uint* _in_)
{
    return (_in_[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U;
}

uint OutOfBoundGetBit(uint x, uint y, uint pw, uint ph, uint breedte, __global uint* _in_) 
{
	uint xx = x;
	uint yy = y;

	//eerst afvangen of je op de rand zit moet je eerst doen en dan er pas een van af trekken. 
	//apart de rechter en linker buur afhandelen
	//uint kan geen -1 zijn, dus eerst checken of de betreffende x op 0 ligt.
	// ph is wel y max, pw is eigenlijk 1714. DUS IPV te werken met ph werken we nu met breedte.
	//VOLGENS NIELS ZOUDEN DE BORDERS GOED MOETEN STAAN EN NIET MET -1 MEEGENOMEN

	//check de hoek apart want dat is een andere case...
	//hoek links onder (0, ph)
	if (x == 0 & y == ph)
	{
		xx = breedte;
		yy = 0;
	}
	//check de buitenste hoek (breedte, 0)
	if (x == breedte & y == 0)
	{
		xx = 0; 
		yy = ph;
	}
	//hoek rechts beneden (breedte, ph)
	if ( x == breedte & y == ph)
	{
		xx = 0;
		yy = 0; 
	}
	//op het moment dat je x == 0 dan weetje dat je aan de linker border zit en wil je dus dat je - 1 buur -> je meest rechter cel wordt (breedte)
	if (x == 0)
	{
		xx = breedte;
	}
	//op het moment dat je y == 0 dan weet je dat je aan de boven kant zit en wil je dat je -1 buur -> je meest onderste cel wordt (ph)
	if (y == 0) 
	{
		yy = ph; 
	}
	//als je aan de rechterkant zit wil je dat je pw dus 0 wordt
	if (x == breedte)
	{
		xx = 0; 
	}
	//als je aan de onderkant zit wil je dat je weer uit komt aan de boven kant, yy = 0; 
	if (y == ph)
	{
		yy = 0; 
	}

	return GetBit(xx, yy, pw, _in_); 
}

//laatste x gaat mss niet alle bits gebruiken, als de lengte geen multipule