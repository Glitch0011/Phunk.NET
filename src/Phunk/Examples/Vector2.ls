class Vector2
	constructor(int x, int x)
		this.x = x
		this.y = y

	+(Vector2 a, Vector2 b)
		return Vector2(a.x + b.x, a.y + b.y)

	string(Vector2 a)
		return "{x},{y}"

	length => return (x*x)+(y*y)

	normalised => this / length

a = Vector2(1,2)
b = a + Vector2(3,4) //b now equals {b}