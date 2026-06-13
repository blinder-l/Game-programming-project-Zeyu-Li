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

## Step 5
Added the basic Blind / Round state management and win-loss checks by introducing `RoundManager` to manage `targetScore`, `currentScore`, `handsRemaining`, and `discardsRemaining`, along with methods such as `HasPassedBlind`, `HasFailedBlind`, `ApplyPlayedHandScore(int score)`, `UseDiscard()`, and `GetDebugStatus()`. This version only implements the core state update and pass/fail logic for a single normal Round and does not yet include keyboard input or a fully playable debug loop. `PrototypeBootstrap` was also updated with hard-coded Round tests to verify passing after reaching the target score, failing after four low-scoring hands, and correctly reducing the remaining discard count after using one discard.

## Step 6
Extended `PrototypeBootstrap` into a minimal playable Console-based debug loop with runtime keyboard controls: number keys `1-8` toggle selection for the corresponding hand cards, `P` plays the selected cards, and `D` discards them. Playing a hand now triggers poker hand evaluation and base scoring, calculates `Chips × Mult`, adds the result to the current Blind score, and reduces the remaining `Hands`; discarding reduces `Discards` and refills the hand. This version allows a full minimal playable Blind loop to be tested in Unity through the Console, outputs `Blind passed.` when the score target is reached, outputs `Blind failed.` when `Hands` are exhausted without success, and stops processing further input after the round ends.

### Stage 3

## Step 1
Extended the base scoring context required for Stage 3 by adding scoring cards, per-card base chip contribution, suit counts, gold reward, and suit effect logs to `ScoreContext`, and introduced the minimal rank chip calculation rules in `ScoreManager` (Two–Nine = 2–9, Ten/Jack/Queen/King = 10, Ace = 11). At the same time, the `High Card` scoring-card selection logic in `PokerHandEvaluator` was adjusted: instead of treating all played cards as scoring cards, it now uses only the highest-valued card as the scoring card for `High Card`, making the behavior more consistent with the intended hand evaluation rule. This version still does not apply suit effects themselves, but it prepares the data pipeline needed to integrate Suit Identity and Suit Mastery into the scoring flow, with rank chips and suit presence verified through Console output in `PrototypeBootstrap`.

## Step 2
Implemented the base suit identity effects by adding `SuitEffectManager` and integrating it into the existing scoring flow. In the current version, Hearts provide a flat `chips` bonus when at least one scoring Heart is present, Spades provide an additional `mult` when a Spade contributes to the scoring hand, Diamonds grant `goldReward` when the number of scoring Diamonds is 1 or 2, and Clubs retrigger the highest base `rank chip` contribution once when at least one scoring Club is present. The triggered results are also written into the effect log, and the base suit effects are verified through Console output in `PrototypeBootstrap`.

## Step 3
Added `SuitMasteryManager` to implement XP and level tracking for the four suits. In the current version, each suit stores its own XP and Level, using fixed thresholds of Lv1 = 3 XP, Lv2 = 6 XP, and Lv3 = 10 XP. The new `AddXpForScoringSuits()` method increases XP by +1 for each suit that participates in the current scoring hand based on the suit count data, while ensuring that the same suit only gains 1 XP per hand even if multiple cards of that suit are present, preserving the anti-Flush rule of gaining XP by participation rather than by card count. `PrototypeBootstrap` was also updated so that after a successful hand is played, XP is awarded according to `scoreContext.suitCounts`, and the Console outputs which suits gained XP as well as the current Level / XP state of all four suits. This step only implements Suit Mastery tracking and does not yet make suit levels strengthen suit effects.

## Step 4
Integrated `Suit Mastery` levels into the four base suit effects so that suit levels now directly affect scoring strength. In the current version, `SuitEffectManager` dynamically adjusts each suit effect based on the current level: Hearts increase from `+10 chips` at Lv0 to `+15 chips` at Lv1, then to `+20 chips` at Lv2/Lv3, with an additional `+0.5 mult` at Lv3 when 2 or more Hearts are present in the scoring hand; Spades increase from `+0.5 mult` at Lv0 to `+0.8 mult` at Lv1, then to `+1.0 mult` at Lv2/Lv3, with an extra `+10 chips` at Lv3; Diamonds increase from `+1 gold` at Lv0 to `+2 gold` at Lv1/Lv2, then to `+3 gold` at Lv3; Clubs keep retriggering the highest `rank chip` at Lv0/Lv1, then gain an extra `+5 chips` at Lv2, and `+5 chips` plus `+0.5 mult` at Lv3. At the same time, `ScoreManager` gained a new `CalculateScore(PokerHandResult, SuitMasteryManager)` overload so scoring can use the current suit mastery levels, while the original `CalculateScore(PokerHandResult)` remains available and defaults to Lv0 behavior. `PrototypeBootstrap` was also updated so that each hand is scored first using the current `SuitMasteryManager`, and only after that are XP gains applied to the suits that participated in scoring. As a result, level-ups from the current hand do not retroactively affect that same hand’s score and instead take effect starting from the next hand. This step was validated in Unity through Console output confirming that higher suit levels correctly strengthen the corresponding suit effects.

## Step 5
Refined and standardized the Console state display and hand resolution logs for Stage 3 by enforcing a fixed output order after each played hand: `Selected Cards`, `Hand Type`, `Score Breakdown`, `Card Chips`, `Scoring Suit Presence`, `Suit Effects`, `Suit Mastery XP Gained`, `Suit Mastery Status`, `Round Status`, and `Next Hand`. `LogCurrentState()` was also updated so that the regular state output now includes the current mastery information for all four suits (Level / XP for Hearts, Diamonds, Clubs, and Spades). This step did not introduce any new gameplay systems, and instead focused on improving the readability, debuggability, and presentability of the existing Suit Identity and Suit Mastery systems for development and coursework review.

 ### Stage 4

## Step 1
Established the foundational Joker framework and connected it to the existing scoring flow as an empty evaluation chain. In the current version, a `JokerBase` abstract class and a `JokerManager` were added, supporting up to five Joker slots, left-to-right Joker resolution order, Joker removal handling, and a hook for Blind-passed notifications. `ScoreContext` was extended with `jokerEffectLog`, and `ScoreManager` gained a Joker-aware scoring overload so that Jokers can be integrated into the scoring pipeline without breaking the existing structure. No concrete Jokers are implemented in this step, so hand resolution only outputs debug information indicating that no Joker effects were triggered. The main purpose of this step is to establish the Joker resolution skeleton while ensuring that `finalScore` is still computed only once after all intermediate effects have been applied.

## Step 2
Implemented the first concrete Joker prototype by adding `SuitRetriggerJoker`, which retriggers the base rank chip contribution of scoring cards that match a specified suit. In the current version, this Joker adds the matching cards’ `rank chips` to `scoreContext.chips` one extra time, but does not retrigger base hand scoring, suit effects, or other Jokers, and does not directly modify `finalScore`. `PrototypeBootstrap` was also updated with temporary debug equip controls, allowing the four suit retrigger Jokers to be equipped via `F1-F4`, and Unity Console output was used to verify Joker list updates, Joker effect logs, and that the final score is still consistently calculated from `chips × mult`.

## Step 3
Added two more Joker prototypes to expand the multiplier-based Joker system in Stage 4. `HighRiskMultiplierJoker` multiplies the current `mult` by 5 whenever scoring occurs, and after each trigger it has a `1/20` chance to break and be removed from the Joker slots; `StoredDiscardMultiplierJoker` starts with `storedMultiplier = 1`, multiplies the current `mult` by that stored value whenever a hand is scored, and increases `storedMultiplier` by the remaining `discards` whenever a Blind is passed. `PrototypeBootstrap` was also updated with temporary debug equip keys `F5` and `F6`, and now notifies Jokers when a Blind has been passed so Blind-passed effects can be processed. This step was verified in Unity Play Mode through Console logs showing multiplier Joker activation, break/removal behavior, and stored multiplier growth after a Blind pass, and the solution was additionally confirmed to build successfully with `dotnet build` with 0 warnings and 0 errors.

## Step 4
Added a minimal multi-Blind progression system, allowing the prototype to advance from “ending after one passed Blind” to “manually starting the next Blind.” A new `RunManager` was introduced to manage the current Blind number and target score progression, using a simple rule of `Blind 1 = 300` and `+200` for each subsequent Blind. At the same time, `PrototypeBootstrap` was updated so that after passing a Blind, the Console prompts the player to press `N` to start the next Blind. Pressing `N` rebuilds the deck and hand for the new Blind and starts a fresh Round while preserving the currently equipped Jokers and the existing Suit Mastery state. This step was manually verified in Unity Play Mode to confirm the basic multi-Blind progression flow, and the solution was additionally confirmed to build successfully with `dotnet build` with 0 warnings and 0 errors.

## Step 5
Added a minimal shop loop that allows the player to enter a Console-based Joker shop after passing a Blind. A new `ShopManager` was introduced to generate 3 random Joker shop offers, expose the current offers, remove purchased offers, and output shop-related debug text. At the same time, `PrototypeBootstrap` was updated so that after a Blind is passed, the game automatically enters the shop state; in the shop, the player can press `1-3` to purchase a corresponding Joker and move on to the next Blind, or press `N` to skip the shop and go directly to the next Blind. In the current version, shop purchases are still only part of the minimal loop validation and do not yet consume Gold, and there is still no reroll, Boss, Ante, Tarot, Planet, Spectral, Voucher, or other later-stage systems. This step was manually verified in Unity Play Mode to confirm shop display, Joker purchase, shop skipping, and that equipped Jokers persist and continue to function in the next Blind, and the solution was additionally confirmed to build successfully with `dotnet build` with 0 warnings and 0 errors.

## Step 6
Integrated Gold into the minimal shop purchase loop, making Diamonds-generated `goldReward` a truly spendable resource for the first time. In the current version, `PrototypeBootstrap` now includes `currentGold` as the run-level Gold state for the prototype and uses a starting Gold value of 10 to make first-shop verification easier. After each scored hand, `scoreContext.goldReward` is added to the current Gold, and the current Gold is displayed in normal status output, hand resolution logs, and the shop view. In the shop, purchasing a Joker now checks whether `currentGold >= joker.Cost`; if enough Gold is available, the purchase succeeds, the corresponding Gold is deducted, and the game proceeds to the next Blind. If there is not enough Gold, the system outputs `Not enough Gold...` and remains in the shop without buying the Joker or advancing to the next Blind. This step only introduces the minimal Gold-to-Joker purchase connection and does not yet include rerolls, Bosses, other shop item types, or a more complex economy system. It was manually verified in Unity Play Mode by checking Gold display, Diamonds Gold Reward accumulation, shop purchase deduction, and correct stay-in-shop behavior when Gold is insufficient, and the solution was additionally confirmed to build successfully with `dotnet build` with 0 warnings and 0 errors.

## Step 7
Added Blind clear Gold rewards to complete the basic Stage 4 economy loop. In addition to the existing immediate `goldReward` gained after each scored hand, the prototype now awards interest based on current Gold and Gold from remaining discards when a Blind is passed. Interest is calculated as `min(currentGold / 5, 5)`, remaining discard Gold is equal to `roundManager.discardsRemaining`, and both are added to `currentGold` before entering the shop. Console logs clearly show Interest, Remaining Discards Gold, End of Blind Gold Total, and Current Gold.

## Step 8
Improved the minimal shop interaction loop so the player can stay in the same shop after buying a Joker, purchase multiple Jokers if Gold and slots allow, reroll the shop for 2 Gold, and only advance to the next Blind by pressing `N`. The shop now displays current Gold, Joker slot usage, current shop options, purchase results, reroll results, and the leave-shop prompt. This remains a Stage 4 Console-only Joker shop and does not add Bosses, UI, other shop item types, or later-stage systems.

## Step 9
Added two Console hand-sorting controls for the Blind state to make the current hand easier to inspect during play. Pressing `S` sorts the current hand by suit, grouping cards as Hearts, Spades, Diamonds, and Clubs, while pressing `T` sorts the current hand by rank from high to low. These sorting actions only change the display/order of cards in the current hand and do not play, discard, draw, score, or otherwise mutate gameplay resources. Existing selected-card markers remain attached to their cards after sorting, allowing the player to reorganize the hand visually without losing selection state.

### Stage 5

## Step 1
Added Ante and Blind display helpers to `RunManager` as the first UI-support step for Stage 5. The run now exposes read-only methods for the current Ante number, Blind position within the Ante, Blind display name, Boss Blind check, and fixed Blind reward. Each Ante contains three Blinds in order: Small Blind, Big Blind, and Boss Blind. Boss Blinds do not yet have special gameplay rules, but they now display correctly and provide a larger fixed CashOut reward for future UI binding.

## Step 2
Added the first UI state controller layer for Stage 5. A new `GameUIState` enum defines `PlayingBlind`, `CashOut`, `Shop`, and `RunFailed`, while `GameUIController` manages the active state of the main UI roots including PlayStateRoot, CashOutPanel, ShopPanel, DeckStatsPanel, CurrentHandStatsPanel, CardTooltipPanel, and ActionButtonsContainer. `PrototypeBootstrap` now optionally notifies the UI controller when entering PlayingBlind, Shop, or RunFailed states, while remaining null-safe so the existing Console flow still works before all UI references are assigned.

## Step 3
Added the first hand display binding layer for Stage 5. `CardSpriteDatabase` maps a `PlayingCard` by `Suit` and `Rank` to the imported card sprites using the existing `Assets/Spirites/Cards` naming pattern, while `HandCardView` renders a single hand slot with a card sprite, fallback card name text, and selected-card visual offset. `GameUIController` can now refresh the eight hand card views from the real `HandManager.CurrentHand`, and `PrototypeBootstrap` calls this refresh after starting a Blind, selecting cards, sorting the hand, playing cards, or discarding cards. This step only updates display state and does not add UI click handling, button actions, scoring changes, shop changes, CashOut logic, or tooltip behavior.
