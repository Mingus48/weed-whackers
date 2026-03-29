using Godot;
using System;
using System.Collections.Generic;

public partial class Barn : Node2D
{
	[Export]
	private CharacterBody2D player;
	[Export]
	private Timer tickTimer;
	[Export]
	private TileMapLayer tileMap;
	[Export]
	private TileMapLayer waterTiles;
	[Export]
	private TileMapLayer bonusMap;

	private string[,] plants = new string[8, 10];
	private float[,] water = new float[8, 10];
	private int[,] growTime = new int[8, 10];
	private float[,] bonus = new float[8, 10];
	private PackedScene fruit;
	//How long it will take for a fully watered plant to become dry * 100
	private int maxWater = 30000;
	//The atlas coords at where you can find the plant
	public Dictionary<string, Vector2I> plantIdx = new Dictionary<string, Vector2I>(){
		{"turnip", new Vector2I(5, 0)},
		{"tomato", new Vector2I(5, 2)}
	};
	//Growth time
	//The number is the time in seconds it takes to grow * 100
	private Dictionary<string, int> growIdx = new Dictionary<string, int>(){
		{"turnip", 1000},
		{"tomato", 2000}
	};
	//For the bonus system
	//Everything that BOOSTS(not everything that is boosted by) the key plant is listed
	//Everything that DIMINISHES(not everything that is diminished by) the plant is listed in the -plant key
	private Dictionary<string, List<string>> bonusIdx = new Dictionary<string, List<string>>(){
		{"turnip", new List<string>{"tomato"}},
		{"tomato", new List<string>{"idk"}},
		{"-turnip", new List<string>{""}},
		{"-tomato", new List<string>{"turnip"}}
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		fruit = GD.Load<PackedScene>("res://scenes/Collectable.tscn");
		Camera2D cam = player.GetNode<Camera2D>("Camera2D");
		cam.LimitLeft = 0;
		cam.LimitRight = 416;
		cam.LimitTop = 0;
		cam.LimitBottom = 272;
		bonusMap.Clear();
		for(int i = 0; i < bonus.GetLength(0); i ++){
			for(int j = 0; j < bonus.GetLength(1); j ++){
				bonus[i, j] = 1;
			}
		}
		addSeed("turnip", new Vector2I(15 * 16, 64));
		addSeed("tomato", new Vector2I(16 * 16, 64));
		addSeed("tomato", new Vector2I(16 * 16, 80));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		harvestPlant();
		/*Current watering script(will shift to player)
		Vector2 localPos = GetGlobalMousePosition() - tileMap.Position;
		localPos /= 16;
		if(localPos.X < 8 && localPos.Y < 10 && localPos.X > 0 && localPos.Y > 0){
			water[(int)localPos.X, (int)localPos.Y] = maxWater;
			updateWater();
		}*/
	}

	private void harvestPlant(){
		if(Input.IsMouseButtonPressed(MouseButton.Left)){
			Vector2I mousePos = tileMap.LocalToMap(tileMap.ToLocal(GetGlobalMousePosition()));
			if(mousePos.X >= 0 && mousePos.Y >= 0 && mousePos.X < plants.GetLength(0) && mousePos.Y < plants.GetLength(1)){
				if(plants[mousePos.X, mousePos.Y] != null && growTime[mousePos.X, mousePos.Y] == 0){
					string plant = plants[mousePos.X, mousePos.Y];
					plants[mousePos.X, mousePos.Y] = null;
					bonus[mousePos.X, mousePos.Y] = 1;
					if(mousePos.X - 1 >= 0){
						reverseBonus(mousePos.X - 1, mousePos.Y, plant);
					}
					if(mousePos.Y - 1 >= 0){
						reverseBonus(mousePos.X, mousePos.Y - 1, plant);
					}
					if(mousePos.X + 1 < plants.GetLength(0)){
						reverseBonus(mousePos.X + 1, mousePos.Y, plant);
					}
					if(mousePos.Y + 1 < plants.GetLength(1)){
						reverseBonus(mousePos.X , mousePos.Y + 1, plant);
					}
					tileMap.EraseCell(mousePos);
					spawnFruit(plant, GetLocalMousePosition());
				}
			}
		}
	}

	public void addSeed(string plant, Vector2 globalCords){
		Vector2I cords = tileMap.LocalToMap(tileMap.ToLocal(globalCords));
		if(cords.X < 0 || cords.X >= plants.GetLength(0) || cords.Y < 0 || cords.Y >= plants.GetLength(1)){
			return;
		}
		if(plants[cords.X, cords.Y] != null){
			return;
		}
		//Adds plant and grow times to the 2d array
		plants[cords.X, cords.Y] = plant;
		growTime[cords.X, cords.Y] = growIdx[plant];
		Vector2I atlasCords = plantIdx[plant];
		//Handles bonuses
		int boosted = 0;
		if(cords.X - 1 >= 0){
			boosted += plantBonuses(cords.X - 1, cords.Y, plant);
		}
		if(cords.Y - 1 >= 0){
			boosted += plantBonuses(cords.X, cords.Y - 1, plant);
		}
		if(cords.X + 1 < plants.GetLength(0)){
			boosted += plantBonuses(cords.X + 1, cords.Y, plant);
		}
		if(cords.Y + 1 < plants.GetLength(1)){
			boosted += plantBonuses(cords.X , cords.Y + 1, plant);
		}
		bonus[cords.X, cords.Y] += boosted / 4f;
		//Puts the tile on the map
		tileMap.SetCell(cords, 0, atlasCords);
	}

	public void waterSpot(Vector2 globalCords, double deltaForce){
		Vector2I cords = waterTiles.LocalToMap(waterTiles.ToLocal(globalCords));
		if(cords.X < 0 || cords.X >= plants.GetLength(0) || cords.Y < 0 || cords.Y >= plants.GetLength(1)){
			return;
		}
		water[cords.X, cords.Y] += maxWater/10;
		//Amount of seconds it takes to fill ^
		if(cords.X - 1 >= 0){
			water[cords.X - 1, cords.Y] += maxWater/10 * (float)deltaForce;
		}
		if(cords.Y - 1 >= 0){
			water[cords.X, cords.Y - 1] += maxWater/20 * (float)deltaForce;
		}
		if(cords.X + 1 < water.GetLength(0)){
			water[cords.X + 1, cords.Y] += maxWater/20 * (float)deltaForce;
		}
		if(cords.Y + 1 < water.GetLength(1)){
			water[cords.X, cords.Y + 1] += maxWater/20 * (float)deltaForce;
		}
		updateWater();
	}

	private int plantBonuses(int x, int y, string plant){
		if(plants[x, y] == null){
			return 0;
		}
		if(bonusIdx[plants[x, y]].Contains(plant)){
			bonus[x, y] += .25f;
		}else if(bonusIdx["-" + plants[x, y]].Contains(plant)){
			bonus[x, y] -= .25f;
		}
		if(bonusIdx[plant].Contains(plants[x, y])){
			return 1;
		}else if(bonusIdx["-" + plant].Contains(plants[x, y])){
			return -1;
		}
		return 0;
	}

	private void reverseBonus(int x, int y, string plant){
		if(plants[x, y] == null){
			return;
		}
		if(bonusIdx[plants[x, y]].Contains(plant)){
			bonus[x, y] -= .25f;
		}else if(bonusIdx["-" + plants[x, y]].Contains(plant)){
			bonus[x, y] += .25f;
		}
	}

	private void updateWater(){
		//Updates all farmland tiles to have the correct water
		for(int i = 0; i < water.GetLength(0); i ++){
			for(int j = 0; j < water.GetLength(1); j ++){
				if(water[i,j] != 0){
					if(water[i, j] > maxWater/2f){
						waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(8, 8));
					}else if(water[i, j] > 0){
						waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(7, 8));
					}else{
						waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(6, 8));
					}
				}
			}
		}
	}

	private void updatePlants(){
		//Updates all plant tiles to be in the correct state
		for(int i = 0; i < growTime.GetLength(0); i ++){
			for(int j = 0; j < growTime.GetLength(1); j ++){
				if(plants[i, j] != null){
					Vector2I atlasCords = plantIdx[plants[i, j]];
					atlasCords.X -= 4 - (int)(5 * ((growTime[i, j] - .001)/growIdx[plants[i, j]]));
					if(atlasCords.X == 1 || atlasCords.X == 7){
						growTime[i, j] = 0;
					}
					tileMap.SetCell(new Vector2I(i, j), 0, atlasCords);
				}
			}
		}
	}

	public void showBonuses(string plant, Vector2 globalCords){
		bonusMap.Clear();
		Vector2I cords = bonusMap.LocalToMap(bonusMap.ToLocal(globalCords));
		if(cords.X < 0 || cords.X >= plants.GetLength(0) || cords.Y < 0 || cords.Y >= plants.GetLength(1)){
			return;
		}
		if(plants[cords.X, cords.Y] != null){
			return;
		}
		int boosted = 0;
		if(cords.X - 1 >= 0){
			boosted += setBonusCell(cords.X - 1, cords.Y, plant);
		}
		if(cords.Y - 1 >= 0){
			boosted += setBonusCell(cords.X, cords.Y - 1, plant);
		}
		if(cords.X + 1 < plants.GetLength(0)){
			boosted += setBonusCell(cords.X + 1, cords.Y, plant);
		}
		if(cords.Y + 1 < plants.GetLength(1)){
			boosted += setBonusCell(cords.X , cords.Y + 1, plant);
		}
		if(boosted > 0){
			bonusMap.SetCell(cords, 0, new Vector2I(0, 0));
		}else if(boosted < 0){
			bonusMap.SetCell(cords, 0, new Vector2I(1, 0));
		}
	}

	private int setBonusCell(int x, int y, string plant){
		if(plants[x, y] == null){
			return 0;
		}
		if(bonusIdx[plants[x, y]].Contains(plant)){
			bonusMap.SetCell(new Vector2I(x, y), 0, new Vector2I(0, 0));
		}else if(bonusIdx["-" + plants[x, y]].Contains(plant)){
			bonusMap.SetCell(new Vector2I(x, y), 0, new Vector2I(1, 0));
		}
		if(bonusIdx[plant].Contains(plants[x, y])){
			return 1;
		}else if(bonusIdx["-" + plant].Contains(plants[x, y])){
			return -1;
		}
		return 0;
	}

	private void spawnFruit(string plant, Vector2 pos){
		Collectable veggie = (Collectable)fruit.Instantiate<RigidBody2D>();
		AddChild(veggie);
		veggie.Position = pos;
		veggie.setTile(plant);
	}

	private void _on_tick_timer_timeout(){
		for(int i = 0; i < water.GetLength(0); i ++){
			for(int j = 0; j < water.GetLength(1); j ++){
				if(water[i, j] != 0){
					water[i, j] -= (int)tickTimer.WaitTime * 100;
					if(water[i, j] < 0){
						water[i, j] = 0;
					}
				}
			}
		}

		for(int i = 0; i < growTime.GetLength(0); i ++){
			for(int j = 0; j < growTime.GetLength(1); j ++){
				if(growTime[i, j] != 0 && water[i, j] != 0){
					growTime[i, j] -= (int)(tickTimer.WaitTime * 100 * bonus[i, j]);
					if(growTime[i, j] < 0){
						growTime[i, j] = 0;
					}
				}
			}
		}
		updateWater();
		updatePlants();
	}
}
