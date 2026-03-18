using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private int maxSpeed = 300;
	[Export]
	private AnimatedSprite2D anim;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		Vector2 axisPowers = Input.GetVector("left", "right", "up", "down");
		if(axisPowers == Vector2.Zero){
			//Idle anim
			anim.Play("idle");
		}else{
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
		Velocity = axisPowers * maxSpeed;
		//GD.Print(Velocity);
		//GD.Print(Position);
		MoveAndSlide();
	}
}
