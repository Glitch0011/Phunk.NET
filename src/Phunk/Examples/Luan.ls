class Vector2
	constructor(float x, float y)
		this.x = x;
		this.y = y;
	
	+(Vector2 a, Vector2 b)
		return Vector2(a.x + b.x, a.y + b.y);
	
	-(Vector2 a, Vector2 b)
		return Vector2(a.x - b.x, a.y - b.y);
	
	/(Vector2 a, float b)
		return Vector2(a.x / b, a.y / b);
	
	,(Vector2 a, Vector2 b)
		return a + b;
		
	length => return (x * x) + (y * y);
	
	unitVector => return this / length;
	
	normalised => return unitVector;
	
	string()
		return "{x}:{y}" //Converting Vector2 to string/String

third = (v) => return v /3

//When reading, you load until you see a line with text further right, or the end.
class HomingFireball
	onCast(lifeAmount)
		fireMana = mana.take(third lifeAmount).manifest("fire");
		speedMana = mana.take(third lifeAmount).manifest("speed");
		lifeMana = mana.take(third lifeAmount); // Create {lifeAmount} of life
		
		entity = (fireMana, speedMana, lifeMana).manifestEntity("HomingFireball"); //manifestEntity has to load the main class each time
		entity.TargetPosition = Vector2(10,10);
		
	onTurn()
		this.Move() //Move in direction
		this.MoveTo() //Move to pos
		this.Position += (this.targetPosition - this.Position).unitVector; //Can't do this, position is read-only
		
		if (this.postDetonate <= 0)
			this.mana.manifest("fire"); //Converting mana into {this.mana.count} fire
			
		this.postDetonate = this.postDetonate - 1 ?? 10 // null - anything = null
		
class HealingTree
	constructor()
		one = 1

	onCastAtPos(Vector2 pos)
		strength = mana.take(10).manifest("life");
		tree = (mana.take(100) + strength).manifest("HealingTree", pos); //Creates a Healing Tree at {pos} with 10 life, giving it 100 mana
		
	onTurnEnd()
		foreach (entity in this.NearbyEntitites(2))
			this.take(one, m => m != "life").manifest("life").give(entity); //Giving {1} life to {entity}
			
class Life
	constructor()
		this.Colour = Vector2(10, 10);

//Array, ManaPool, Simulation
//Most of the engine tries to be case-insensitive (but uses the most correct)

//case-curious, mostly-typed


