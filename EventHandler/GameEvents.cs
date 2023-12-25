﻿using RadiantConnect.EventHandler.Events;

namespace RadiantConnect.EventHandler
{
    public class GameEvents
    {
        public QueueEvents Queue = new();
        public PreGameEvents PreGame = new();
        public MatchEvents Match = new();
        public RoundEvents Round = new();
        public VoteEvents Vote = new();
        public InGameEvents InGame = new();

        internal string LastEventCall = "";
        internal long LastLineRead;
        
        internal void HandleEvent(Action<string, string> eventAction, string logInvoker, string log, long lineIndex)
        {
            if (LastEventCall == logInvoker && !(logInvoker.Equals("Party_ChangeQueue") || logInvoker.Equals("Pregame_SelectCharacter"))) return;
            eventAction.Invoke(logInvoker, log);
            LastEventCall = logInvoker;
            LastLineRead = lineIndex;
        }

        internal void ParseLogText(string logText)
        {
            string[] fileLines = logText.Split('\n');

            for (long lineIndex = fileLines.Length - 1; lineIndex > LastLineRead; lineIndex--)
            {
                string line = fileLines[lineIndex];

                if (line.Contains("Log file closed")) break;

                if (LastLineRead == lineIndex) break;

                switch (line)
                {
                    case var _ when line.Contains("Party_ChangeQueue"):
                        HandleEvent(Queue.HandleQueueEvent, "Party_ChangeQueue", line, lineIndex);
                        break;
                    case var _ when line.Contains("Party_EnterMatchmakingQueue"):
                        HandleEvent(Queue.HandleQueueEvent, "Party_EnterMatchmakingQueue", line, lineIndex);
                        break;
                    case var _ when line.Contains("Party_LeaveMatchmakingQueue"):
                        HandleEvent(Queue.HandleQueueEvent, "Party_LeaveMatchmakingQueue", line, lineIndex);
                        break;
                    case var _ when line.Contains("Party_MakePartyIntoCustomGame"):
                        HandleEvent(Queue.HandleQueueEvent, "Party_MakePartyIntoCustomGame", line, lineIndex);
                        break;

                    case var _ when line.Contains("Pregame_GetPlayer"):
                        HandleEvent(PreGame.HandlePreGameEvents, "Pregame_GetPlayer", line, lineIndex);
                        break;
                    case var _ when line.Contains("Pregame_GetMatch"):
                        HandleEvent(PreGame.HandlePreGameEvents, "Pregame_GetMatch", line, lineIndex);
                        break;
                    case var _ when line.Contains("Pregame_LockCharacter"):
                        HandleEvent(PreGame.HandlePreGameEvents, "Pregame_LockCharacter", line, lineIndex);
                        break;
                    case var _ when line.Contains("Pregame_SelectCharacter"):
                        HandleEvent(PreGame.HandlePreGameEvents, "Pregame_SelectCharacter", line, lineIndex);
                        break;

                    case var _ when line.Contains("LogMapLoadModel: Update: [Map Name: ") && line.Contains("| Changed: FALSE] [Local World: TRUE | Changed: FALSE] [Match Setup: TRUE | Changed: TRUE] [Map Ready: FALSE | Changed: FALSE] [Map Complete: FALSE | Changed: FALSE]"):
                        HandleEvent(Match.HandleMatchEvent, "Map_Loaded", line, lineIndex);
                        break;
                    case var _ when line.Contains("Match Ended: Completion State:"):
                        Round.ResetRound();
                        HandleEvent(Match.HandleMatchEvent, "Match_Ended", line, lineIndex);
                        break;
                    case var _ when line.Contains("LogLoadTimeMetrics: Display: [Match Load Times] LoadingScreenTime: "):
                        Round.ResetRound();
                        HandleEvent(Match.HandleMatchEvent, "Match_Started", line, lineIndex);
                        break;

                    case var _ when line.Contains("AShooterGameState::OnRoundEnded"):
                        HandleEvent(Round.HandleRoundEvent, "Round_Ended", line, lineIndex);
                        break;
                    case var _ when line.Contains("Gameplay started at local time"):
                        HandleEvent(Round.HandleRoundEvent, "Round_Started", line, lineIndex);
                        break;

                    case var _ when line.Contains("LogVoteControllerComponent: Setting vote input bindings enabled to 1"):
                        HandleEvent(Vote.HandleVoteEvent, "Vote_Called", line, lineIndex);
                        break;
                    case var _ when line.Contains("LogVoteControllerComponent: Making vote request for option "):
                        HandleEvent(Vote.HandleVoteEvent, "Vote_Invoked", line, lineIndex);
                        break;

                    case var _ when line.Contains("LogMenuStackManager: Opening preRound"):
                        HandleEvent(InGame.HandleInGameEvent, "Buy_Menu_Opened", line, lineIndex);
                        break;
                    case var _ when line.Contains("LogMenuStackManager: Closing preRound"):
                        HandleEvent(InGame.HandleInGameEvent, "Buy_Menu_Closed", line, lineIndex);
                        break;
                    case var _ when line.Contains("LogNet: Warning: UNetDriver::ProcessRemoteFunction: No owning connection for actor"):
                        HandleEvent(InGame.HandleInGameEvent, "Util_Placed", line, lineIndex);
                        break;
                }
            }
        }
    }
}