using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D{
	[Export]
	private AnimationTree animTree;
	[Export]
	private TileMapLayer seedsLayer;
	[Export]
	private Sprite2D inventSelect;
	[Export]
	private Label label;
	[Export]
	private Label moneyTxt;
	[Export]
	private Label decrypt;

	public int money = 50;

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
	private int inventPos = -1;
	private string[] veggies = {"turnip", "tomato", "melon", "eggplant", "lemon", "wheat", "strawberry", "potato", "orange", "corn"};
	private Dictionary<string, int> buyIdx = new Dictionary<string, int>(){
		{"turnip", 10},
		{"potato", 25},
		{"wheat", 40},
		{"strawberry", 60},
		{"tomato", 120},
		{"corn", 150},
		{"eggplant", 200},
		{"melon", 300},
		{"lemon", 500},
		{"orange", 550}
	};
	private Dictionary<string, int> sellIdx = new Dictionary<string, int>(){
		{"turnip", 20},
		{"potato", 55},
		{"wheat", 90},
		{"strawberry", 140},
		{"tomato", 300},
		{"corn", 375},
		{"eggplant", 550},
		{"melon", 900},
		{"lemon", 1500},
		{"orange", 1650}
	};
	private Dictionary<string, string> descriptionIdx = new Dictionary<string, string>(){
    {"turnip", "Fast-growing and hardy.\n• Acts as a 'trap crop' for tomato pests.\n• Creates space for strawberries to spread."},
    {"potato", "Deep-rooted tubers.\n• Loves the dappled shade provided by corn.\n• Benefits from nitrogen left behind by wheat."},
    {"wheat", "A gold-standard staple.\n• Thrives when planted near corn or turnips.\n• Neighbors help break up the soil for its roots."},
    {"strawberry", "A delicate ground-cover.\n• Flourishes in the partial shade of lemon trees.\n• Benefits from the pest-deterring scent of tomatoes."},
    {"tomato", "The garden's heavy-hitter.\n• Grows stronger with turnips nearby to lure away aphids.\n• Uses lemon trees to shelter from harsh winds."},
    {"corn", "A natural trellis.\n• Reaches high to provide shade for melons.\n• Works with potatoes to share deep soil nutrients."},
    {"eggplant", "A sun-lover.\n• Grows best near nightshade cousins like tomatoes and potatoes.\n• These plants share similar soil and water needs."},
    {"melon", "A thirsty vine.\n• Uses corn stalks for shade to prevent sun-scorch.\n• Thrives near citrus trees to keep the soil active."},
    {"lemon", "A vibrant citrus tree.\n• Canopy provides a micro-climate for strawberries.\n• Fends off ground-level heat for nearby tomatoes."},
    {"orange", "The king of the orchard.\n• Pairs well with eggplants and melons.\n• Creates a mutually beneficial moisture-retaining environment."}
};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		barnScript = (Barn)GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		//Handles switching to water
		if(Input.IsActionJustPressed("water")){
			if(isWatering){
				isWatering = false;
			}else{
				isWatering = true;
				isFarming = false;
			}
		}

		//Handles Inventory
		inventory();

		//Handles planting and watering
		plantWater(delta);

		//Handles movement
		move();

		moneyTxt.Text = "Money: $" + money;
		MoveAndSlide();
	}

	private void move(){
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
	}

	private void inventory(){
		if(isWatering){
			inventSelect.Position = firstInventPos;
			if(!inventSelect.Visible){
				inventSelect.Show();
			}
			label.Text = "\nwater";
			decrypt.Text = "";
			label.PivotOffset = new Vector2(label.Size.X/2f, 0);
			inventPos = -1;
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
			label.Text = currentPlant + "\nbuy $" + buyIdx[currentPlant] + " | sell $" + sellIdx[currentPlant];
			label.PivotOffset = new Vector2(label.Size.X/2f, 0);
			decrypt.Text = descriptionIdx[currentPlant];
			isWatering = false;
		}else if(!isWatering){
			inventSelect.Hide();
			label.Text = "";
			decrypt.Text = "";
		}
	}

	private void plantWater(double delta){
		if(isFarming){
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
			}
			previousPlant = currentPlant;
		}else{
			seedsLayer.Clear();
			barnScript.clearBonus();
			lastSeedPos = new Vector2I(-1, -1);
		}

		if(isWatering && !isRunning){
			barnScript.waterSpot(new Vector2(Position.X, Position.Y + 8), delta);
		}
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

	public void getMoney(string plant){
		//GD.Print(plant);
		money += sellIdx[plant];
	}
}
