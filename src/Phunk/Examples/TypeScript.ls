class Greeter
    constructor(string greeting) = () => this.greeting = greeting
	
    greet() 
        "<h1>" + this.greeting + "</h1>";
    

greeter = Greeter("Hello, world!");
    
document.body.innerHTML = greeter.greet();