//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS("trigger", "attack(ms)", "release(ms)");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => ADSR adsr => RAYTONE_OUTPUT => dac;
1 => float attack;
100 => float release;

// infinite loop
while (true)
{
	// Wait for tick
	RAYTONE_TICK => now;
	
	// Read trig value
	if(RAYTONE_TRIG(0) == 1)
	{
		// Close previous envelope
		0::samp => adsr.releaseTime;
		adsr.keyOff();
		1::samp => now;
		
		// Read attack inlet; set to 1 if not connected
		if (RAYTONE_INLET_STATUS(1) == 1)
		{
			Math.max(RAYTONE_INLET(1), 1) => attack;
		}
		else 
		{
			1 => attack;
		}

		// Read release inlet; set to 100 if not connected
		if (RAYTONE_INLET_STATUS(2) == 1)
		{
			Math.max(RAYTONE_INLET(2), 1) => release;
		}
		else 
		{
			100 => release;
		}

		// Start envelope
		adsr.set( attack::ms, 0::ms, 1, release::ms );
		adsr.keyOn();
		
		// Advance time 
		spork ~ advanceTime();
	}	
}

// Start the release phase after attack phase has ended
fun void advanceTime()
{
    attack::ms => now;
    adsr.keyOff();
}