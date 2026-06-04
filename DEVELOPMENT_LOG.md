# Development Log

### Stage 1

## Step 1

Implemented Suit and Rank enums as the foundation of the card system.



## Step 2

Implemented the PlayingCard data model with core card fields and debug display output.



## Step 3

Implemented the standard 52-card deck generation in `DeckManager`, ensuring all four suits and thirteen ranks are correctly combined, and established the base deck structure for future shuffle and draw logic.

 ## Step 4
Added `PrototypeBootstrap` as a minimal debug entry point to initialize the deck at startup and print basic validation information in the Unity Console for incremental testing.

## Step 5
Implemented shuffle and draw logic in `DeckManager`, supporting single-card and multi-card drawing from the draw pile, adding basic safety handling when the deck has insufficient cards, and verifying shuffle behavior and draw pile count changes through debug output.

## Step 6
Implemented the `HandManager` hand container, supporting storage of the current hand and refilling up to the maximum hand size of 8 cards, establishing the foundation for future selection, play, and discard interactions, and verifying the initial hand contents and remaining draw pile count through debug output.

## Step 7
Implemented a minimal selection, play, and discard debug flow, supporting card selection toggling, retrieval of selected cards, play/discard actions, refilling the hand to the maximum size, updating basic history counters, and verifying the full loop through Console output.

### Stage 2

## Step 1
Established the foundational data structures for poker hand evaluation by adding the `PokerHandType` enum, the `PokerHandResult` data class, and the `PokerHandEvaluator` skeleton, with a minimal implementation that consistently returns `High Card` as the first working evaluation path.

 ## Step 2
Implemented rank-count-based poker hand detection in `PokerHandEvaluator`, supporting `Pair`, `Two Pair`, `Three of a Kind`, `Full House`, and `Four of a Kind`, while keeping `High Card` as the fallback result, and verified the correctness of each rank-pattern case through hard-coded test hands in the Console.

## Step 3
Extended `PokerHandEvaluator` to detect `Straight`, `Flush`, and `Straight Flush`, adding the helper methods `IsStraight()` and `IsFlush()` and establishing the full hand priority order: `StraightFlush > FourOfAKind > FullHouse > Flush > Straight > ThreeOfAKind > TwoPair > Pair > HighCard`. In this version, `Ace` is only treated as the highest rank, and the `A-2-3-4-5` low straight is not implemented yet. `PrototypeBootstrap` was also updated with Console tests for `Straight`, `Flush`, and `Straight Flush` to verify the new hand detection logic.

## Step 4
Added the base scoring system by introducing `ScoreContext` to store the current play’s `playedCards`, `handType`, `chips`, `mult`, and `finalScore`, and adding `ScoreManager` to return base `chips` and `mult` from the detected hand type and calculate `finalScore = chips × mult`. This version only implements base hand scoring and does not yet include individual card rank bonuses, suit effects, or Joker modifiers. `PrototypeBootstrap` was also updated to pass `PokerHandResult` into `ScoreManager` and print the current hand type, `Chips`, `Mult`, and `Final Score` to the Console, with an additional `Pair` scoring test to verify that the base scoring output is correct.

