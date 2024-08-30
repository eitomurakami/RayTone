//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS();
RAYTONE_DEFINE_INLETS("trigger", "rate", "pos(ms)");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(true);
//-----------------------------------------------------------------------------

// DSP chain
SndBuf buf => RAYTONE_OUTPUT => dac;
RAYTONE_FILEPATH => buf.read;
0 => buf.gain;

// infinite loop
while (true)
{
	// Wait for tick
    RAYTONE_TICK => now;

	// Check for trigger
	if (RAYTONE_TRIG(0) == 1)
	{
		RAYTONE_LOCAL_GAIN => buf.gain;

		// Read rate inlet; set to 1 if not connected
		if (RAYTONE_INLET_STATUS(1) == 1)
		{
			RAYTONE_INLET(1) => buf.rate;
		}
		else
		{
			1 => buf.rate;
		}

		// Read playback position in ms
		(RAYTONE_INLET(2) / (buf.length() / 1::ms) * buf.samples()) $ int => buf.pos;
	}
}

