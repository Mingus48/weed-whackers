using Godot;
using System;
using System.Collections.Generic;

public partial class NPC : CharacterBody2D
{
	[Export] private AnimatedSprite2D animSprite;
	[Export] private Vector2 homePosition = Vector2.Zero;
	[Export] private Vector2 outsidePosition = new Vector2(0, 40);
	[Export] private Control dialogueBox;
	[Export] private Label dialogueLabel;
	[Export] private VBoxContainer optionsContainer;
	[Export] private Button option1Button;
	[Export] private Button option2Button;
	[Export] private Button option3Button;

	private int moveSpeed = 40;
	private bool isOutside = false;
	private bool isMoving = false;
	private bool shopOpen = false;
	private bool playerNearby = false;
	private CharacterBody2D player;

	private Action _cb1, _cb2, _cb3;

	private Dictionary<string, int> seedPrices = new Dictionary<string, int>()
	{
		{ "turnip", 5 },
		{ "tomato", 10 }
	};

	private Dictionary<string, int> cropValues = new Dictionary<string, int>()
	{
		{ "turnip", 8 },
		{ "tomato", 15 }
	};

	private int playerCoins = 100;
	private Dictionary<string, int> playerCrops = new Dictionary<string, int>()
	{
		{ "turnip", 0 },
		{ "tomato", 0 }
	};

	public override void _Ready()
	{
		Position = homePosition;
		HideDialogue();

		// Hook up button callbacks once
		if (option1Button != null) option1Button.Pressed += () => _cb1?.Invoke();
		if (option2Button != null) option2Button.Pressed += () => _cb2?.Invoke();
		if (option3Button != null) option3Button.Pressed += () => _cb3?.Invoke();
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
					OpenGreeting();
			}
			else
			{
				Velocity = direction * moveSpeed;
				MoveAndSlide();

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

		if (playerNearby && Input.IsActionJustPressed("ui_accept"))
		{
			if (!isOutside && !isMoving)
				CallNPCOutside();
			else if (isOutside && !shopOpen && !isMoving)
				OpenGreeting();
		}

		if (shopOpen && Input.IsActionJustPressed("ui_cancel"))
			CloseShop();
	}

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

	private void OpenGreeting()
	{
		shopOpen = true;
		ShowDialogue("Howdy! Welcome to my shop.\nWhat can I do for ya?",
			"Buy Seeds", OnBuySelected,
			"Sell Crops", OnSellSelected,
			"Nothing, thanks", OnFarewellSelected);
	}

	private void OnBuySelected()
	{
		string msg = $"Which seed would you like?\n(You have {playerCoins} coins)\n";
		foreach (var kv in seedPrices)
			msg += $"  {kv.Key}: {kv.Value} coins\n";
		ShowDialogue(msg,
			"Buy Turnip (5c)", () => BuySeed("turnip"),
			"Buy Tomato (10c)", () => BuySeed("tomato"),
			"Go back", OpenGreeting);
	}

	private void OnSellSelected()
	{
		string msg = $"I'll buy your crops!\n(You have {playerCoins} coins)\n";
		foreach (var kv in playerCrops)
			msg += $"  {kv.Key}: {kv.Value} in bag (worth {cropValues[kv.Key]}c each)\n";
		ShowDialogue(msg,
			"Sell Turnips", () => SellCrop("turnip"),
			"Sell Tomatoes", () => SellCrop("tomato"),
			"Go back", OpenGreeting);
	}

	private void OnFarewellSelected()
	{
		ShowDialogue("Come back anytime!",
			"Goodbye!", CloseShop,
			"", null,
			"", null);
	}

	private void BuySeed(string plant)
	{
		int price = seedPrices[plant];
		if (playerCoins >= price)
		{
			playerCoins -= price;
			GD.Print($"Bought {plant} seed! Coins left: {playerCoins}");
			ShowDialogue($"Here's your {plant} seed!\nCoins left: {playerCoins}",
				"Buy more", OnBuySelected,
				"Done", OpenGreeting,
				"", null);
		}
		else
		{
			ShowDialogue("You don't have enough coins!",
				"Go back", OnBuySelected,
				"", null,
				"", null);
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
			ShowDialogue($"Sold {amount} {plant} for {earned} coins!\nTotal: {playerCoins}",
				"Sell more", OnSellSelected,
				"Done", OpenGreeting,
				"", null);
		}
		else
		{
			ShowDialogue($"You don't have any {plant} to sell!",
				"Go back", OnSellSelected,
				"", null,
				"", null);
		}
	}

	private void CloseShop()
	{
		shopOpen = false;
		HideDialogue();
		ReturnHome();
	}

	private void ShowDialogue(string text,
		string lbl1, Action cb1,
		string lbl2, Action cb2,
		string lbl3, Action cb3)
	{
		if (dialogueBox == null) { GD.Print("[NPC] " + text); return; }
		dialogueBox.Visible = true;
		dialogueLabel.Text = text;

		_cb1 = cb1;
		_cb2 = cb2;
		_cb3 = cb3;

		SetButtonLabel(option1Button, lbl1);
		SetButtonLabel(option2Button, lbl2);
		SetButtonLabel(option3Button, lbl3);
	}

	private void SetButtonLabel(Button btn, string label)
	{
		if (btn == null) return;
		btn.Visible = !string.IsNullOrEmpty(label);
		btn.Text = label;
	}

	private void HideDialogue()
	{
		if (dialogueBox != null)
			dialogueBox.Visible = false;
	}

	private void _on_area_2d_body_entered(Node2D body)
	{
		if (body.Name == "Player")
		{
			playerNearby = true;
			player = (CharacterBody2D)body;
			GD.Print("Player nearby! Press E to interact.");
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

	public void AddCropToPlayerBag(string plant, int amount)
	{
		if (playerCrops.ContainsKey(plant))
			playerCrops[plant] += amount;
	}

	public int GetPlayerCoins() => playerCoins;
}
