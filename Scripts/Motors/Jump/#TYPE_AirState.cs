namespace Clouds.Platformer.Character {
	[System.Flags]
	public enum VerticalCollisions {
		Falling = 0,
		Grounded = 1,
		GoingUp = 2, 
		HeadHit = 4,
		HitByOther = 8
	}
}