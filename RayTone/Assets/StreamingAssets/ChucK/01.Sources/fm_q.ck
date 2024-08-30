//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS();
RAYTONE_DEFINE_INLETS("freq", "ratio", "index");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
// carrier
SinOsc c => RAYTONE_OUTPUT => dac;
// modulator
SinOsc m => blackhole;

0 => float cf;
0 => float ratio;
0 => float index;

0 => c.gain;
spork ~ inletLoop();

// DSP loop
while (true)
{
	cf + (index * m.last()) => c.freq;
	1::samp => now;
}

// Inlet loop
fun void inletLoop()
{
	while (true)
	{
		// Wait for tick
		RAYTONE_TICK => now;
		
		// Silence operator if frequency is 0
		if (RAYTONE_INLET(0) == 0)
		{
			0 => c.gain;
		}
		else
		{
			RAYTONE_LOCAL_GAIN => c.gain;

			RAYTONE_INLET(0) => cf;
			RAYTONE_INLET(1) => ratio;
			RAYTONE_INLET(2) => index;

			cf * ratio => m.freq;
			cf + (index * m.last()) => c.freq;
		}
	}
}

