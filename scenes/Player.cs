using Godot;
using System;

public partial class Player : CharacterBody2D{
	[Export]
	private AnimationTree animTree;
	[Export]
	private TileMapLayer seedsLayer;
	[Export]
	private Sprite2D inventSelect;
	[Export]
	private Label label;
	private int maxSpeed = 75;
	private float acceleration = .4f;
	private float friction = .2f;
	private bool isRunning;
	private bool isWatering;
	private Vector2 lastAxis = new Vector2(0, 1);
	private Barn barnScript;
	private bool isFarming = false;
	private string currentPlant = "turnip";
	private string previousPlant = "turnip";
	private Vector2I lastSeedPos = Vector2I.Zero;
	private Vector2 firstInventPos = new Vector2(350, 570);
	private int inventPos = 0;
	private string[] veggies = {"turnip", "tomato", "melon", "eggplant", "lemon", "wheat", "strawberry", "potato", "orange", "corn"};
	private int[] seeds = new int[10];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		barnScript = (Barn)GetParent();
		seeds[0] = 2;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(Input.IsActionJustPressed("water")){
			if(isWatering){
				isWatering = false;
			}else{
				isWatering = true;
				isFarming = false;
			}
		}

		//Handles Inventory
		if(isWatering){
			inventSelect.Position = firstInventPos;
			if(!inventSelect.Visible){
				inventSelect.Show();
			}
			label.Text = "water";
			label.PivotOffset = new Vector2(label.Size.X/2f, 0);
		}
		if(Input.IsActionJustReleased("scrollUp")){
			inventPos ++;
			if(inventPos > 9){
				inventPos = 0;
			}
			isFarming = true;
		}
		if(Input.IsActionJustReleased("scrollDown")){
			inventPos --;
			if(inventPos < 0){
				inventPos = 9;
			}
			isFarming = true;
		}
		checkKeyPressed(0);
		checkKeyPressed(1);
		checkKeyPressed(2);
		checkKeyPressed(3);
		checkKeyPressed(4);
		checkKeyPressed(5);
		checkKeyPressed(6);
		checkKeyPressed(7);
		checkKeyPressed(8);
		checkKeyPressed(9);
		if(isFarming){
			currentPlant = veggies[inventPos];
			inventSelect.Position = new Vector2(firstInventPos.X + 70 + (inventPos * 42), firstInventPos.Y);
			if(!inventSelect.Visible){
				inventSelect.Show();
				inventSelect.Position = firstInventPos;
			}
			label.Text = currentPlant + " seeds: " + seeds[inventPos];
			label.PivotOffset = new Vector2(label.Size.X/2f, 0);
			isWatering = false;
		}else if(!isWatering){
			inventSelect.Hide();
			label.Text = "";
		}

		//Handles planting
		if(isFarming && seeds[inventPos] > 0){
			Vector2 offset = -1 * (Position % 16);
			offset += new Vector2(16, 16);
			seedsLayer.Position = offset;
			Vector2 mousePos = seedsLayer.ToLocal(GetGlobalMousePosition());
			Vector2I coords = seedsLayer.LocalToMap(mousePos);
			if(coords != lastSeedPos || currentPlant != previousPlant){
				seedsLayer.EraseCell(lastSeedPos);
				seedsLayer.SetCell(coords, 0, barnScript.plantIdx[currentPlant]);
				lastSeedPos = coords;
				previousPlant = currentPlant;
				barnScript.showBonuses(currentPlant, GetGlobalMousePosition());
			}
			if(Input.IsActionJustPressed("leftClick")){
				barnScript.addSeed(currentPlant, GetGlobalMousePosition());
				seeds[inventPos] --;
			}
		}else if (isFarming){
			previousPlant = currentPlant;
			seedsLayer.Clear();
		}else{
			seedsLayer.Clear();
		}

		if(isWatering && !isRunning){
			barnScript.waterSpot(new Vector2(Position.X, Position.Y + 8), delta);
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
				animTree.Set("parameters/TimeScale/scale", .75);
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

	private void checkKeyPressed(int x){
		if(Input.IsActionJustPressed(x + "")){
			if(x == 0){
				x = 10;
			}
			if(inventPos != x - 1){
				inventPos = x - 1;
				isFarming = true;
			}else{
				isFarming = false;
				inventPos = x - 2;
				if(inventPos < 0){
					inventPos += 10;
				}
			}
		}
	}
}
