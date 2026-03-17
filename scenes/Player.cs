using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private int maxSpeed = 300;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		Vector2 axisPowers = Input.GetVector("left", "right", "up", "down");
		if(axisPowers == Vector2.Zero){
			//Friction
		}else{
			//Acceleration
		}
		Velocity = axisPowers * maxSpeed;
		//GD.Print(Velocity);
		//GD.Print(Position);
		MoveAndSlide();
	}
}
