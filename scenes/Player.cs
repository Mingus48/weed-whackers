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
			//anim.Play("idle");
		}else{
			//Acceleration
			Velocity = Velocity.Lerp(axisPowers * maxSpeed, acceleration);
			//animTree.Set("parameters/Idle Run/Run/blend_position", axisPowers);
			//Move anim
			if(axisPowers.X != 0){
				if(axisPowers.X > 0){
					anim.Play("runRight");
				}else{
					anim.Play("runLeft");
				}
			}else if(axisPowers.Y > 0){
				anim.Play("runDown");
			}else{
				anim.Play("runUp");
			}
		}
		GD.Print(Velocity);
		//GD.Print(Position);
		MoveAndSlide();
	}
}
