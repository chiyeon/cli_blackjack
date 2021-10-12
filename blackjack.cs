/* blackjack.cs
 * by Benjamin Wong
 * October 11, 2021
 * GDIM31
 *
 * a simple cli blackjack in c#
 * allows player to play against a House and up to 5 computer-controller players
 * outputs to a local flie
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace BlackJack {

    /* record class
     * has static functions similar to those in the Console
     * class like WriteLine, ReadLine...
     * does as they do but also writes to a file
     */
    class Record {
        /* choosing to store to a local directory instead of
         * global one, something like
         * string path = “c:\\blackjack_record.txt”;
         */
        public static string path = "./blackjack_record.txt";

        public static void WriteLine(string message, params object[] args) {
            Record.Write(message + "\n", args);
        }

        public static void Write(string message, params object[] args) {
            
            Console.Write(message, args);

            try {
                using(StreamWriter sw = new StreamWriter(Record.path, true)) {
                    // constantly opening/closing sw. performance issues?
                    // the only fix would be to not ues static classes
                    // create one stream writer in the constructor...
                    // close it at the end...
                    sw.Write(message, args);
                    sw.Close();
                }
            } catch(Exception e) {
                Console.WriteLine("Stream Writer Error! {0}", e.Message);
            }
        }

        public static string ReadLine() {
            string input = Console.ReadLine();
            
            try {
                using(StreamWriter sw = new StreamWriter(Record.path, true)) {
                    // ReadLine skips the newline in the message
                    // append it here !
                    sw.Write(input + "\n");
                    sw.Close();
                }
            } catch(Exception e) {
                Console.WriteLine("Stream Writer Error! {0}", e.Message);
            }

            return input;
        }
    }

    /* 
     * contains main method and main game loop
     */
    class BlackJack {
        public static void Main(string[] args) {

            string playerName = "";
            int numGames = 0;
            int winningGames = 0;

            Record.WriteLine("############################################");
            Record.WriteLine("#                                          #");
            Record.WriteLine("#       Welcome to BlackJack!              #");
            Record.WriteLine("#           created by Benjamin Wong       #");
            Record.WriteLine("#                                          #");
            Record.WriteLine("############################################");
            Record.WriteLine("");

            // request to start game loop
            string input = "";
            do {
                Record.Write("Would you like to Play (y/n)? ");
                try {
                    input = Record.ReadLine().ToLower();
                    if(input == "n") {
                        Record.WriteLine("Goodbye!");
                        return;
                    } else if (input != "y") {
                        Record.WriteLine("Please input a valid response (y/n)!\n");
                    }
                } catch(Exception e) {
                    Record.WriteLine("Error: {0} Please try again!", e.Message);
                }
            } while(input != "y");

            // take player name
            do {
                Record.Write("What is your name? ");
                playerName = Record.ReadLine();

                if(playerName == "")
                    Record.WriteLine("Please input a valid player name!\n");
            } while(playerName == "");

            // main game loop
            bool gameOver = false;
            do {
                numGames++;

                int numAdditionalPlayers = 0;
                bool validNumPlayers = false;

                Record.WriteLine("\n=====================================");
                Record.WriteLine("   === GAME {0} ===", numGames);
                Record.WriteLine("=====================================");

                Record.WriteLine("\nWelcome, {0}!\n", playerName);
                Record.WriteLine("The game defaults to TWO players, You and the House.");

                do {
                    Record.Write("How many additional AI players do you want? ");
                    try {
                        numAdditionalPlayers = Convert.ToInt32(Record.ReadLine());
                    } catch (Exception e) {
                        /* generic error catching 
                        * Format & Overflow */
                        Record.WriteLine("Error: {0} Please try again!\n", e.Message);
                        continue;
                    }

                    if(numAdditionalPlayers < 0)
                        Record.WriteLine("Please input a number 0 or more!");
                    else if(numAdditionalPlayers > 5)
                        Record.WriteLine("Maximum of 5 additional players allowed!\n");
                    else
                        validNumPlayers = true;
                } while (!validNumPlayers);

                Record.WriteLine("Starting a game with {0} additional players...\n", numAdditionalPlayers);

                // all game logic is here, in the Game class instance
                // returns true/false depending on whether or not player won the game
                if(new Game(playerName, numAdditionalPlayers).Play()) {
                    winningGames++;
                }
                do {
                    Record.Write("\nWould you like to play another (y/n)? ");
                    input = Record.ReadLine().ToLower();
                    if(input == "n") {
                        gameOver = true;
                        break;
                    } else if (input != "y") {
                        Record.WriteLine("Invalid response. Please input (y/n)!");
                    }
                } while(input != "y");
            } while(!gameOver);

            Record.WriteLine("\nToday {0}, you won {1} out of {2} games.", playerName, winningGames, numGames);

            int winRate = (int)(((float)winningGames / (float)numGames) * 100.0);

            Record.Write("Thats a {0}% win rate. ", winRate);

            if(winRate >= 90) {
                Record.WriteLine("You are Incredible!!!");
            } else if(winRate >= 50) {
                Record.WriteLine("Nice!");
            } else if(winRate >= 25) {
                Record.WriteLine("Better Luck Next Time!");
            } else if(winRate >= 10) {
                Record.WriteLine("We Can't All Be Winners.");
            } else {
                Record.WriteLine("May you find better luck elsewhere.");
            }

            Record.Write("Goodbye! (Enter to Quit)");
            Console.ReadLine();
            // wait for user to quit
        }
    }

    class Card {
        // i wanted to use bit shifting n stuff for data hmm
        // but oop is probably cleaner
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
                // Math.Clamp requires .NET Core 2.0
                // not on windows by default
                // return Math.Clamp((int)value + 1, 1, 10);
                return (int)value + 1 > 10 ? 10 : (int)value + 1;
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
                Record.WriteLine(card.GetName() + ": " + card.GetGameValue());
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
            
            Record.WriteLine("\t{0} decided to hit!", name);

            hand.Add(card);

            Record.WriteLine("\t{0} received the card: {1}", name, card.GetName());
            Record.WriteLine("\tVisible Hand Value: {0}\n", GetHandValue());

            if(GetHandValue(true) > 21) {
                Record.WriteLine("\t{0} busted!", name);
                Bust();
            }
        }

        public void PrintHand() {
            foreach(Card card in hand) {
                Record.WriteLine(card.GetName());
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
            foreach(Card c in startingHand)
                hand.Add(c);

            name = "The House";
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

        public AIPlayer(Card[] startingHand) {
            //foreach(Card c in startingHand)
            //    c.SetFaceUp(false);

            // following assumes startingHand has 2 cards
            startingHand[0].SetFaceUp(false);
            startingHand[1].SetFaceUp(true);
            foreach(Card c in startingHand)
                hand.Add(c);

            GenerateName();
        }

        public void GenerateName() {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            name = names[rand.Next(names.Length)];
        }
    }

    class Game {
        private List<Player> players = new List<Player>();
        private Deck deck;

        public Game(string playerName, int numAdditionalPlayers) {
            deck = new Deck(true);
            players.Add(new Player(playerName, deck.GetTopCards(2)));
            for(int i = 0; i < numAdditionalPlayers; i++) {
                AIPlayer newPlayer = new AIPlayer(deck.GetTopCards(2));

                // ensure no duplicate names exist
                bool foundDuplicate = false;
                do {
                    foreach(Player p in players) {
                        if(p.name == newPlayer.name) {
                            newPlayer.GenerateName();
                            foundDuplicate = true;
                        }
                    }
                    foundDuplicate = false;
                } while(foundDuplicate);

                players.Add(new AIPlayer(deck.GetTopCards(2)));
            }
            players.Add(new House(deck.GetTopCards(2)));
        }

        // returns t/f depending on player win or not
        public bool Play() {
            //Record.WriteLine("The deck has {0} cards left.", deck.cards.Count);
            //Record.WriteLine("There are {0} players left.\n", GetNumberOfActivePlayers());

            Record.WriteLine("--- House's Hand ---");
            Record.WriteLine("\n\t{0}", players[players.Count - 1].GetHandStatus());

            // iterate thru each player until they bust or stay
            foreach(Player p in players) {
                if(p.isBusted)
                    continue;

                // player gets manual control
                if (p.GetType() == typeof(Player)) {
                    Record.WriteLine("--- It is your turn. ---\n");

                    Record.WriteLine("\t{0}", p.GetHandStatus());

                    string input = "";

                    // hit/stay loop
                    do {
                        Record.Write("\tWould you like to hit (y/n)? ");
                        input = Record.ReadLine().ToLower();

                        if(input == "y") {
                            p.GiveCard(deck.GetTopCard(), true);
                            if(p.isBusted) {
                                do {
                                    Record.Write("\nWould you like to spectate the rest of this game (y/n)? ");
                                    input = Record.ReadLine().ToLower();
                                    if(input == "n") {
                                        return false;
                                    }
                                } while(input != "y");
                                break;
                            }
                        } else if(input == "n") {
                            Record.WriteLine("\tYou decided to stay.");
                        } else {
                            Record.WriteLine("\tInvalid input!\n");
                        }
                    } while(input != "n");
                // computer controller bot.
                // very simple ai.
                // todo: diff difficulties depending on name ?
                } else {
                    bool botHitting = true;
                    Record.WriteLine("\n--- It is {0}'s turn. ---\n", p.name);
                    Record.WriteLine("\t{0}", p.GetHandStatus());
                    do {
                        Record.WriteLine("\t{0} is thinking...", p.name);

                        if(p.GetHandValue(true) > 15) {
                            Record.WriteLine("\t{0} decided to stay.", p.name);
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
                    // only Console is needed as this will be deleted later
                    // no need to output to file
                    // 
                    // after testing, the windows command line seems to need
                    // the cursor position to be one lower than on linux cli
                    Console.Write("\nEnter to Continue.");
                    Console.ReadLine();
                    if(Environment.OSVersion.Platform.ToString() == "Win32NT") {
                        Console.SetCursorPosition(0, Console.CursorTop - 3);
                    } else {
                        Console.SetCursorPosition(0, Console.CursorTop - 2);
                    }
                    Console.Write(new string(' ', Console.WindowWidth));
                }
            }

            // game over

            Record.WriteLine("\n-------------------");
            Record.WriteLine("----- RESULTS -----");
            Record.WriteLine("-------------------\n");

            // house hand
            int houseValue = players[players.Count - 1].GetHandValue(true);
            Record.WriteLine("The House's hand was valued at {0}!\n", houseValue);

            // winners
            var winners = 
                from p in players
                where !p.isBusted && ((p.GetHandValue(true) > houseValue) || (houseValue > 21 && p.name != "The House"))
                select p;

            Record.WriteLine("=== Winners ===");

            if(winners.Count() == 0) {
                Record.WriteLine("None");
            } else {
                foreach(Player p in winners) {
                    Record.WriteLine("{0} (Hand: {1})", p.name, p.GetHandValue(true));
                }
            }

            // busted players
            var bustedPlayers = 
                from p in players
                where p.isBusted == true
                select p;

            Record.WriteLine("\n=== Busted Players ===");

            if(bustedPlayers.Count() == 0) {
                Record.WriteLine("None");
            } else {
                foreach(Player p in bustedPlayers) {
                    Record.WriteLine("{0} (Hand: {1})", p.name, p.GetHandValue(true));
                }
            }

            // other players
            var everyoneElse =
                from p in players
                where !p.isBusted && p.GetHandValue(true) <= houseValue && p.name != "The House" && houseValue <= 21
                select p;
            
            Record.WriteLine("\n=== Everyone Else ===");

            if(everyoneElse.Count() == 0) {
                Record.WriteLine("None");
            } else {
                foreach(Player p in everyoneElse) {
                    Record.WriteLine("{0} (Hand: {1})", p.name, p.GetHandValue(true));
                }
            }

            // check if player won, return tru/false accordingly
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