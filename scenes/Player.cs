using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	private AnimatedSprite2D anim;
	[Export]
	private AnimationTree animTree;
	private int maxSpeed = 75;
	private float acceleration = .4f;
	private float friction = .2f;
	private bool isRunning;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		Vector2 axisPowers = Input.GetVector("left", "right", "up", "down");
		if(axisPowers == Vector2.Zero){
			//Friction
			Velocity = Velocity.Lerp(Vector2.Zero, friction);
			//Idle anim
			isRunning = false;
			animTree.Set("parameters/IdleRun/IdleSpace/blend_position", axisPowers);
		}else{
			//Acceleration
			Velocity = Velocity.Lerp(axisPowers * maxSpeed, acceleration);
			animTree.Set("parameters/IdleRun/RunSpace/blend_position", axisPowers);
			//Move anim
			isRunning = true;
		}
		MoveAndSlide();
	}
}
