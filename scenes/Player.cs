using Godot;
using System;

public partial class Player : CharacterBody2D{
	[Export]
	private AnimationTree animTree;
	[Export]
	private TileMapLayer seedsLayer;
	private int maxSpeed = 75;
	private float acceleration = .4f;
	private float friction = .2f;
	private bool isRunning;
	private bool isWatering;
	private Vector2 lastAxis = new Vector2(0, 1);
	private Barn barnScript;
	private int mode = 0;
	private string currentPlant = "turnip";
	private Vector2I lastSeedPos = Vector2I.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		barnScript = (Barn)GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(Input.IsActionPressed("farmMode")){
			mode = 1 - mode;
		}
		if(Input.IsActionJustPressed("water")){
			GD.Print("hi");
			if(isWatering){
				isWatering = false;
			}else{
				isWatering = true;
			}
		}

		//Handles planting
		if(mode == 1){
			Vector2 offset = -1 * (Position % 16);
			offset += new Vector2(16, 16);
			seedsLayer.Position = offset;
			Vector2 mousePos = seedsLayer.ToLocal(GetGlobalMousePosition());
			Vector2I coords = seedsLayer.LocalToMap(mousePos);
			if(coords != lastSeedPos){
				seedsLayer.EraseCell(lastSeedPos);
				seedsLayer.SetCell(coords, 0, barnScript.plantIdx[currentPlant]);
				lastSeedPos = coords;
				barnScript.showBonuses(currentPlant, GetGlobalMousePosition());
			}
			if(Input.IsActionJustPressed("leftClick")){
				GD.Print("hi");
				barnScript.addSeed(currentPlant, GetGlobalMousePosition());
			}
		}

		if(isWatering && !isRunning){
			barnScript.waterSpot(new Vector2(Position.X, Position.Y + 8), delta);
			GD.Print(delta);
		}

		//Handles movement
		Vector2 axisPowers = Input.GetVector("left", "right", "up", "down");
		if(axisPowers == Vector2.Zero){
			//Friction
			Velocity = Velocity.Lerp(Vector2.Zero, friction);
			//Idle anim
			isRunning = false;
			if(!isWatering){
				animTree.Set("parameters/IdleRun/IdleSpace/blend_position", lastAxis);
				animTree.Set("parameters/TimeScale/scale", .75);
			}else{
				animTree.Set("parameters/IdleRun/WaterSpace/blend_position", lastAxis);
				animTree.Set("parameters/TimeScale/scale", 1.5);
			}
		}else{
			//Acceleration
			Velocity = Velocity.Lerp(axisPowers * maxSpeed, acceleration);
			//Move anim
			isRunning = true;
			animTree.Set("parameters/IdleRun/RunSpace/blend_position", axisPowers);
			animTree.Set("parameters/TimeScale/scale", 1.5);

			lastAxis = axisPowers;
		}
		MoveAndSlide();
	}
}
