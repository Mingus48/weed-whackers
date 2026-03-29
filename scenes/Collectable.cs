using Godot;
using System;

public partial class Collectable : RigidBody2D
{
	[Export]
	private AnimationPlayer anim;
	[Export]
	private TileMapLayer pic;
	public string plant;
	private Barn barnScript;
	private bool trackPlayer = false;
	private CharacterBody2D player;
	private int speed = 100;
	private float startingY;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		barnScript = (Barn)GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(Position.Y >= startingY){
			GravityScale = 0;
			LinearVelocity = new Vector2(LinearVelocity.X, 0);
			LinearDamp = 5;
		}
		if(trackPlayer){
			LinearVelocity = ToLocal(player.Position).Normalized() * speed;
			if((Position - player.Position).Length() < 16){
				anim.Play("delete");
			}
		}
	}

	public void setTile(string plant){
		Vector2I atlasCords = barnScript.plantIdx[plant];
		atlasCords.X -= 5;
		pic.SetCell(Vector2I.Zero, 0, atlasCords);
		Random rng = new Random();
		int rng1 = rng.Next(30);
		int rng2 = rng.Next(2);
		int theta = 0;
		if(rng2 == 0){
			theta = 30 + rng1;
		}else{
			theta -= 30 - rng1;
		}
		float tan = Mathf.Tan(Mathf.DegToRad(theta));
		startingY = Position.Y;
		Position = new Vector2(Position.X, Position.Y - 1);
		LinearVelocity = new Vector2(tan, -1).Normalized() * 60;
	}

	private void _on_area_2d_body_entered(Node2D body){
		if(body.Name == "Player"){
			trackPlayer = true;
			player = (CharacterBody2D)body;
		}
	}
}
