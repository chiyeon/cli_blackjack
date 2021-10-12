# cli_blackjack
cli_blackjack is a simple console based blackjack game in c#
made for GDIM31

### about
cli_blackjack is a singleplayer game that supports up to 5 computer controlled players besides the House, for a total of 7 including the player

there is no betting, only choosing to hit/pass

the player's inputs and the game's console outputs are all recorded to a local file ./blackjack_record.txt
included is a record for 20 games/rounds of play

### to compile & run in one line on linux
**required:**
 - mcs
 - mono

**command:**

`mcs -out:blackjack.exe blackjack.cs && mono blackjack.exe`
