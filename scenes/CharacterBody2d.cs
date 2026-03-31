using Godot;
using System;
using System.Collections.Generic;

public partial class NPC : CharacterBody2D
{
	[Export] private AnimatedSprite2D animSprite;

	// The position inside the house (where NPC starts/hides)
	[Export] private Vector2 homePosition = Vector2.Zero;
	// The position outside the house (where NPC walks to when called)
	[Export] private Vector2 outsidePosition = new Vector2(0, 40);

	private int moveSpeed = 40;
	private bool isOutside = false;
	private bool isMoving = false;
	private bool shopOpen = false;
	private bool playerNearby = false;
	private CharacterBody2D player;

	// Shop inventory: plant name -> price to buy seed
	private Dictionary<string, int> seedPrices = new Dictionary<string, int>()
	{
		{ "turnip", 5 },
		{ "tomato", 10 }
	};

	// How much the NPC pays for harvested crops
	private Dictionary<string, int> cropValues = new Dictionary<string, int>()
	{
		{ "turnip", 8 },
		{ "tomato", 15 }
	};

	// Player's inventory (coins and crops) - you can hook this into your Player.cs later
	private int playerCoins = 100;
	private Dictionary<string, int> playerCrops = new Dictionary<string, int>()
	{
		{ "turnip", 0 },
		{ "tomato", 0 }
	};

	// Dialogue state
	private enum ShopState { None, Greeting, MainMenu, Buying, Selling, Farewell }
	private ShopState currentState = ShopState.None;

	// UI nodes (add these to NPC.tscn)
	[Export] private Control dialogueBox;
	[Export] private Label dialogueLabel;
	[Export] private VBoxContainer optionsContainer;
	[Export] private Button option1Button;
	[Export] private Button option2Button;
	[Export] private Button option3Button;
	[Export] private Button closeButton;

	public override void _Ready()
	{
		Position = homePosition;
		HideDialogue();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isMoving)
		{
			Vector2 target = isOutside ? outsidePosition : homePosition;
			Vector2 direction = (target - Position).Normalized();
			float dist = Position.DistanceTo(target);

			if (dist < 2f)
			{
				Position = target;
				isMoving = false;
				if (isOutside)
				{
					// NPC arrived outside - open shop
					OpenGreeting();
				}
			}
			else
			{
				Velocity = direction * moveSpeed;
				MoveAndSlide();

				// Animate walk direction
				if (animSprite != null)
				{
					if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
						animSprite.Play(direction.X > 0 ? "walkRight" : "walkLeft");
					else
						animSprite.Play(direction.Y > 0 ? "walkDown" : "walkUp");
				}
			}
		}
		else if (animSprite != null && !shopOpen)
		{
			animSprite.Play("idle");
		}

		// Check for interaction input
		if (playerNearby && Input.IsActionJustPressed("ui_accept"))
		{
			if (!isOutside && !isMoving)
			{
				CallNPCOutside();
			}
			else if (isOutside && !shopOpen && !isMoving)
			{
				OpenGreeting();
			}
		}

		// Close shop with Escape
		if (shopOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			CloseShop();
		}
	}

	// Called by the House node when player interacts with it
	public void CallNPCOutside()
	{
		if (isMoving || isOutside) return;
		isOutside = true;
		isMoving = true;
		GD.Print("NPC is coming outside!");
	}

	private void ReturnHome()
	{
		isOutside = false;
		isMoving = true;
		shopOpen = false;
		HideDialogue();
	}

	// ---- Dialogue / Shop ----

	private void OpenGreeting()
	{
		shopOpen = true;
		currentState = ShopState.Greeting;
		ShowDialogue(
			"Howdy! Welcome to my shop.\nWhat can I do for ya?",
			("Buy Seeds", OnBuySelected),
			("Sell Crops", OnSellSelected),
			("Nothing, thanks", OnFarewellSelected)
		);
	}

	private void OnBuySelected()
	{
		currentState = ShopState.Buying;
		string msg = $"Which seed would you like?\n(You have {playerCoins} coins)\n";
		foreach (var kv in seedPrices)
			msg += $"  {kv.Key}: {kv.Value} coins\n";

		ShowDialogue(msg,
			("Buy Turnip seed (5c)", () => BuySeed("turnip")),
			("Buy Tomato seed (10c)", () => BuySeed("tomato")),
			("Go back", OpenGreeting)
		);
	}

	private void OnSellSelected()
	{
		currentState = ShopState.Selling;
		string msg = $"I'll buy your crops!\n(You have {playerCoins} coins)\n";
		foreach (var kv in playerCrops)
			msg += $"  {kv.Key}: {kv.Value} in bag (worth {cropValues[kv.Key]}c each)\n";

		ShowDialogue(msg,
			("Sell all Turnips", () => SellCrop("turnip")),
			("Sell all Tomatoes", () => SellCrop("tomato")),
			("Go back", OpenGreeting)
		);
	}

	private void OnFarewellSelected()
	{
		currentState = ShopState.Farewell;
		ShowDialogue("Come back anytime! 👋",
			("Goodbye!", CloseShop),
			null,
			null
		);
	}

	private void BuySeed(string plant)
	{
		int price = seedPrices[plant];
		if (playerCoins >= price)
		{
			playerCoins -= price;
			// TODO: add seed to player inventory / call barnScript.addSeed()
			GD.Print($"Bought {plant} seed! Coins left: {playerCoins}");
			ShowDialogue($"Here's your {plant} seed!\nCoins left: {playerCoins}",
				("Buy more", OnBuySelected),
				("Done", OpenGreeting),
				null
			);
		}
		else
		{
			ShowDialogue("You don't have enough coins!",
				("Go back", OnBuySelected),
				null,
				null
			);
		}
	}

	private void SellCrop(string plant)
	{
		int amount = playerCrops[plant];
		if (amount > 0)
		{
			int earned = amount * cropValues[plant];
			playerCoins += earned;
			playerCrops[plant] = 0;
			GD.Print($"Sold {amount} {plant} for {earned} coins! Total: {playerCoins}");
			ShowDialogue($"Sold {amount} {plant} for {earned} coins!\nTotal coins: {playerCoins}",
				("Sell more", OnSellSelected),
				("Done", OpenGreeting),
				null
			);
		}
		else
		{
			ShowDialogue($"You don't have any {plant} to sell!",
				("Go back", OnSellSelected),
				null,
				null
			);
		}
	}

	private void CloseShop()
	{
		shopOpen = false;
		HideDialogue();
		ReturnHome();
	}

	// ---- UI Helpers ----

	private void ShowDialogue(string text,
		(string label, Action callback)? opt1,
		(string label, Action callback)? opt2,
		(string label, Action callback)? opt3)
	{
		if (dialogueBox == null) { GD.Print("[NPC Dialogue] " + text); return; }
		dialogueBox.Visible = true;
		dialogueLabel.Text = text;

		SetupButton(option1Button, opt1);
		SetupButton(option2Button, opt2);
		SetupButton(option3Button, opt3);
	}

	private void SetupButton(Button btn, (string label, Action callback)? opt)
	{
		if (btn == null) return;
		if (opt == null)
		{
			btn.Visible = false;
			return;
		}
		btn.Visible = true;
		btn.Text = opt.Value.label;
		// Disconnect all previous signals before reconnecting
		btn.Pressed -= opt.Value.callback; // safe even if not connected
		btn.Pressed += opt.Value.callback;
	}

	private void HideDialogue()
	{
		if (dialogueBox != null)
			dialogueBox.Visible = false;
	}

	// ---- Area detection (connect Area2D body_entered/exited signals to these) ----

	private void _on_area_2d_body_entered(Node2D body)
	{
		if (body.Name == "Player")
		{
			playerNearby = true;
			player = (CharacterBody2D)body;
			GD.Print("Player is near the NPC/house. Press [E] to interact.");
		}
	}

	private void _on_area_2d_body_exited(Node2D body)
	{
		if (body.Name == "Player")
		{
			playerNearby = false;
			if (shopOpen) CloseShop();
		}
	}

	// Call this from outside (e.g. Player.cs) to give the NPC a crop after harvest
	public void AddCropToPlayerBag(string plant, int amount)
	{
		if (playerCrops.ContainsKey(plant))
			playerCrops[plant] += amount;
	}

	// Call this to get current coin count
	public int GetPlayerCoins() => playerCoins;
}
