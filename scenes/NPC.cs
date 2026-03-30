using Godot;
using System;

public partial class NPC : CharacterBody2D{
	[Export]
	private Sprite2D speech;
	[Export]
	private Sprite2D heart;
	[Export]
	private Sprite2D fruit;
	private AnimatedSprite2D anim;
	private bool atDoor = true;
	private string desire;
	private Barn barnScript;
	private string[] veggies = {"turnip", "tomato", "melon", "eggplant", "lemon", "wheat", "strawberry", "potato", "orange", "corn"};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		barnScript = (Barn)GetParent();
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(atDoor){
			anim.Play("idle");
			speech.Visible = true;
			fruit.Visible = true;
			Random rng = new Random();
			desire = veggies[rng.Next(veggies.Length)];
			fruit.FrameCoords = new Vector2I(barnScript.plantIdx[desire].X - 5, barnScript.plantIdx[desire].Y);
			atDoor = false;
		}
	}
}
