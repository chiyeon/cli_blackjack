using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackJack {
    class BlackJack {
        public static void Main(string[] args) {
            string playerName = "";
            int numAdditionalPlayers = 0;
            bool validNumPlayers = false;
            int numGames = 0;
            int winningGames = 0;

            Console.WriteLine("====================================");
            Console.WriteLine("   Welcome to BlackJack!");
            Console.WriteLine("      created by Benjamin Wong");
            Console.WriteLine("====================================");
            Console.WriteLine("");

            string input = "";
            do {
                Console.Write("Would you like to Play (y/n)? ");
                try {
                    input = Console.ReadLine().ToLower();
                    if(input == "n") {
                        Console.WriteLine("Goodbye!");
                        return;
                    }
                } catch(Exception e) {
                    Console.WriteLine("Error: {0} Please try again!", e.Message);
                }
            } while(input != "y");

            do {
                Console.Write("What is your name? ");
                playerName = Console.ReadLine();

                if(playerName == "")
                    Console.WriteLine("Please input a valid player name!\n");
            } while(playerName == "");

            bool gameOver = false;
            do {
                numGames++;

                numAdditionalPlayers = 0;
                validNumPlayers = false;

                Console.WriteLine("\n=====================================");
                Console.WriteLine("   === GAME {0} ===", numGames);
                Console.WriteLine("=====================================");

                Console.WriteLine("\nWelcome, {0}!\n", playerName);
                Console.WriteLine("The game defaults to TWO players, You and the House.");

                do {
                    Console.Write("How many additional AI players do you want? ");
                    try {
                        numAdditionalPlayers = Convert.ToInt32(Console.ReadLine());
                    } catch (Exception e) {
                        /* generic error catching 
                        * Format & Overflow */
                        Console.WriteLine("Error: {0} Please try again!\n", e.Message);
                        continue;
                    }

                    if(numAdditionalPlayers < 0)
                        Console.WriteLine("Please input a number 0 or more!");
                    else if(numAdditionalPlayers > 5)
                        Console.WriteLine("Maximum of 5 additional players allowed!\n");
                    else
                        validNumPlayers = true;
                } while (!validNumPlayers);

                Console.WriteLine("Starting a game with {0} additional players...\n", numAdditionalPlayers);

                if(new Game(playerName, numAdditionalPlayers).Play()) {
                    winningGames++;
                }
                do {
                    Console.Write("\nWould you like to play another (y/n)? ");
                    input = Console.ReadLine().ToLower();
                    if(input == "n") {
                        gameOver = true;
                        break;
                    } else if (input != "y") {
                        Console.WriteLine("Invalid response. Please input (y/n)!");
                    }
                } while(input != "y");
            } while(!gameOver);

            Console.WriteLine("\nToday {0}, you won {1} out of {2} games.", playerName, winningGames, numGames);

            int winRate = (int)(((float)winningGames / (float)numGames) * 100.0);

            Console.Write("Thats a {0}% win rate. ", winRate);

            if(winRate >= 90) {
                Console.WriteLine("You are Incredible!!!");
            } else if(winRate >= 50) {
                Console.WriteLine("Nice!");
            } else if(winRate >= 25) {
                Console.WriteLine("Better Luck Next Time!");
            } else if(winRate >= 10) {
                Console.WriteLine("We Can't All Be Winners.");
            } else {
                Console.WriteLine("May you find better luck elsewhere.");
            }

            Console.WriteLine("Goodbye!");
        }
    }

    class Card {
        public enum Value { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
        public enum Suit { Spades, Diamonds, Clubs, Hearts }

        public Value value { get; private set; }
        public Suit suit { get; private set; }
        public bool faceUp { get; private set; }

        public Card(Value value, Suit suit, bool faceUp = true) {
            this.value = value;
            this.suit = suit;
            this.faceUp = faceUp;
        }

        public string GetName(bool ignoreFacingDirection = false) {
            return faceUp || ignoreFacingDirection ? value.ToString() + " of " + suit.ToString() : "Face down Card";
        }

        public int GetGameValue() {
            if (value == Value.Ace) {
                return 11;
            } else {
                return Math.Clamp((int)value + 1, 1, 10);
            }
        }

        public void Flip() {
            faceUp = !faceUp;
        }

        public void SetFaceUp(bool faceUp) {
            this.faceUp = faceUp;
        }
    }

    class Deck {
        // deck of cards, where 0 is the top of the deck
        public List<Card> cards = new List<Card>();
        
        private Random rand = new Random();

        public Deck(bool faceUp = false, bool shuffle = true) {
            // generate standard deck in order
            for(int i = 0; i < 4; i++) {
                for(int j = 0; j < 13; j++) {
                    //cards[i * 13 + j] = new Card((Card.Value)j, (Card.Suit)i);
                    cards.Add(new Card((Card.Value)j, (Card.Suit)i, faceUp));
                }
            }
            
            if(shuffle) Shuffle();
        }

        public void Shuffle() {
            // for each card
            for(int i = 0; i < 52; i++) {
                // find a random target card that isn't the same
                int target = i;
                do {
                    target = rand.Next(52);
                } while (target == i);
                
                // switch the cards
                Card temp = cards[i];
                cards[i] = cards[target];
                cards[target] = temp;
            }
        }

        public void PrintAllCards() {
            foreach(Card card in cards) {
                Console.WriteLine(card.GetName() + ": " + card.GetGameValue());
            }
        }

        // removes and returns card from top of deck
        public Card GetTopCard() {
            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        // removes and returns a number of cards from top of deck
        public Card[] GetTopCards(int amount) {
            Card[] c = new Card[amount];
            for(int i = 0; i < amount; i++) {
                c[i] = cards[0];
                cards.RemoveAt(0);
            }
            return c;
        }
    }

    class Player {
        public string name { get; protected set; }
        public List<Card> hand = new List<Card>();
        public bool isBusted = false;

        public Player() {
            name = "Uninitialized Player";
        }

        public Player(string name, Card[] startingHand) {
            this.name = name;
            foreach(Card c in startingHand)
                hand.Add(c);
        }

        public void GiveCard(Card card, bool isPlayer) {
            string name = this.name;
            if(isPlayer) name = "You";
            
            Console.WriteLine("\t{0} decided to hit!", name);

            hand.Add(card);

            Console.WriteLine("\t{0} received the card: {1}", name, card.GetName());
            Console.WriteLine("\tVisible Hand Value: {0}\n", GetHandValue());

            if(GetHandValue(true) > 21) {
                Console.WriteLine("\t{0} busted!", name);
                Bust();
            }
        }

        public void PrintHand() {
            foreach(Card card in hand) {
                Console.WriteLine(card.GetName());
            }
        }

        public string GetHand(bool ignoreFacingDirection = false) {
            string s = "";
            foreach(Card card in hand) {
                s += card.GetName(ignoreFacingDirection);
                if (card != hand[hand.Count - 1]) s += ", ";
            }

            return s;
        }

        public string GetHandStatus(bool ignoreFacingDirection = false) {
            int handValue = GetHandValue(ignoreFacingDirection);
            return name + "'s hand: " + GetHand(ignoreFacingDirection) + " (Visible Value: " + (handValue == 0 ? "Unknown" : handValue.ToString()) + ")\n";
        }

        public int GetHandValue(bool ignoreFacingDirection = false) {
            int value = 0;

            foreach(Card c in hand) {
                if(c.faceUp || ignoreFacingDirection) {
                    value += c.GetGameValue();
                }
            }

            // todo : encapuslate in own function?
            bool hasAce = false;
            foreach(Card c in hand) {
                if(c.value == Card.Value.Ace) 
                    hasAce = true;
            }

            if(value > 21 && hasAce)
                value -= 10;

            return value;
        }

        public void Bust() {
            isBusted = true;
        }
    }

    class House : Player {
        public House(Card[] startingHand) {
            // following assumes startingHand has 2 cards
            startingHand[0].SetFaceUp(false);
            startingHand[1].SetFaceUp(true);

            name = "The House";
            foreach(Card c in startingHand)
                hand.Add(c);
        }
    }

    class AIPlayer : Player {
        public string[] names = {
            "Izzy",
            "Miles",
            "Milan",
            "Justin",
            "Warat",
            "Daniel",
            "Kai",
            "Shri",
            "Professor Allison",
            "Gabe Newell",
            "Donald Bren"
        };
        public Random rand = new Random();

        public AIPlayer(Card[] startingHand) {
            //foreach(Card c in startingHand)
            //    c.SetFaceUp(false);
            
            name = names[rand.Next(names.Length)];
            foreach(Card c in startingHand)
                hand.Add(c);
        }
    }

    class Game {
        private List<Player> players = new List<Player>();
        private Deck deck;

        public Game(string playerName, int numAdditionalPlayers) {
            deck = new Deck(true);
            players.Add(new Player(playerName, deck.GetTopCards(2)));
            for(int i = 0; i < numAdditionalPlayers; i++)
                players.Add(new AIPlayer(deck.GetTopCards(2)));
            players.Add(new House(deck.GetTopCards(2)));
        }

        public bool Play() {
            //Console.WriteLine("The deck has {0} cards left.", deck.cards.Count);
            //Console.WriteLine("There are {0} players left.\n", GetNumberOfActivePlayers());

            Console.WriteLine("--- House's Hand ---");
            Console.WriteLine("\n\t{0}", players[players.Count - 1].GetHandStatus());

            foreach(Player p in players) {
                if(p.isBusted)
                    continue;

                if (p.GetType() == typeof(Player)) {
                    Console.WriteLine("--- It is your turn. ---\n");

                    Console.WriteLine("\t{0}", p.GetHandStatus());

                    string input = "";
                    do {
                        Console.Write("\tWould you like to hit (y/n)? ");
                        input = Console.ReadLine().ToLower();

                        if(input == "y") {
                            p.GiveCard(deck.GetTopCard(), true);
                            if(p.isBusted) {
                                do {
                                    Console.Write("\nWould you like to spectate the rest of this game (y/n)? ");
                                    input = Console.ReadLine().ToLower();
                                    if(input == "n") {
                                        return false;
                                    }
                                } while(input != "y");
                                break;
                            }
                        } else {
                            Console.WriteLine("\tYou decided to stay.");
                        }
                    } while(input != "n");
                } else {
                    bool botHitting = true;
                    Console.WriteLine("\n--- It is {0}'s turn. ---\n", p.name);
                    Console.WriteLine("\t{0}", p.GetHandStatus());
                    do {
                        Console.WriteLine("\t{0} is thinking...", p.name);

                        if(p.GetHandValue(true) > 15) {
                            Console.WriteLine("\t{0} decided to stay.", p.name);
                            botHitting = false;
                        } else {
                            p.GiveCard(deck.GetTopCard(), false);
                            if(p.isBusted) {
                                botHitting = false;
                                if(p.GetType() == typeof(House) || GetNumberOfActivePlayers() == 1) {
                                    break;
                                }
                            }
                        }
                    } while (botHitting);

                    // wait for user input then delete prompt when done
                    Console.Write("\nEnter to Continue.");
                    string input = Console.ReadLine();
                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                    Console.Write(new string(' ', Console.WindowWidth));
                }
            }

            Console.WriteLine("\n=====================================");
            Console.WriteLine("   === RESULTS ===");
            Console.WriteLine("=====================================\n");

            int houseValue = players[players.Count - 1].GetHandValue(true);
            Console.WriteLine("The House's hand was valued at {0}!\n", houseValue);

            int count = 0;

            var winners = 
                from p in players
                where !p.isBusted && ((p.GetHandValue(true) > houseValue) || (houseValue > 21 && p.name != "The House"))
                select p;
            
            Console.WriteLine("=== Winners ===");
            count = 0;
            if(winners.Count() == 0) {
                Console.Write("None\n");
            } else {
                foreach(Player p in winners) {
                    count++;
                    Console.Write("{0} (Hand: {1})", p.name, p.GetHandValue(true));
                    //if(count != winners.Count())
                        Console.Write("\n");
                }
            }

            var bustedPlayers = 
                from p in players
                where p.isBusted == true
                select p;

            Console.WriteLine("\n=== Busted Players ===");
            count = 0;
            if(bustedPlayers.Count() == 0) {
                Console.Write("None\n");
            } else {
                foreach(Player p in bustedPlayers) {
                    count++;
                    Console.Write("{0} (Hand: {1})", p.name, p.GetHandValue(true));
                    //if(count != bustedPlayers.Count())
                        Console.Write("\n");
                }
            }

            var everyoneElse =
                from p in players
                where !p.isBusted && p.GetHandValue(true) <= houseValue && p.name != "The House" && houseValue <= 21
                select p;
            
            Console.WriteLine("\n=== Everyone Else ===");
            count = 0;
            if(everyoneElse.Count() == 0) {
                Console.Write("None\n");
            } else {
                foreach(Player p in everyoneElse) {
                    count++;
                    Console.Write("{0} (Hand: {1})", p.name, p.GetHandValue(true));
                    //if(count != everyoneElse.Count())
                        Console.Write("\n");
                }
            }

            if(winners.Contains(players[0])) {
                return true;
            } else {
                return false;
            }
        }

        // returns players who haven't busted yet
        public int GetNumberOfActivePlayers() {
            var activePlayers = 
                from p in players
                where !p.isBusted
                select p;
            return activePlayers.Count();
        }
    }
}