namespace Galbet.CharacterBehaviors {
	
			//returner += lateral.doWalk(
			//	new LateralWalkParams(lateralVelocity), //move speeds
			//	DeltaTime,
			//	horizontal, //input - just in case move speeds change by input
			//	facing.x //For selecting between forward & backward speeds.
			//);
			//returner += jump.doJump(false, collisions, DeltaTime); //Just to ensure gravity is reset.
			//returner += gravity.doGravity(DeltaTime, collisions);

			//Do physics based movement.
			//The movement speed is + release velocity until the player hits
			//the ground. If above threshold, the forward button is
			//additionally mapped to the same value as no-input.
			//Holding back will be unchanged if entered by letting go,
			//hence slowing the player just a bit.
			//When bounced or hurled, should recognize descending slopes,
			//but not use them except if the player is trying to move
			//that way as they impact. And then they still have to slow 
			//down and stop before movement becomes normal.

			//Facing dir changes by input, but affects nothing except aesthetics.
}